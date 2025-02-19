using System.Diagnostics;
using System.Globalization;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class DefaultsTest
{
#pragma warning disable IDE0052 // 删除未读的私有成员
    private readonly ITestOutputHelper _console;
#pragma warning restore IDE0052 // 删除未读的私有成员

    public DefaultsTest(ITestOutputHelper console)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        _console = console;
    }

    [Fact]
    public void DefaultValueShouldBeZero()
    {
        using MpfrFloat r = new();
        Assert.Equal(0, r.ToDouble());
    }

    [Fact]
    public void DefaultWithPrecisionShouldBeZero()
    {
        using MpfrFloat r = new(64);
        Assert.Equal(0, r.ToDouble());
    }

    [Fact]
    public void MpfrRoundingDefault()
    {
        Assert.Equal(MpfrRounding.ToEven, MpfrFloat.DefaultRounding);
    }

    [Fact]
    public void AssignRounding()
    {
        MpfrRounding old = MpfrFloat.DefaultRounding;
        try
        {
            MpfrFloat.DefaultRounding = MpfrRounding.ToPositiveInfinity;
            Assert.Equal(MpfrRounding.ToPositiveInfinity, MpfrFloat.DefaultRounding);
        }
        finally
        {
            MpfrFloat.DefaultRounding = old;
        }
    }

    [Fact]
    public void AssignRoundingDifferentThread()
    {
        Thread t = new((p) =>
        {
            MpfrFloat.DefaultRounding = MpfrRounding.ToPositiveInfinity;
        });
        t.Start();
        t.Join();
        Assert.Equal(MpfrRounding.ToEven, MpfrFloat.DefaultRounding);
    }

    [Fact]
    public void DefaultPrecision()
    {
        Assert.Equal(53, MpfrFloat.DefaultPrecision);
    }

    [Fact]
    public void AssignDefaultPrecision()
    {
        int old = MpfrFloat.DefaultPrecision;
        try
        {
            MpfrFloat.DefaultPrecision = 100;
            Assert.Equal(100, MpfrFloat.DefaultPrecision);
        }
        finally
        {
            MpfrFloat.DefaultPrecision = old;
        }
    }

    [Fact]
    public void AssignDefaultPrecisionEffectDefaultFloat()
    {
        int old = MpfrFloat.DefaultPrecision;
        try
        {
            MpfrFloat.DefaultPrecision = 100;
            using MpfrFloat flt = new();
            Assert.Equal(100, flt.Precision);
        }
        finally
        {
            MpfrFloat.DefaultPrecision = old;
        }
    }

    [Fact]
    public void SpecifiedPrecisionShouldWork()
    {
        int old = MpfrFloat.DefaultPrecision;
        try
        {
            MpfrFloat.DefaultPrecision = 100;
            using MpfrFloat flt = new(precision: 80);
            Assert.Equal(80, flt.Precision);
        }
        finally
        {
            MpfrFloat.DefaultPrecision = old;
        }
    }

    [Theory]
    [InlineData("1.625", "1.625")]
    [InlineData("NaN", "NaN")]
    [InlineData("-inf", "-Infinity")]
    [InlineData("+iNf", "Infinity")]
    [InlineData("-0", "-0")]
    public void ToStringTest(string val, string expected)
    {
        using MpfrFloat flt = MpfrFloat.Parse(val);
        Assert.Equal(expected, flt.ToString());
    }
}
