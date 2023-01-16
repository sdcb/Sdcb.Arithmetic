using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests
{
    public class BigFloatTest
    {
        private readonly ITestOutputHelper _console;

        public BigFloatTest(ITestOutputHelper console)
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
            _console.WriteLine($"default precision: {b1.Precision}");
        }

        [Fact]
        public void PiMulE()
        {
            BigFloat b1 = BigFloat.From(3.14);
            BigFloat b2 = BigFloat.Parse("2.718");
        }
    }
}