using Sdcb.Arithmetic.Gmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
    public class ArithmeticTests
    {
        private readonly ITestOutputHelper _console;

        public ArithmeticTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void SumTest()
        {
            using MpfrFloat a = MpfrFloat.From(3);
            using MpfrFloat b = MpfrFloat.From(4.25);
            using MpfrFloat c = MpfrFloat.From(-2);

            using MpfrFloat result = MpfrFloat.Sum(new[] { a, b, c });
            Assert.Equal(5.25, result.ToDouble());
        }

        [Fact]
        public void SumInplaceTest()
        {
            using MpfrFloat a = MpfrFloat.From(3);
            using MpfrFloat b = MpfrFloat.From(4.25);
            using MpfrFloat c = MpfrFloat.From(-2);

            using MpfrFloat result = new(precision: 100);
            MpfrFloat.SumInplace(result, new[] { a, b, c });
            Assert.Equal(5.25, result.ToDouble());
        }

        [Fact]
        public void HypotTest()
        {
            using MpfrFloat a = MpfrFloat.From(3);
            using MpfrFloat b = MpfrFloat.From(4);

            using MpfrFloat result = MpfrFloat.Hypot(a, b);
            Assert.Equal(5, result.ToDouble());
        }

        [Fact]
        public void FMMSTest()
        {
            using MpfrFloat a = MpfrFloat.From(3);
            using MpfrFloat b = MpfrFloat.From(4);
            using MpfrFloat c = MpfrFloat.From(5);
            using MpfrFloat d = MpfrFloat.From(6);

            // (3x4) - (5x6) = 12 - 30 = -18
            using MpfrFloat result = MpfrFloat.FMMS(a, b, c, d);
            Assert.Equal(-18, result.ToDouble());
        }

        [Fact]
        public void FactorialTest()
        {
            using MpfrFloat r = MpfrFloat.Factorial(5);
            Assert.Equal(120, r.ToInt32());
        }

        [Fact]
        public void OperatorConvertFromTest()
        {
            using MpfrFloat rsi = 3;
            using MpfrFloat rui = 3u;
            using MpfrFloat rd = 3.14;
            using MpfrFloat rz = (MpfrFloat)GmpInteger.From(3);
            using MpfrFloat rq = (MpfrFloat)GmpRational.From(3);
            using MpfrFloat rf = GmpFloat.From(3.14);

            Assert.Equal(3, rsi.ToInt32());
            Assert.Equal(3u, rui.ToUInt32());
            Assert.Equal(3.14, rd.ToDouble());
            Assert.Equal(3, rz.ToInt32());
            Assert.Equal(3, rq.ToInt32());
            Assert.Equal(3.14, rf.ToDouble());
        }

        [Fact]
        public void OperatorConvertToTest()
        {
            using MpfrFloat r = 1.5;
            Assert.Equal(1, (int)r);
            Assert.Equal(1u, (uint)r);
            Assert.Equal(1.5, (double)r);

            using GmpInteger z = (GmpInteger)r;
            Assert.Equal(1, z.ToInt32());

            using GmpRational q = (GmpRational)r;
            Assert.Equal(1.5, q.ToDouble());

            using GmpFloat f = (GmpFloat)r;
            Assert.Equal(1.5, f.ToDouble());
        }

        [Fact]
        public void CloneTest()
        {
            using MpfrFloat r = new (precision: 998);
            r.Assign(-3.14);
            using MpfrFloat r2 = r.Clone();

            Assert.Equal(-3.14, r2.ToDouble());
            Assert.Equal(r.Precision, r2.Precision);

            r2.Assign(2.718);
            Assert.Equal(2.718, r2.ToDouble());
            Assert.Equal(-3.14, r.ToDouble());
        }

        [Fact]
        public void ExplicitConvertFromTest()
        {
            using MpfrFloat r = new(precision: 998);
            r.Assign(3.14);
            using GmpInteger z = (GmpInteger)r;
            using GmpFloat f = (GmpFloat)r;
            using GmpRational q = (GmpRational)r;

            Assert.Equal(3, z.ToInt32());
            Assert.Equal(3.14, f.ToDouble());
            Assert.Equal(3.14, q.ToDouble());
        }

        [Fact]
        public void ExplicitConvertToTest()
        {
            {
                using GmpInteger z = -99;
                using MpfrFloat zr = (MpfrFloat)z;
                Assert.Equal(-99, zr.ToInt32());
            }
            {
                using GmpFloat f = -99;
                using MpfrFloat fr = (MpfrFloat)f;
                Assert.Equal(-99, fr.ToDouble());
            }
            {
                using GmpRational q = -99;
                using MpfrFloat qr = (MpfrFloat)q;
                Assert.Equal(-99, qr.ToInt32());
            }
        }

        [Fact]
        public void FromBigIntegerTest()
        {
            string str = "2399668902200934240538265661362538479646144714727726081987941826880160606384643329140253260934807552";
            using GmpInteger d = GmpInteger.Parse(str);
            using MpfrFloat f = MpfrFloat.From(d);
            Assert.Equal(str, f.ToString());
        }
    }
}
