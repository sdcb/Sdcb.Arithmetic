using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
    public class TranscendentalTests
    {
        private readonly ITestOutputHelper _console;

        public TranscendentalTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void ConstPiTest()
        {
            Stopwatch sw = Stopwatch.StartNew();
            //MpfrFloat pi = MpfrFloat.ConstPi(precision: 332_2000);
            MpfrFloat pi = MpfrFloat.ConstPi();
            _console.WriteLine($"elapsed={sw.ElapsedMilliseconds}ms");
            _console.WriteLine(pi.ToString().Length.ToString());
            Assert.StartsWith("3.1415926", pi.ToString());
        }
    }
}
