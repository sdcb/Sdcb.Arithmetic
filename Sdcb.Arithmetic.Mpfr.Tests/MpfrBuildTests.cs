using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class MpfrBuildTests
{
    private readonly ITestOutputHelper _console;

    public MpfrBuildTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void VersionTest()
    {
        Assert.NotNull(MpfrBuild.Version); // "4.2.0"
        _console.WriteLine(MpfrBuild.Version);
    }

    [Fact]
    public void PatchesTest()
    {
        Assert.NotNull(MpfrBuild.Patches); // ""
        _console.WriteLine(MpfrBuild.Patches);
    }

    [Fact]
    public void BuildOptTlsTest()
    {
        _console.WriteLine(MpfrBuild.HasThreadLocalStorage.ToString());
    }

    [Fact]
    public void AllBuildOpts()
    {
        _console.WriteLine($"HasThreadLocalStorage: {MpfrBuild.HasThreadLocalStorage}");
        _console.WriteLine($"HasFloat128: {MpfrBuild.HasFloat128}");
        _console.WriteLine($"HasDecimal: {MpfrBuild.HasDecimal}");
        _console.WriteLine($"HasGmpInternals: {MpfrBuild.HasGmpInternals}");
        _console.WriteLine($"HasSharedCache: {MpfrBuild.HasSharedCache}");
        _console.WriteLine($"TuneCase: {MpfrBuild.TuneCase}");
    }
}
