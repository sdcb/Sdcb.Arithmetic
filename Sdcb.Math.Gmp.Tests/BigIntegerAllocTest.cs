using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class BigIntegerAllocTest
{
    private readonly ITestOutputHelper _console;

    public BigIntegerAllocTest(ITestOutputHelper console)
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
        using BigInteger n = new BigInteger(bitCount: 65536);
        n.Assign(num);
        n.ReallocToFit();
        Assert.True(n.Raw.Allocated * GmpNative.LimbBitSize < 65536);
        Assert.Equal(num, n.ToString());
    }
}