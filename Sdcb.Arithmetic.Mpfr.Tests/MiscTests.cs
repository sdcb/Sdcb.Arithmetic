using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class MiscTests
{
    private readonly ITestOutputHelper _console;

    public MiscTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void MinTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3u);
        using MpfrFloat op2 = MpfrFloat.From(4);
        using MpfrFloat expected = MpfrFloat.From(3);
        using MpfrFloat min = MpfrFloat.Min(op1, op2);
        Assert.Equal(expected, min);
    }

    [Fact]
    public void MaxTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3u);
        using MpfrFloat op2 = MpfrFloat.From(4);
        using MpfrFloat expected = MpfrFloat.From(4);
        using MpfrFloat min = MpfrFloat.Max(op1, op2);
        Assert.Equal(expected, min);
    }

    [Fact]
    public void TowardTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3u);
        using MpfrFloat op2 = MpfrFloat.From(4);
        op1.NextToward(op2);
        Assert.Equal(3.0000000000000004, op1.ToDouble());
    }

    [Fact]
    public void NextAboveTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3u);
        op1.NextAbove();
        Assert.Equal(3.0000000000000004, op1.ToDouble());
    }

    [Fact]
    public void NextBelowTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3.0000000000000004);
        op1.NextBelow();
        Assert.Equal(3, op1.ToDouble());
    }

    [Fact]
    public void ExpTest()
    {
        using MpfrFloat op1 = MpfrFloat.From(3.14);
        Assert.Equal(2, op1.Exponent);
        op1.Exponent *= 2;
        Assert.Equal(3.14 * 2 * 2, op1.ToDouble());
    }
}
