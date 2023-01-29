using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpIntegerNumTheoreticTest
{
    private readonly ITestOutputHelper _console;

    public GmpIntegerNumTheoreticTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void InvertTest()
    {
        GmpInteger a = GmpInteger.From(7);
        GmpInteger b = GmpInteger.From(17);
        GmpInteger c = GmpInteger.Invert(a, b);
        Assert.Equal(5, (int)c);
    }

    [Fact]
    public void RemoveTest()
    {
        GmpInteger a = GmpInteger.From(98);
        GmpInteger b = GmpInteger.From(7);
        GmpInteger c = GmpInteger.RemoveFactor(a, b);
        Assert.Equal(2, (int)c);
    }

    [Fact]
    public void FactorTest()
    {
        GmpInteger a = GmpInteger.Factorial(5);
        Assert.Equal(120, (int)a);
    }

    [Fact]
    public void Fibonacci2()
    {
        (GmpInteger a, GmpInteger b) = GmpInteger.Fibonacci2(10);
        Assert.Equal(55, (int)a);
        Assert.Equal(34, (int)b);
    }

    [Fact]
    public void LucasNum2()
    {
        (GmpInteger a, GmpInteger b) = GmpInteger.LucasNum2(10);
        Assert.Equal(123, (int)a);
        Assert.Equal(76, (int)b);
    }
}