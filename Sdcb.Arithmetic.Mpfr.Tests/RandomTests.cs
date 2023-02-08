using Sdcb.Arithmetic.Gmp;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class RandomTests
{
    private readonly ITestOutputHelper _console;

    public RandomTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void FastRandom5()
    {
        using GmpRandom r = new();
        for (int i = 0; i < 5; ++i)
        {
            using MpfrFloat f = r.NextMpfrFloat(precision: 100);
            _console.WriteLine(f.ToString());
        }
    }
}
