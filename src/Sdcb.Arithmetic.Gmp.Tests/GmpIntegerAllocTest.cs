using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpIntegerAllocTest
{
    private readonly ITestOutputHelper _console;

    public GmpIntegerAllocTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData("-100")]
    [InlineData(
        "12345678901234567890123456789012345678901234567890123456" +
        "12345678901234567890123456789012345678901234567890123456" +
        "12345678901234567890123456789012345678901234567890123456")]
    public void RellocToFitShouldOk(string num)
    {
        using GmpInteger n = new(bitCount: 65536);
        n.Assign(num);
        n.ReallocToFit();
        Assert.True(n.Raw.Allocated * GmpLib.LimbBitSize < 65536);
        Assert.Equal(num, n.ToString());
    }
}