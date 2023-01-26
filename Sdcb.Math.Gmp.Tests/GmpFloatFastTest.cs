using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class GmpFloatFastTest
{
    private readonly ITestOutputHelper _console;

    public GmpFloatFastTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void AssertSpecifiedPrecision()
    {
        GmpFloat b1 = new GmpFloat(precision: 100);
        Assert.Equal(128ul, b1.Precision);
    }

    [Fact]
    public void DefaultPrecision()
    {
        GmpFloat b1 = new GmpFloat();
        Assert.Equal(64u, b1.Precision);
    }

    [Fact]
    public void PiMulE()
    {
        uint oldPrecision = GmpFloat.DefaultPrecision;
        try
        {
            GmpFloat.DefaultPrecision = 2 << 10;
            GmpFloat b1 = GmpFloat.From(3.14);
            GmpFloat b2 = GmpFloat.Parse("2.718");
            GmpFloat b3 = b1 * b2;
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
    [InlineData(42, 10, "42")]
    [InlineData(-2147483647, 10, "-2147483647")]
    [InlineData(7000, 10, "7000")]
    [InlineData(-90000, 10, "-90000")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        GmpFloat f = GmpFloat.From(val);
        Assert.Equal(expected, f.ToString(opBase));
    }
}