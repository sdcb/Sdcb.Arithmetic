using System.Diagnostics;
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
        public void DefaultValueShouldBeZero()
        {
            using MpfrFloat r = new();
            Assert.Equal(0, r.ToDouble());
        }

        [Fact]
        public void MpfrRoundingDefault()
        {
            Assert.Equal(MpfrRounding.ToEven, MpfrFloat.DefaultRounding);
        }

        [Fact]
        public void AssignRounding()
        {
            MpfrFloat.DefaultRounding = MpfrRounding.ToPositiveInfinity;
            Assert.Equal(MpfrRounding.ToPositiveInfinity, MpfrFloat.DefaultRounding);
        }

        [Fact]
        public void AssignRoundingDifferentThread()
        {
            Task.Run(() =>
            {
                MpfrFloat.DefaultRounding = MpfrRounding.ToPositiveInfinity;
            }).Wait();
            Assert.Equal(MpfrRounding.ToEven, MpfrFloat.DefaultRounding);
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

        [Theory]
        [InlineData("1.625", "1.625")]
        [InlineData("NaN", "NaN")]
        [InlineData("-inf", "-Inf")]
        [InlineData("+iNf", "Inf")]
        public void ToStringTest(string val, string expected)
        {
            MpfrFloat flt = MpfrFloat.Parse(val);
            Assert.Equal(expected, flt.ToString());
        }
    }
}
