using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
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
        public void RoundToPrecisionTest()
        {
            MpfrFloat r50 = MpfrFloat.ConstPi(50);
            MpfrFloat r100 = MpfrFloat.ConstPi(100);
            r100.RoundToPrecision(50);
            Assert.Equal(r50, r100);
            Assert.Equal(50, r100.Precision);
        }

        [Fact]
        public void MinimalPrecisionTest()
        {
            MpfrFloat r50to100 = MpfrFloat.ConstPi(50);
            r50to100.Precision = 100;
            Assert.Equal(50, r50to100.MinimalPrecision);
            Assert.Equal(3.1415926535897931, r50to100.ToDouble());
        }
    }
}
