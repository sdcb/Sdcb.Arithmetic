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

        [Theory]
        [InlineData(0.5, 2, 2.25)]
        public void CompoundTest(double op, int n, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Compound(fop, n);
            Assert.Equal(expected, rop.ToDouble());
        }

        #region Trigonometric function
        [Theory]
        [InlineData(0, 1)]
        [InlineData(Math.PI / 2, 0)]
        [InlineData(Math.PI, -1)]
        [InlineData(Math.PI * 1.5, 0)]
        [InlineData(Math.PI * 2, 1)]
        public void CosTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Cos(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(Math.PI / 2, 1)]
        [InlineData(Math.PI, 0)]
        [InlineData(Math.PI * 1.5, -1)]
        [InlineData(Math.PI * 2, 0)]
        public void SinTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Sin(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(Math.PI / 2, 16331239353195370)]
        [InlineData(Math.PI, 0)]
        [InlineData(Math.PI * 1.5, 5443746451065123)]
        [InlineData(Math.PI * 2, 0)]
        public void TanTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Tan(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(90, 0)]
        [InlineData(180, -1)]
        [InlineData(270, 0)]
        [InlineData(360, 1)]
        public void CosUTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.CosU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(90, 1)]
        [InlineData(180, 0)]
        [InlineData(270, -1)]
        [InlineData(360, 0)]
        public void SinUTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.SinU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(90, double.PositiveInfinity)]
        [InlineData(180, 0)]
        [InlineData(270, double.NegativeInfinity)]
        [InlineData(360, 0)]
        public void TanUTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.TanU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0.5, 0)]
        [InlineData(1, -1)]
        [InlineData(1.5, 0)]
        [InlineData(2, 1)]
        public void CosPiTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.CosPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0.5, 1)]
        [InlineData(1, 0)]
        [InlineData(1.5, -1)]
        [InlineData(2, 0)]
        public void SinPiTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.SinPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0.5, double.PositiveInfinity)]
        [InlineData(1, 0)]
        [InlineData(1.5, double.NegativeInfinity)]
        [InlineData(2, 0)]
        public void TanPiTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.TanPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(Math.PI / 2, 1, 0)]
        [InlineData(Math.PI, 0, -1)]
        [InlineData(Math.PI * 1.5, -1, 0)]
        [InlineData(Math.PI * 2, 0, 1)]
        public void SinCosTest(double op, double expectedSin, double expectedCos)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            (MpfrFloat sop, MpfrFloat cop)= MpfrFloat.SinCos(fop);
            Assert.Equal(expectedSin, sop.ToDouble(), 15);
            Assert.Equal(expectedCos, cop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(Math.PI / 2, 16331239353195370)]
        [InlineData(Math.PI, -1)]
        [InlineData(Math.PI * 1.5, -5443746451065123)]
        [InlineData(Math.PI * 2, 1)]
        public void SecTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Sec(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(Math.PI / 2, 1)]
        [InlineData(Math.PI, 8165619676597686)]
        [InlineData(Math.PI * 1.5, -1)]
        [InlineData(Math.PI * 2, -4082809838298843)]
        public void CscTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Csc(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(Math.PI / 2, 0)]
        [InlineData(Math.PI, -8165619676597686)]
        [InlineData(Math.PI * 1.5, 0)]
        [InlineData(Math.PI * 2, -4082809838298843)]
        public void CotTest(double op, double expected)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Cot(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(Math.PI / 2, 0)]
        [InlineData(Math.PI, -1)]
        public void AcosTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Acos(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(Math.PI / 2, 1)]
        [InlineData(-Math.PI / 2, -1)]
        public void AsinTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Asin(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(Math.PI / 2, double.PositiveInfinity)]
        [InlineData(-Math.PI / 2, double.NegativeInfinity)]
        public void AtanTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Atan(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(90, 0)]
        [InlineData(180, -1)]
        public void AcosUTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AcosU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(90, 1)]
        [InlineData(-90, -1)]
        public void AsinUTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AsinU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(90, double.PositiveInfinity)]
        [InlineData(-90, double.NegativeInfinity)]
        public void AtanUTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AtanU(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0.5, 0)]
        [InlineData(1, -1)]
        public void AcosPiTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AcosPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0.5, 1)]
        [InlineData(-0.5, -1)]
        public void AsinPiTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AsinPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0.5, double.PositiveInfinity)]
        [InlineData(-0.5, double.NegativeInfinity)]
        public void AtanPiTest(double expected, double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.AtanPi(fop);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(Math.PI / 4, 1, 1)]
        [InlineData(Math.PI / 2, 1, 0)]
        [InlineData(-Math.PI / 2, -1, 0)]
        public void Atan2Test(double expected, double op1, double op2)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat fop2 = MpfrFloat.From(op2);
            using MpfrFloat rop = MpfrFloat.Atan2(fop1, fop2);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(45, 1, 1)]
        [InlineData(90, 1, 0)]
        [InlineData(-90, -1, 0)]
        public void Atan2UTest(double expected, double op1, double op2)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat fop2 = MpfrFloat.From(op2);
            using MpfrFloat rop = MpfrFloat.Atan2U(fop1, fop2);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(0.25, 1, 1)]
        [InlineData(0.5, 1, 0)]
        [InlineData(-0.5, -1, 0)]
        public void Atan2PiTest(double expected, double op1, double op2)
        {
            using MpfrFloat fop1 = MpfrFloat.From(op1);
            using MpfrFloat fop2 = MpfrFloat.From(op2);
            using MpfrFloat rop = MpfrFloat.Atan2Pi(fop1, fop2);
            Assert.Equal(expected, rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 4)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 2)]
        public void CoshTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Cosh(fop);
            Assert.Equal(Math.Cosh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 1.5)]
        [InlineData(Math.PI * 2)]
        public void SinhTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Sinh(fop);
            Assert.Equal(Math.Sinh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 4)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 2)]
        public void TanhTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Tanh(fop);
            Assert.Equal(Math.Tanh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 1.5)]
        [InlineData(Math.PI * 2)]
        public void SinhCoshTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            (MpfrFloat sop, MpfrFloat cop) = MpfrFloat.SinhCosh(fop);
            Assert.Equal(Math.Sinh(op), sop.ToDouble(), 15);
            Assert.Equal(Math.Cosh(op), cop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 1.5)]
        [InlineData(Math.PI * 2)]
        public void SechTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Sech(fop);
            Assert.Equal(1.0 / Math.Cosh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 1.5)]
        [InlineData(Math.PI * 2)]
        public void CschTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Csch(fop);
            Assert.Equal(1.0 / Math.Sinh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        [InlineData(Math.PI * 1.5)]
        [InlineData(Math.PI * 2)]
        public void CothTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Coth(fop);
            Assert.Equal(1.0 / Math.Tanh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(Math.PI)]
        public void AcoshTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Acosh(fop);
            Assert.Equal(Math.Acosh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(-Math.PI / 2)]
        public void AsinhTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Asinh(fop);
            Assert.Equal(Math.Asinh(op), rop.ToDouble(), 15);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Math.PI / 2)]
        [InlineData(-Math.PI / 2)]
        public void AtanhTest(double op)
        {
            using MpfrFloat fop = MpfrFloat.From(op);
            using MpfrFloat rop = MpfrFloat.Atanh(fop);
            Assert.Equal(Math.Atanh(op), rop.ToDouble(), 15);
        }
        #endregion

        [Fact]
        public void EintTest()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat eint = MpfrFloat.Eint(op);
            Assert.Equal(1.8951178163559368, eint.ToDouble());
        }

        [Fact]
        public void Li2Test()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat li2 = MpfrFloat.Li2(op);
            Assert.Equal(1.6449340668482264, li2.ToDouble());
        }

        [Fact]
        public void GammaTest()
        {
            using MpfrFloat op = MpfrFloat.From(7);
            using MpfrFloat gamma = MpfrFloat.Gamma(op);
            Assert.Equal(720, gamma.ToDouble());
        }

        [Fact]
        public void GammaIncTest()
        {
            using MpfrFloat op = MpfrFloat.From(7);
            using MpfrFloat op2 = MpfrFloat.From(0);
            using MpfrFloat gamma = MpfrFloat.GammaInc(op, op2);
            Assert.Equal(720, gamma.ToDouble());
        }

        [Fact]
        public void LogGammaTest()
        {
            using MpfrFloat op = MpfrFloat.From(7);
            using MpfrFloat gamma = MpfrFloat.LogGamma(op);
            Assert.Equal(Math.Log(720), gamma.ToDouble());
        }

        [Fact]
        public void LGammaTest()
        {
            using MpfrFloat op = MpfrFloat.From(7);
            (int sign, MpfrFloat lgamma, int round) = MpfrFloat.LGamma(op);
            Assert.Equal(1, sign);
            Assert.Equal(Math.Log(720), lgamma.ToDouble());
            Assert.Equal(1, round);
        }

        [Fact]
        public void DigammaTest()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat digamma = MpfrFloat.Digamma(op);
            Assert.Equal(0.577215664901532, -digamma.ToDouble(), 14);
        }


        [Fact]
        public void BetaTest()
        {
            using MpfrFloat op1 = MpfrFloat.From(1);
            using MpfrFloat op2 = MpfrFloat.From(2);
            using MpfrFloat rop = MpfrFloat.Beta(op1, op2);
            Assert.Equal(0.5, rop.ToDouble());
        }

        [Fact]
        public void ZetaTest()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat rop = MpfrFloat.Zeta(op);
            Assert.Equal(double.PositiveInfinity, rop.ToDouble());
        }

        [Fact]
        public void ZetaUIntTest()
        {
            using MpfrFloat rop = MpfrFloat.Zeta(1);
            Assert.Equal(double.PositiveInfinity, rop.ToDouble());
        }

        [Fact]
        public void ErrorFunctionTest()
        {
            using MpfrFloat op = MpfrFloat.From(3.29);
            using MpfrFloat rop = MpfrFloat.ErrorFunction(op);
            Assert.Equal(0.999997, rop.ToDouble(), 6);
        }

        [Fact]
        public void ComplementaryErrorFunctionTest()
        {
            using MpfrFloat op = MpfrFloat.From(3.29);
            using MpfrFloat rop = MpfrFloat.ComplementaryErrorFunction(op);
            Assert.Equal(0.000003, rop.ToDouble(), 6);
        }

        [Fact]
        public void AGMTest()
        {
            using MpfrFloat op1 = MpfrFloat.From(3);
            using MpfrFloat op2 = MpfrFloat.From(5);
            using MpfrFloat rop = MpfrFloat.AGM(op1, op2);
            Assert.Equal(3.936236, rop.ToDouble(), 6);
        }

        [Fact]
        public void AiryTest()
        {
            using MpfrFloat op = MpfrFloat.From(8);
            using MpfrFloat rop = MpfrFloat.Airy(op);
            Assert.Equal(0, rop.ToDouble(), 6);
        }

        #region Bessel function
        [Fact]
        public void BesselJTest()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat j0 = MpfrFloat.J0(op);
            using MpfrFloat j1 = MpfrFloat.J1(op);
            using MpfrFloat j2 = MpfrFloat.JN(2, op);
            Assert.Equal(0.765198, j0.ToDouble(), 6);
            Assert.Equal(0.440051, j1.ToDouble(), 6);
            Assert.Equal(0.114903, j2.ToDouble(), 6);
        }

        [Fact]
        public void BesselYTest()
        {
            using MpfrFloat op = MpfrFloat.From(1);
            using MpfrFloat y0 = MpfrFloat.Y0(op);
            using MpfrFloat y1 = MpfrFloat.Y1(op);
            using MpfrFloat y2 = MpfrFloat.YN(2, op);
            Assert.Equal(0.088257, y0.ToDouble(), 6);
            Assert.Equal(-0.781213, y1.ToDouble(), 6);
            Assert.Equal(-1.650683, y2.ToDouble(), 6);
        }
        #endregion

        [Fact]
        public void ConstLog2Test()
        {
            using MpfrFloat log2 = MpfrFloat.ConstLog2();
            Assert.Equal(Math.Log(2), log2.ToDouble());
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

        [Fact]
        public void ConstEularTest()
        {
            using MpfrFloat log2 = MpfrFloat.ConstEuler();
            Assert.Equal(0.5772156649015329, log2.ToDouble());
        }

        [Fact]
        public void ConstCatalanTest()
        {
            using MpfrFloat log2 = MpfrFloat.ConstCatalan();
            Assert.Equal(0.915965594177219, log2.ToDouble(), 3);
        }
    }
}
