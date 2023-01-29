using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpRandomTest
{
    private readonly ITestOutputHelper _console;

    public GmpRandomTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void RandomInteger()
    {
        using GmpRandom random = new();
        using GmpInteger r1 = random.NextGmpIntegerNBits(bitCount: 128);
        using GmpInteger r2 = random.NextGmpIntegerNBits(bitCount: 128);
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void RandomDefaultSeedNotSame()
    {
        using GmpRandom random1 = new();
        using GmpInteger r1 = random1.NextGmpIntegerNBits(bitCount: 128);

        using GmpRandom random2 = new();
        using GmpInteger r2 = random2.NextGmpIntegerNBits(bitCount: 128);

        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void RandomSameSeedNotSame()
    {
        using GmpRandom random1 = new(seed: 7);
        using GmpInteger r1 = random1.NextGmpIntegerNBits(bitCount: 128);

        using GmpRandom random2 = new(seed: 7);
        using GmpInteger r2 = random2.NextGmpIntegerNBits(bitCount: 128);

        Assert.Equal(r1, r2);
    }

    [Fact]
    public void RandomFloatNotSame()
    {
        using GmpRandom random = new(seed: 7);
        using GmpFloat r1 = random.NextGmpFloat(128, nbits: 128);
        using GmpFloat r2 = random.NextGmpFloat(128, nbits: 128);
        Assert.NotEqual(r1, r2);
    }
}