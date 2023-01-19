using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class BigFloatFastTest
{
    private readonly ITestOutputHelper _console;

    public BigFloatFastTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void AssertSpecifiedPrecision()
    {
        BigFloat b1 = new BigFloat(precision: 100);
        Assert.Equal(128ul, b1.Precision);
    }

    [Fact]
    public void DefaultPrecision()
    {
        BigFloat b1 = new BigFloat();
        Assert.Equal(64u, b1.Precision);
    }

    [Fact]
    public void PiMulE()
    {
        uint oldPrecision = BigFloat.DefaultPrecision;
        try
        {
            BigFloat.DefaultPrecision = 2 << 10;
            BigFloat b1 = BigFloat.From(3.14);
            BigFloat b2 = BigFloat.Parse("2.718");
            BigFloat b3 = b1 * b2;
            Assert.Equal("8.53452000000000033796965226429165340960025787353515625", b3.ToString());
        }
        finally
        {
            BigFloat.DefaultPrecision = oldPrecision;
        }
    }

    [Theory]
    [InlineData(-65535.125, 10, "-65535.125")]
    [InlineData(0.125, 10, "0.125")]
    [InlineData(0, 10, "0")]
    [InlineData(42, 10, "42")]
    [InlineData(-2147483647, 10, "-2147483647")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        BigFloat f = BigFloat.From(val);
        Assert.Equal(expected, f.ToString(opBase));
    }
}