using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class CompareTests
{
    private readonly ITestOutputHelper _console;

    public CompareTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void MainCompare()
    {
        using MpfrFloat a = MpfrFloat.From(3, precision: 100);
        using MpfrFloat b = MpfrFloat.From(4, precision: 200);
        using MpfrFloat c = MpfrFloat.From(4, precision: 60);
        Assert.True(a < b);
        Assert.True(a <= b);
        Assert.True(b > a);
        Assert.True(b >= a);
        Assert.True(b == c);
        Assert.True(a != b);
        Assert.True(MpfrFloat.CompareLess(a, b));
        Assert.True(MpfrFloat.CompareLessOrEquals(a, b));
        Assert.True(MpfrFloat.CompareGreater(b, a));
        Assert.True(MpfrFloat.CompareGreaterOrEquals(b, a));
        Assert.True(MpfrFloat.CompareEquals(b, c));
        Assert.False(MpfrFloat.CompareEquals(a, b));
    }

    [Fact]
    public void TwoHashCodeShouldSame()
    {
        using MpfrFloat a = MpfrFloat.From(3.5, precision: 500);
        using MpfrFloat b = MpfrFloat.Parse("3.5", precision: 500);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
