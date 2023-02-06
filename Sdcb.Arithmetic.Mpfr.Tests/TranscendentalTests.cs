using System.Diagnostics;
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
        public void ExpTest()
        {
            using MpfrFloat op = MpfrFloat.From(2);
            using MpfrFloat exp = MpfrFloat.Exp(op);
            Assert.Equal(7.389056098930650227230427460575, exp.ToDouble()); // e ^ 2
        }

        [Fact]
        public void ExpM1Test()
        {
            using MpfrFloat op = MpfrFloat.From(2);
            using MpfrFloat exp = MpfrFloat.ExpM1(op);
            Assert.Equal(7.389056098930650227230427460575 - 1, exp.ToDouble()); // e ^ 2
        }

        [Fact]
        public void Exp2Test()
        {
            using MpfrFloat op = MpfrFloat.From(2);
            using MpfrFloat exp = MpfrFloat.Exp2(op);
            Assert.Equal(4, exp.ToDouble()); // 2 ^ 2
        }

        [Fact]
        public void Exp10Test()
        {
            using MpfrFloat op = MpfrFloat.From(2);
            using MpfrFloat exp = MpfrFloat.Exp10(op);
            Assert.Equal(100, exp.ToDouble()); // 10 ^ 2
        }

        [Fact]
        public void ConstPiTest()
        {
            Stopwatch sw = Stopwatch.StartNew();
            //MpfrFloat pi = MpfrFloat.ConstPi(precision: 332_2000);
            using MpfrFloat pi = MpfrFloat.ConstPi();
            _console.WriteLine($"elapsed={sw.ElapsedMilliseconds}ms");
            _console.WriteLine(pi.ToString().Length.ToString());
            Assert.StartsWith("3.1415926", pi.ToString());
        }
    }
}
