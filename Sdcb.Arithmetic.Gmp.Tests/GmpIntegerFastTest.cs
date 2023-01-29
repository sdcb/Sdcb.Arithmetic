using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpIntegerFastTest
{
    private readonly ITestOutputHelper _console;

    public GmpIntegerFastTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(65535, 0, "65535")]
    [InlineData(0, 0, "0")]
    [InlineData(-3, 10, "-3")]
    [InlineData(255, 2, "11111111")]
    [InlineData(-255, 16, "-ff")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        GmpInteger z = GmpInteger.From(val);
        Assert.Equal(expected, z.ToString(opBase));
    }
}