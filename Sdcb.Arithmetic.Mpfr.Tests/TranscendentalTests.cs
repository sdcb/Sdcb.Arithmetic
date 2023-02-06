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

        [Theory]
        [InlineData(2, 7.389056098930650227230427460575 - 1)]
        public void ExpM1Test(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat exp = MpfrFloat.ExpM1(fop);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(2, 4 - 1)]
        public void Exp2M1Test(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat exp = MpfrFloat.Exp2M1(fop);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(2, 100 - 1)]
        public void Exp10M1Test(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat exp = MpfrFloat.Exp10M1(fop);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(3, 4, 81)]
        [InlineData(0, 0, 1)]
        [InlineData(double.NaN, 0, 1)]
        public void PowerTest(double op1, double op2, double result)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat fop2 = MpfrFloat.From(op2);
            using MpfrFloat rop = MpfrFloat.Power(fop1, fop2);
            Assert.Equal(result, rop.ToDouble());
        }

        [Theory]
        [InlineData(3, 4, 81)]
        [InlineData(0, 0, double.NaN)]
        [InlineData(double.NaN, 0, double.NaN)]
        public void PowerRTest(double op1, double op2, double result)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat fop2 = MpfrFloat.From(op2);
            using MpfrFloat rop = MpfrFloat.PowerR(fop1, fop2);
            Assert.Equal(result, rop.ToDouble());
        }

        [Theory]
        [InlineData(Math.E, 1)]
        public void LogTest(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.Log(fop1);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(2, 1)]
        public void Log2Test(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.Log2(fop1);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(10, 1)]
        public void Log10Test(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.Log10(fop1);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(Math.E - 1, 1)]
        public void LogP1Test(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.LogP1(fop1);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(2 - 1, 1)]
        public void Log2P1Test(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.Log2P1(fop1);
            Assert.Equal(expected, exp.ToDouble());
        }

        [Theory]
        [InlineData(10 - 1, 1)]
        public void Log10P1Test(double op1, double expected)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat exp = MpfrFloat.Log10P1(fop1);
            Assert.Equal(expected, exp.ToDouble());
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
