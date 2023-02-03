using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
    public class DefaultsTest
    {
        private readonly ITestOutputHelper _console;

        public DefaultsTest(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void DefaultValueShouldBeNan()
        {
            using MpfrFloat r = new();
            Assert.Equal(double.NaN, r.ToDouble());
        }

        [Fact]
        public void MpfrRoundingDefault()
        {
            Assert.Equal(MpfrRounding.Nearest, MpfrFloat.DefaultRounding);
        }

        [Fact]
        public void AssignRounding()
        {
            MpfrFloat.DefaultRounding = MpfrRounding.Up;
            Assert.Equal(MpfrRounding.Up, MpfrFloat.DefaultRounding);
        }

        [Fact]
        public void AssignRoundingDifferentThread()
        {
            Task.Run(() =>
            {
                MpfrFloat.DefaultRounding = MpfrRounding.Up;
            }).Wait();
            Assert.Equal(MpfrRounding.Nearest, MpfrFloat.DefaultRounding);
        }

        [Fact]
        public void DefaultPrecision()
        {
            Assert.Equal(53, MpfrFloat.DefaultPrecision);
        }

        [Fact]
        public void AssignDefaultPrecision()
        {
            MpfrFloat.DefaultPrecision = 100;
            Assert.Equal(100, MpfrFloat.DefaultPrecision);
        }

        [Fact]
        public void AssignDefaultPrecisionEffectDefaultFloat()
        {
            MpfrFloat.DefaultPrecision = 100;
            using MpfrFloat flt = new();
            Assert.Equal(100, flt.Precision);
        }

        [Fact]
        public void SpecifiedPrecisionShouldWork()
        {
            MpfrFloat.DefaultPrecision = 100;
            using MpfrFloat flt = new(precision: 80);
            Assert.Equal(80, flt.Precision);
        }
    }
}
