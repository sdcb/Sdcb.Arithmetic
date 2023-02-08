using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class IntegerTests
{
    private readonly ITestOutputHelper _console;

    public IntegerTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void FractionalTest()
    {
        using MpfrFloat op = MpfrFloat.From(3.14);
        Assert.Equal(0.14, MpfrFloat.Fractional(op).ToDouble(), 2);
    }

    [Theory]
    [InlineData(3.14, false)]
    [InlineData(10086, true)]
    public void IsIntegerTest(double d, bool isInteger)
    {
        using MpfrFloat op = MpfrFloat.From(d);
        Assert.Equal(isInteger, op.IsInteger);
    }

    [Fact]
    public void ModFractionalTest()
    {
        using MpfrFloat pi = MpfrFloat.ConstPi(100);
        using MpfrFloat expectedFrac = pi - 3;
        using MpfrFloat expectedInteger = MpfrFloat.Round(pi);
        (MpfrFloat actualInteger, MpfrFloat actualFrac, int round) = MpfrFloat.ModFractional(pi);
        Assert.Equal(expectedFrac, actualFrac);
        Assert.Equal(expectedInteger, actualInteger);
    }

    [Fact]
    public void ModTest()
    {
        using MpfrFloat a = MpfrFloat.ConstPi(100);
        using MpfrFloat b = MpfrFloat.From(1.5);
        using MpfrFloat mod = a % b;
        using MpfrFloat expectedMod = a - 3;
        Assert.Equal(expectedMod, mod);
    }

    [Fact]
    public void ModUITest()
    {
        using MpfrFloat a = MpfrFloat.ConstPi(100);
        MpfrFloat.MultiplyInplace(a, a, 2); // 6.28

        using MpfrFloat mod = a % 3;
        using MpfrFloat expectedMod = a - 6;
        Assert.Equal(expectedMod, mod);
    }

    [Fact]
    public void ModQuoTest()
    {
        using MpfrFloat a = MpfrFloat.ConstPi(100);
        MpfrFloat.MultiplyInplace(a, a, 2); // 6.28
        using MpfrFloat b = MpfrFloat.From(1.5);

        (MpfrFloat mod, int q, int round) = MpfrFloat.ModQuotient(a, b, precision: 100);
        MpfrFloat expectedMod = a - 6;
        Assert.Equal(expectedMod, mod);
        Assert.Equal(4, q);
        Assert.Equal(0, round);
    }

    [Fact]
    public void ReminderTest()
    {
        using MpfrFloat a = MpfrFloat.ConstPi(precision: 100);
        using MpfrFloat b = MpfrFloat.From(2);
        using MpfrFloat mod = MpfrFloat.Mod(a, b, a.Precision);
        using MpfrFloat rem = MpfrFloat.Reminder(a, b, a.Precision);
        using MpfrFloat expectedMod = a - 2;
        using MpfrFloat expectedRem = a - 4;
        Assert.Equal(expectedMod, mod);
        Assert.Equal(expectedRem, rem);
    }

    [Fact]
    public void ReminderQuoTest()
    {
        using MpfrFloat a = MpfrFloat.ConstPi(100);
        MpfrFloat.MultiplyInplace(a, a, 2); // 6.28
        using MpfrFloat b = MpfrFloat.From(4);

        (MpfrFloat rem, int q, int round) = MpfrFloat.ReminderQuotient(a, b, precision: 100);
        MpfrFloat expectedRem = a - 8;
        Assert.Equal(expectedRem, rem);
        Assert.Equal(2, q);
        Assert.Equal(0, round);
    }
}
