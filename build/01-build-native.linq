<Query Kind="Program">
  <NuGetReference>SharpZipLib</NuGetReference>
  <NuGetReference>System.Linq.Async</NuGetReference>
  <Namespace>ICSharpCode.SharpZipLib.Tar</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\00-common"

static string NativeVersion => Projects.First(x => x.name == "Sdcb.Math.Gmp").version;

async Task Main()
{
	await SetupAsync(QueryCancelToken);
	await new WindowsNugetSource("win-x64", "win64", "gmp.dll", @"C:\_\3rd\vcpkg\packages\gmp_x64-windows\bin")
		.Process(QueryCancelToken);
}

static string BuildNuspec(string[] libs, string rid, string titleRid)
{
	// props
	{
		XDocument props = XDocument.Parse(File
			.ReadAllText("./Sdcb.Math.Gmp.runtime.props"));
		string ns = props.Root.GetDefaultNamespace().NamespaceName;
		XmlNamespaceManager nsr = new(new NameTable());
		nsr.AddNamespace("p", ns);

		string platform = rid.Split("-").Last();

		XElement itemGroup = props.XPathSelectElement("/p:Project/p:ItemGroup", nsr);
		itemGroup.Add(
		libs
			.Select(x => Path.GetFileName(x))
			.Select(x => new XElement(XName.Get("Content", ns), new XAttribute("Include", $@"$(SdcbGmpDlls)\{rid}\native\{x}"),
				new XElement(XName.Get("Link", ns), @$"dll\{platform}\{x}"),
				new XElement(XName.Get("CopyToOutputDirectory", ns), "PreserveNewest")))
				);
		props.Save(@$"./{titleRid}/Sdcb.Math.Gmp.runtime.{titleRid}.props");
	}

	// nuspec
	{
		XDocument nuspec = XDocument.Parse(File
			.ReadAllText("./Sdcb.Math.Gmp.runtime.nuspec")
			.Replace("{rid}", rid)
			.Replace("{titleRid}", titleRid));

		string ns = nuspec.Root.GetDefaultNamespace().NamespaceName;
		XmlNamespaceManager nsr = new(new NameTable());
		nsr.AddNamespace("p", ns);

		XElement files = nuspec.XPathSelectElement("/p:package/p:files", nsr);
		files.Add(libs.Select(x => new XElement(
			XName.Get("file", ns),
			new XAttribute("src", x.Replace($@"{titleRid}\", "")),
			new XAttribute("target", @$"runtimes\{rid}\native"))));
		files.Add(new[] { "net", "netstandard", "netcoreapp" }.Select(x => new XElement(
			XName.Get("file", ns),
			new XAttribute("src", $"Sdcb.Math.Gmp.runtime.{titleRid}.props"),
			new XAttribute("target", @$"build\{x}\Sdcb.Math.Gmp.runtime.{titleRid}.props"))));

		string destinationFile = @$"./{titleRid}/{rid}.nuspec";
		nuspec.Save(destinationFile);
		return destinationFile;
	}
}

public record WindowsNugetSource(string rid, string titleRid, string libName, string folder) : NupkgBuildSource(rid, titleRid, libName, folder)
{
	protected override async Task Decompress(string folder, CancellationToken cancellationToken)
	{
		foreach (string entry in Directory.EnumerateFiles(folder).Where(x => Path.GetExtension(x) switch
		{
			".dll" => true, 
			".pdb" => true, 
			_ => false, 
		}))
		{
			string localEntryDest = Path.Combine(PlatformDir, Path.GetFileName(entry));
			Console.Write($"Expand {entry} -> {localEntryDest}... ");
			File.Copy(entry, localEntryDest);
			Console.WriteLine("Done");
		}
	}

	protected override string[] GetDlls()
	{
		return Directory.EnumerateFiles(PlatformDir)
			.Where(x => Path.GetExtension(x) switch
			{
				".dll" => true,
				".pdb" => true,
				_ => false,
			})
			.Select(f => f.Replace(rid + @"\", ""))
			.ToArray();
	}
}

public abstract record NupkgBuildSource(string rid, string titleRid, string libName, string folder)
{
	public string CLibFilePath => $@"./{titleRid}/bin/{libName}";
	public string PlatformDir => Path.GetDirectoryName(CLibFilePath);
	public string NuGetPath => $@".\nupkgs\Sdcb.Math.Gmp.runtime.{titleRid}.{NativeVersion}.nupkg";

	protected abstract Task Decompress(string localZipFile, CancellationToken cancellationToken);
	protected abstract string[] GetDlls();

	public async Task Process(CancellationToken cancellationToken)
	{
		Console.WriteLine($"processing {titleRid}...");
		if (!File.Exists(CLibFilePath))
		{
			await Decompress(folder, cancellationToken);
		}
		string[] libs = GetDlls();

		string nugetExePath = await EnsureNugetExe(cancellationToken);

		string nuspecPath = BuildNuspec(libs, rid, titleRid);
		if (!File.Exists(NuGetPath))
		{
			string iconDestPath = @$".\{titleRid}\icon.jpg";
			if (!File.Exists(iconDestPath)) File.Copy(@$".\icon.jpg", iconDestPath);
			NuGetRun($@"pack {nuspecPath} -Version {NativeVersion} -OutputDirectory .\nupkgs".Dump());
		}
		else
		{
			Util.HorizontalRun(false, Util.Metatext(NuGetPath), " exists, skip.").Dump();
		}
	}
}