using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests
{
    public class BigFloatMiscTest
    {
        private readonly ITestOutputHelper _console;

        public BigFloatMiscTest(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void AssertSpecifiedPrecision()
        {
            BigFloat b1 = new BigFloat(precision: 100);
            Assert.Equal(128ul, b1.Precision);
        }

        [Fact]
        public void DefaultPrecision()
        {
            BigFloat b1 = new BigFloat();
            Assert.Equal(64u, b1.Precision);
        }

        [Fact]
        public void PiMulE()
        {
            uint oldPrecision = BigFloat.DefaultPrecision;
            try
            {
                BigFloat.DefaultPrecision = 2 << 10;
                BigFloat b1 = BigFloat.From(3.14);
                BigFloat b2 = BigFloat.Parse("2.718");
                BigFloat b3 = b1 * b2;
                Assert.Equal("8.53452000000000033796965226429165340960025787353515625", b3.ToString());
            }
            finally
            {
                BigFloat.DefaultPrecision = oldPrecision;
            }
        }

        [Fact]
        public void NegativeToString()
        {
            BigFloat b = BigFloat.Parse("-65535.125");
            Assert.Equal("-65535.125", b.ToString());
        }

        [Fact]
        public void DecimalToString()
        {
            BigFloat b = BigFloat.Parse("0.125");
            Assert.Equal("0.125", b.ToString());
        }

        [Fact]
        public void ZeroToString()
        {
            BigFloat b = BigFloat.Parse("0");
            Assert.Equal("0", b.ToString());
        }
    }
}