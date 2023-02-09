using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
    public class ExceptionFunctionsTests
    {
        private readonly ITestOutputHelper _console;

        public ExceptionFunctionsTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void EMinMax()
        {
            _console.WriteLine($"MinExponent: {MpfrFloat.EMin}");
            _console.WriteLine($"MaxExponent: {MpfrFloat.EMax}");
        }

        [Fact]
        public void SetEMin()
        {
            int org = MpfrFloat.EMin;
            MpfrFloat.EMin = 100;
            Assert.Equal(100, MpfrFloat.EMin);
            MpfrFloat.EMin = org;
            Assert.Equal(org, MpfrFloat.EMin);
        }

        [Fact]
        public void SetEMax()
        {
            int org = MpfrFloat.EMax;
            MpfrFloat.EMax = 100;
            Assert.Equal(100, MpfrFloat.EMax);
            MpfrFloat.EMax = org;
            Assert.Equal(org, MpfrFloat.EMax);
        }

        [Fact]
        public void MinMaxE()
        {
            _console.WriteLine($"Min-EMin: {MpfrFloat.MinimumEMin}");
            _console.WriteLine($"Min-EMax: {MpfrFloat.MinimumEMax}");
            _console.WriteLine($"Max-EMax: {MpfrFloat.MaximumEMin}");
            _console.WriteLine($"Max-EMax: {MpfrFloat.MaximumEMax}");
        }
    }
}
