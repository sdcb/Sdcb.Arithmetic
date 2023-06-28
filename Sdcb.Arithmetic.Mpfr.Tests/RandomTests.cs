using Sdcb.Arithmetic.Gmp;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class RandomTests
{
#pragma warning disable IDE0052 // 删除未读的私有成员
    private readonly ITestOutputHelper _console;
#pragma warning restore IDE0052 // 删除未读的私有成员

    public RandomTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void NextMpfrFloatTest()
    {
        using GmpRandom r = new(0);
        using MpfrFloat f = r.NextMpfrFloat(precision: 100);
        Assert.True(f >= 0);
        Assert.True(f < 1);
    }

    [Fact]
    public void NextMpfrFloatRoundTest()
    {
        using GmpRandom r = new(0);
        using MpfrFloat f = r.NextMpfrFloatRound(precision: 100);
        Assert.True(f >= 0);
        Assert.True(f < 1);
    }

    [Fact]
    public void NextNormalDistributedMpfrFloatTest()
    {
        using GmpRandom r = new();
        using MpfrFloat f = r.NextNMpfrFloat(precision: 100);
        Assert.True(f < 7.0);
    }

    [Fact, Obsolete]
    public void Next2NormalDistributedMpfrFloatTest()
    {
        using GmpRandom r = new();
        (MpfrFloat f1, MpfrFloat f2) = r.NextGMpfrFloat(precision: 100);
        Assert.NotEqual(f1, f2);
    }

    [Fact]
    public void NextExponentialDistributedMpfrFloatTest()
    {
        using GmpRandom r = new();
        using MpfrFloat f = r.NextEMpfrFloat(precision: 100);
        Assert.True(f < 20u);
    }
}
