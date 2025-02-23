using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class ExceptionFunctionsTests
{
    private readonly ITestOutputHelper _console;

    public ExceptionFunctionsTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void EMinMax()
    {
        _console.WriteLine($"MinExponent: {MpfrFloat.EMin}");
        _console.WriteLine($"MaxExponent: {MpfrFloat.EMax}");
    }

    [Fact]
    public void SetEMin()
    {
        nint org = MpfrFloat.EMin;
        MpfrFloat.EMin = 100;
        Assert.Equal(100, MpfrFloat.EMin);
        MpfrFloat.EMin = org;
        Assert.Equal(org, MpfrFloat.EMin);
    }

    [Fact]
    public void SetEMax()
    {
        nint org = MpfrFloat.EMax;
        MpfrFloat.EMax = 100;
        Assert.Equal(100, MpfrFloat.EMax);
        MpfrFloat.EMax = org;
        Assert.Equal(org, MpfrFloat.EMax);
    }

    [Fact]
    public void MinMaxE()
    {
        _console.WriteLine($"Min-EMin: {MpfrFloat.MinEMin}");
        _console.WriteLine($"Min-EMax: {MpfrFloat.MinEMax}");
        _console.WriteLine($"Max-EMax: {MpfrFloat.MaxEMin}");
        _console.WriteLine($"Max-EMax: {MpfrFloat.MaxEMax}");
    }

    [Fact]
    public void SubNormalizeTest()
    {
        nint org = MpfrFloat.EMin;
        using MpfrFloat a = MpfrFloat.Parse("0.00001111000011110101010101", @base: 2, precision: 24);

        MpfrFloat.EMin = -23;
        a.SubNormalize(0);
        Assert.Equal("0.000011110000111101010101", a.ToString(2));

        MpfrFloat.EMin = -22;
        a.SubNormalize(0);
        Assert.Equal("0.0000111100001111010101", a.ToString(2));

        MpfrFloat.EMin = org;
    }

    [Fact]
    public void FlagsTest()
    {
        MpfrFloat.ErrorFlags = 0;
        Assert.False(MpfrKnownErrorFlags.ERange);

        MpfrFloat.ErrorFlags |= MpfrErrorFlags.ERange;
        Assert.True(MpfrKnownErrorFlags.ERange);
    }

    [Fact]
    public void UnderflowTest()
    {
        nint org = MpfrFloat.EMin;

        using MpfrFloat a = MpfrFloat.Parse("0.00001111000011110101010101", @base: 2, precision: 24);
        MpfrFloat.EMin = -23;
        a.SubNormalize(0);
        _console.WriteLine(MpfrFloat.ErrorFlags.ToString());
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.Underflow));

        MpfrFloat.EMin = org;
    }

    [Fact]
    public void OverflowTest()
    {
        nint emax = MpfrFloat.EMax;

        MpfrFloat.EMax = 2000;
        using MpfrFloat a = MpfrFloat.From(double.MaxValue);
        MpfrFloat.MultiplyInplace(a, a, double.MaxValue);
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.Overflow));

        MpfrFloat.EMax = emax;
    }

    [Fact]
    public void InexactTest()
    {
        MpfrFloat.ErrorFlags = 0;
        using MpfrFloat f = MpfrFloat.From(double.MinValue);
        MpfrFloat.SubtractInplace(f, f, 1);
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.Inexact));
        _console.WriteLine(MpfrFloat.ErrorFlags.ToString());
    }

    [Fact]
    public void DivideByZeroTest()
    {
        MpfrFloat.ErrorFlags = 0;
        using MpfrFloat f = MpfrFloat.From(double.MinValue);
        MpfrFloat.DivideInplace(f, f, (nuint)0);
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.DivideByZero));
    }

    [Fact]
    public void NaNTest()
    {
        MpfrFloat.ErrorFlags = 0;
        using MpfrFloat f = MpfrFloat.From(double.NaN);
        f.NextAbove();
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.NaN));
    }

    [Fact]
    public void ERangeTest()
    {
        MpfrFloat.ErrorFlags = 0;
        using MpfrFloat f1 = MpfrFloat.From(double.MinValue);
        using MpfrFloat f2 = MpfrFloat.From(double.NaN);
        MpfrFloat.Compare(f1, f2);
        Assert.True(MpfrFloat.ErrorFlags.HasFlag(MpfrErrorFlags.ERange));
    }
}
