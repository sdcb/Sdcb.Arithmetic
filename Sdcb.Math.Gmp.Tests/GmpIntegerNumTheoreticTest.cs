using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

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
}