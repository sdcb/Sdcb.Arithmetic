using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpFloatFastTest
{
#pragma warning disable IDE0052 // 删除未读的私有成员
    private readonly ITestOutputHelper _console;
#pragma warning restore IDE0052 // 删除未读的私有成员

    public GmpFloatFastTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void AssertSpecifiedPrecision()
    {
        using GmpFloat b1 = new(precision: 100);
        Assert.Equal(128ul, b1.Precision);
    }

    [Fact]
    public void DefaultPrecision()
    {
        using GmpFloat b1 = new();
        Assert.Equal(64u, b1.Precision);
    }

    [Fact]
    public void PiMulE()
    {
        uint oldPrecision = GmpFloat.DefaultPrecision;
        try
        {
            GmpFloat.DefaultPrecision = 2 << 10;
            using GmpFloat b1 = GmpFloat.From(3.14);
            using GmpFloat b2 = GmpFloat.Parse("2.718");
            using GmpFloat b3 = b1 * b2;
            Assert.Equal("8.53452000000000033796965226429165340960025787353515625", b3.ToString());
        }
        finally
        {
            GmpFloat.DefaultPrecision = oldPrecision;
        }
    }

    [Theory]
    [InlineData(-65535.125, 10, "-65535.125")]
    [InlineData(0.125, 10, "0.125")]
    [InlineData(0, 10, "0")]
    [InlineData(-2147483647, 10, "-2147483647")]
    [InlineData(255, 16, "ff")]
    [InlineData(2147483647, 62, "2LKcb1")]
    [InlineData(2147483647, 2, "1111111111111111111111111111111")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        using GmpFloat f = GmpFloat.From(val);
        Assert.Equal(expected, f.ToString(opBase));
    }

    [Theory]
    [InlineData(-2, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(10, true)]
    [InlineData(62, true)]
    [InlineData(63, false)]
    public void ValidBaseTest(int @base, bool good)
    {
        using GmpFloat f = GmpFloat.From(3.14);
        if (good)
        {
            f.ToString(@base);
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => f.ToString(@base));
        }
    }
}