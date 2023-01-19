using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class BigIntegerFastTest
{
    private readonly ITestOutputHelper _console;

    public BigIntegerFastTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(3, 0, "3")]
    [InlineData(-3, 10, "-3")]
    [InlineData(255, 2, "11111111")]
    [InlineData(-255, 16, "-ff")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        BigInteger z = BigInteger.From(val);
        Assert.Equal(expected, z.ToString(opBase));
    }
}