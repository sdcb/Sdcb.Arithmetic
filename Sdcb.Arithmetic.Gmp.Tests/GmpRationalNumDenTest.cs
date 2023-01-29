using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpRationalNumDenTest
{
    private readonly ITestOutputHelper _console;

    public GmpRationalNumDenTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(17, 5)]
    [InlineData(-17, 5)]
    [InlineData(0, 1)]
    public void BasicNumDen(int num, uint den)
    {
        using GmpRational r = GmpRational.From(num, den);
        Assert.Equal(num, (int)r.Num);
        Assert.Equal(den, (uint)r.Den);
    }

    [Theory]
    [InlineData(17, 5)]
    [InlineData(-17, 5)]
    public void AssignNumDen(int num, uint den)
    {
        using GmpRational r = new();
        r.Num = GmpInteger.From(num);
        r.Den = GmpInteger.From(den);
        Assert.Equal(num, (int)r.Num);
        Assert.Equal(den, (uint)r.Den);
    }
}