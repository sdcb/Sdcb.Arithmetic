using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class CompareTests
{
    private readonly ITestOutputHelper _console;

    public CompareTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void TwoHashCodeShouldSame()
    {
        using MpfrFloat a = MpfrFloat.From(3.5, precision: 500);
        using MpfrFloat b = MpfrFloat.Parse("3.5", precision: 500);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
