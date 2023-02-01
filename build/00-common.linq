<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

async Task Main()
{
	await SetupAsync(QueryCancelToken);
}

async Task SetupAsync(CancellationToken cancellationToken = default)
{
	Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
	Environment.CurrentDirectory = Util.CurrentQuery.Location;
	await EnsureNugetExe(cancellationToken);
}

static void NuGetRun(string args) => Run(@".\nuget.exe", args, Encoding.GetEncoding("gb2312"));
static void DotNetRun(string args) => Run("dotnet", args, Encoding.GetEncoding("utf-8"));
static void Run(string exe, string args, Encoding encoding) => Util.Cmd(exe, args, encoding);
static ProjectVersion[] Projects = new[]
{
	new ProjectVersion("Sdcb.Arithmetic.Gmp", "1.0.10-preview.12"), 
};

static async Task DownloadFile(Uri uri, string localFile, CancellationToken cancellationToken = default)
{
	using HttpClient http = new();

	HttpResponseMessage resp = await http.GetAsync(uri, cancellationToken);
	if (!resp.IsSuccessStatusCode)
	{
		throw new Exception($"Failed to download: {uri}, status code: {(int)resp.StatusCode}({resp.StatusCode})");
	}

	using (FileStream file = File.OpenWrite(localFile))
	{
		await resp.Content.CopyToAsync(file, cancellationToken);
	}
}

static async Task<string> EnsureNugetExe(CancellationToken cancellationToken = default)
{
	Uri uri = new Uri(@"https://dist.nuget.org/win-x86-commandline/latest/nuget.exe");
	string localPath = @".\nuget.exe";
	if (!File.Exists(localPath))
	{
		await DownloadFile(uri, localPath, cancellationToken);
	}
	return localPath;
}

record ProjectVersion(string name, string version);