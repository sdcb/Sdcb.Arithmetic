<Query Kind="Program">
  <NuGetReference>Octokit</NuGetReference>
  <Namespace>Octokit</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

#load ".\00-common"

async Task Main()
{
	await SetupAsync(QueryCancelToken);
	string folder = await DownloadAllArtifactsAndExtract(@"C:\Users\ZhouJie\Downloads\artifacts");
	await Parallel.ForEachAsync(GetSources(folder), async (s, ct) =>
	{
		await s.Process(QueryCancelToken);
	});
}

IEnumerable<WindowsNugetSource> GetSources(string folder)
{
	string sharedPrefix = "shared-";
	return Directory.EnumerateDirectories(folder)
		.Select(x => (path: x, name: Path.GetFileName(x)))
		.Where(x => x.name.StartsWith(sharedPrefix))
		.SelectMany(x =>
		{
			string rid = x.name[sharedPrefix.Length..];
			string title = rid;
			string binPath = Path.Combine(x.path, "bin");
			string gmpPath = Directory.EnumerateFiles(binPath, "*gmp*")
				.Where(x => !x.Contains("gmpxx"))
				.First();
			string mpfrPath = Directory.EnumerateFiles(binPath, "*mpfr*").First();
			return new WindowsNugetSource[]
			{
				//new(rid, title, new LibNames(Path.GetFileName(gmpPath)), binPath, "Sdcb.Arithmetic.Gmp", []),
				new(rid, title, new LibNames(Path.GetFileName(mpfrPath)), binPath, "Sdcb.Arithmetic.Mpfr", ["Sdcb.Arithmetic.Gmp"]),
			};
		});
}

static string BuildNuspec(string[] libs, string rid, string titleRid, string folder, string pkgName, string[] deps)
{
	// props
	{
		XDocument props = XDocument.Parse($"""
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<PropertyGroup>
	</PropertyGroup>
	<ItemGroup Condition="$(TargetFrameworkVersion.StartsWith('v4')) Or $(TargetFramework.StartsWith('net4'))">
	</ItemGroup>
</Project>
""");
		string normalizedName = pkgName.Replace(".", "") + "Dlls";
		string ns = props.Root.GetDefaultNamespace().NamespaceName;
		XmlNamespaceManager nsr = new(new NameTable());
		nsr.AddNamespace("p", ns);

		string platform = rid.Split("-").Last();

		XElement? itemGroup = props.XPathSelectElement("/p:Project/p:ItemGroup", nsr);
		if (itemGroup == null) throw new Exception($"{nameof(itemGroup)} invalid in props file.");
		itemGroup.Add(
		libs
			.Select(x => Path.GetFileName(x))
			.Select(x =>
				new XElement(XName.Get("Content", ns), new XAttribute("Include", $@"$({normalizedName})\{rid}\native\{x}"),
					new XElement(XName.Get("Link", ns), @$"dll\{platform}\{x}"),
					new XElement(XName.Get("CopyToOutputDirectory", ns), "PreserveNewest")))
				);

		XElement? propGroup = props.XPathSelectElement("/p:Project/p:PropertyGroup", nsr);
		if (propGroup == null) throw new Exception($"{nameof(propGroup)} invalid in props file.");
		propGroup.Add(new XElement(XName.Get(normalizedName, ns), @"$(MSBuildThisFileDirectory)..\..\runtimes"));
		props.Save(Path.Combine(folder, $"{titleRid}.props"));
	}

	// nuspec
	{
		string masterName = pkgName;
		XDocument nuspec = XDocument.Parse($"""
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
    <metadata>
        <id>{pkgName}.runtime.{titleRid}</id>
        <version>2.2.1</version>
        <title>{masterName} native bindings for {titleRid}</title>
        <authors>sdcb</authors>
        <license type="expression">LGPL-3.0-or-later</license>
        <projectUrl>https://github.com/sdcb/{pkgName}</projectUrl>
        <icon>images\icon.png</icon>
        <requireLicenseAcceptance>true</requireLicenseAcceptance>
        <description>Native binding for {masterName} to work on {titleRid}.</description>
        <summary>Native binding for {masterName} to work on {titleRid}.</summary>
        <releaseNotes></releaseNotes>
        <copyright>Copyright {DateTime.Now.Year}</copyright>
        <tags>GMP;BigInteger;BigRational;BigFloat;GmpInteger;GmpRational;GmpFloat;GmpRandom;MPFR;MpfrFloat;multi-precision;arithmetic;linqpad-samples</tags>
        <repository type="git" url="https://github.com/sdcb/{pkgName}.git" />
        <dependencies>
        </dependencies>
        <frameworkAssemblies>
        </frameworkAssemblies>
    </metadata>
    <files>
        <file src="..\{pkgName}.png" target="images\icon.png" />
    </files>
</package>
""");

		string ns = nuspec.Root.GetDefaultNamespace().NamespaceName;
		XmlNamespaceManager nsr = new(new NameTable());
		nsr.AddNamespace("p", ns);

		XElement? files = nuspec.XPathSelectElement("/p:package/p:files", nsr);
		if (files == null) throw new Exception($"{nameof(files)} invalid in nuspec file.");
		files.Add(libs.Select(x => new XElement(
			XName.Get("file", ns),
			new XAttribute("src", x.Split('\\', 2)[1]),
			new XAttribute("target", @$"runtimes\{rid}\native"))));
		files.Add(new[] { "net", "netstandard", "netcoreapp" }.Select(x => new XElement(
			XName.Get("file", ns),
			new XAttribute("src", $"{titleRid}.props"),
			new XAttribute("target", @$"build\{x}\{pkgName}.runtime.{titleRid}.props"))));

		if (deps.Any())
		{
			XElement? dependencies = nuspec.XPathSelectElement("/p:package/p:metadata/p:dependencies", nsr);
			if (dependencies == null) throw new Exception($"{nameof(dependencies)} invalid in nuspec file.");
			dependencies.Add(deps.Select(depId => new XElement(XName.Get("dependency", ns),
				new XAttribute("id", $"{depId}.runtime.{titleRid}"),
				new XAttribute("version", Projects.First(x => x.name == depId).version))));
		}

		string destinationFile = Path.Combine(folder, $"{titleRid}.nuspec");
		nuspec.Save(destinationFile);
		return destinationFile;
	}
}

public record WindowsNugetSource(string rid, string titleRid, LibNames libNames, string url, string pkgName, string[] deps) : NupkgBuildSource(rid, titleRid, libNames, url, pkgName, deps)
{
	protected override async Task Decompress(CancellationToken cancellationToken)
	{
		Directory.CreateDirectory(CLibFolder);
		if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
		{
			using HttpClient http = new HttpClient();
			Stream compressed = await http.GetStreamAsync(url);
			using ZipArchive zip = new ZipArchive(compressed);

			foreach (ZipArchiveEntry entry in zip.Entries.Where(x => libNames.Contains(x.Name)))
			{
				if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

				string localEntryDest = Path.Combine(CLibFolder, entry.Name);
				Console.Write($"Expand {entry} -> {localEntryDest}... ");
				entry.ExtractToFile(localEntryDest, overwrite: true);
			}
		}
		else if (Directory.Exists(url))
		{
			foreach (string libName in libNames.All)
			{
				if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

				string srcFile = Path.Combine(url, libName);
				string destFile = Path.Combine(CLibFolder, libName);
				if (!File.Exists(srcFile) && libName != libNames.Primary)
				{
					Console.WriteLine($"{srcFile} not exists, skip... ");
				}
				else
				{
					Console.WriteLine($"Copy {srcFile} to {destFile}... ");
					File.Copy(srcFile, destFile, overwrite: true);
				}
			}
		}
		Console.WriteLine("Done");
	}
}

public abstract record NupkgBuildSource(string rid, string titleRid, LibNames libNames, string url, string pkgName, string[] deps)
{
	public string CLibFilePath => Path.Combine(CLibFolder, libNames.Primary);
	public string CLibFolder => Path.Combine(RidFolder, "bin");
	public string RidFolder => $@"{titleRid}-{Path.GetFileNameWithoutExtension(libNames.Primary)}";
	public string Version => Projects.First(x => x.name == pkgName).version;
	public string NuGetPath => Path.Combine("nupkgs", $"{pkgName}.runtime.{titleRid}.{Version}.nupkg");

	protected abstract Task Decompress(CancellationToken cancellationToken);
	protected string[] GetDlls()
	{
		return Directory.EnumerateFiles(CLibFolder)
			.Where(x => Path.GetExtension(x) switch
			{
				".dll" => true,
				".dylib" => true, 
				_ => false,
			} || x.Contains(".so"))
			.Select(f => f.Replace(rid + @"\", ""))
			.ToArray();
	}

	public async Task Process(CancellationToken cancellationToken)
	{
		Console.WriteLine($"processing {titleRid}...");
		if (File.Exists(CLibFilePath))
		{
			Console.WriteLine($"{CLibFilePath} exists, override!");
		}
		await Decompress(cancellationToken);

		string[] libs = GetDlls();

		string nugetExePath = await EnsureNugetExe(cancellationToken);

		string nugetTempFolder = Path.GetDirectoryName(CLibFilePath)!;
		Directory.CreateDirectory(nugetTempFolder);
		string nuspecPath = BuildNuspec(libs, rid, titleRid, RidFolder, pkgName, deps);
		if (File.Exists(NuGetPath))
		{
			Console.WriteLine($"{NuGetPath} exists, override!");
		}

		NuGetRun($@"pack {nuspecPath} -Version {Version} -OutputDirectory .\nupkgs".Dump());
		System.IO.Directory.Delete(nugetTempFolder, true);
	}
}

public class LibNames
{
	public string Primary { get; }
	public IReadOnlyCollection<string> All { get; }

	public LibNames(params string[] libNames)
	{
		if (libNames.Length == 0) throw new ArgumentException(nameof(libNames));

		Primary = libNames[0];
		All = libNames.ToHashSet();
	}

	public bool Contains(string name) => All.Contains(name);
	
	public object ToDump() => string.Join(", ", All);
}

async Task<string> DownloadAllArtifactsAndExtract(string folder)
{
	GitHubClient client = new(new ProductHeaderValue("MyApp"));
	client.Credentials = new Credentials(Util.GetPassword("sdcb-arithmetic-github-token"));

	var workflowRuns = await client.Actions.Workflows.Runs.ListByWorkflow("sdcb", "Sdcb.Arithmetic", "build-all.yml", new WorkflowRunsRequest()
	{
		Status = new StringEnum<CheckRunStatusFilter>(CheckRunStatusFilter.Success),
		Branch = "master",
	}, new ApiOptions()
	{
		PageCount = 1,
		PageSize = 1,
	});

	if (workflowRuns.WorkflowRuns.Count == 0)
	{
		throw new InvalidOperationException("No successful workflow runs found.");
	}

	long runId = workflowRuns.WorkflowRuns[0].Id;
	Console.WriteLine($"Found successful workflow run with ID: {runId}");

	ListArtifactsResponse artifacts = await client.Actions.Artifacts.ListWorkflowArtifacts("sdcb", "Sdcb.Arithmetic", runId);

	if (artifacts.Artifacts.Count == 0)
	{
		throw new InvalidOperationException("No artifacts found for the workflow run.");
	}

	Console.WriteLine($"Found {artifacts.Artifacts.Count} artifact(s). Starting download...");

	string runFolder = Path.Combine(folder, runId.ToString());
	await Parallel.ForEachAsync(artifacts.Artifacts, new ParallelOptions()
	{
		MaxDegreeOfParallelism = 8,
	}, async (artifact, cancellationToken) =>
	{
		string artifactFolder = Path.Combine(runFolder, artifact.Name);
		if (Directory.Exists(artifactFolder))
		{
			Console.WriteLine($"Artifact folder {artifactFolder} already exists. Skipping extraction...");
		}
		else
		{
			Console.WriteLine($"Downloading artifact: {artifact.Name} (ID: {artifact.Id})...");
			Stream response = await client.Actions.Artifacts.DownloadArtifact("sdcb", "Sdcb.Arithmetic", artifact.Id, "zip");

			Directory.CreateDirectory(artifactFolder);
			using (ZipArchive archive = new(response, ZipArchiveMode.Read))
			{
				archive.ExtractToDirectory(artifactFolder);
			}
			Console.WriteLine($"Artifact {artifact.Name} downloaded and saved to {artifactFolder}.");
		}
	});

	return runFolder;
}