using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class RoundTests
{
    private readonly ITestOutputHelper _console;

    public RoundTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(3.14)]
    [InlineData(3.8)]
    [InlineData(3.5)]
    [InlineData(4.5)]
    public void RoundTest(double op)
    {
        using MpfrFloat fop = MpfrFloat.From(op);
        using MpfrFloat rop = new();

        MpfrFloat.RIntInplace(rop, fop, MpfrRounding.ToEven);
        Assert.Equal(Math.Round(op, MidpointRounding.ToEven), rop.ToDouble());
        MpfrFloat.RoundEvenInplace(rop, fop);
        Assert.Equal(Math.Round(op, MidpointRounding.ToEven), rop.ToDouble());

        MpfrFloat.RIntInplace(rop, fop, MpfrRounding.ToZero);
        Assert.Equal(Math.Round(op, MidpointRounding.ToZero), rop.ToDouble());
        MpfrFloat.TruncateInplace(rop, fop);
        Assert.Equal(Math.Round(op, MidpointRounding.ToZero), rop.ToDouble());

        MpfrFloat.RIntInplace(rop, fop, MpfrRounding.ToPositiveInfinity);
        Assert.Equal(Math.Round(op, MidpointRounding.ToPositiveInfinity), rop.ToDouble());
        MpfrFloat.CeilingInplace(rop, fop);
        Assert.Equal(Math.Round(op, MidpointRounding.ToPositiveInfinity), rop.ToDouble());

        MpfrFloat.RIntInplace(rop, fop, MpfrRounding.ToNegativeInfinity);
        Assert.Equal(Math.Round(op, MidpointRounding.ToNegativeInfinity), rop.ToDouble());
        MpfrFloat.FloorInplace(rop, fop);
        Assert.Equal(Math.Round(op, MidpointRounding.ToNegativeInfinity), rop.ToDouble());

        MpfrFloat.RIntInplace(rop, fop, MpfrRounding.Faithful);
        Assert.Equal(Math.Round(op, MidpointRounding.AwayFromZero), rop.ToDouble());
        MpfrFloat.RoundInplace(rop, fop);
        Assert.Equal(Math.Round(op, MidpointRounding.AwayFromZero), rop.ToDouble());
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
}
