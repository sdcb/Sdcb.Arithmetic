using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests
{
    public class GmpRationalArithmeticTests
    {
        private readonly ITestOutputHelper _console;

        public GmpRationalArithmeticTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Theory]
        [InlineData("-1/3", "1/2", "1/6")]
        [InlineData("-1/3000000000000000000000000000000000000000000", "1/3000000000000000000000000000000000000000000", "0")]
        public void AddTest(string op1String, string op2String, string expected)
        {
            using GmpRational op1 = GmpRational.Parse(op1String);
            using GmpRational op2 = GmpRational.Parse(op2String);
            using GmpRational result = op1 + op2;
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("-1/3", "1/2", "1/6")]
        [InlineData("-1/3000000000000000000000000000000000000000000", "1/3000000000000000000000000000000000000000000", "0")]
        public void AddStaticTest(string op1String, string op2String, string expected)
        {
            using GmpRational op1 = GmpRational.Parse(op1String);
            using GmpRational op2 = GmpRational.Parse(op2String);
            using GmpRational result = GmpRational.Add(op1, op2);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("-1/3", "1/2", "1/6")]
        [InlineData("-1/3000000000000000000000000000000000000000000", "1/3000000000000000000000000000000000000000000", "0")]
        public void AddInplaceTest(string op1String, string op2String, string expected)
        {
            using GmpRational op1 = GmpRational.Parse(op1String);
            using GmpRational op2 = GmpRational.Parse(op2String);
            using GmpRational result = new GmpRational();

            GmpRational.AddInplace(result, op1, op2);
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData("-1/3", "1/2", "1/6")]
        [InlineData("-1/3000000000000000000000000000000000000000000", "1/3000000000000000000000000000000000000000000", "0")]
        public void SubtractTest(string op1String, string op2String, string expected)
        {
            using GmpRational op1 = GmpRational.Parse(op1String);
            using GmpRational op2 = GmpRational.Parse(op2String);
            using GmpRational result = op1 + op2;
            Assert.Equal(expected, result.ToString());
        }
    }
}
