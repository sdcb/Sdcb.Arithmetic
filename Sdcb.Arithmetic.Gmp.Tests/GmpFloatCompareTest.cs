using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpFloatCompareTest
{
    private readonly ITestOutputHelper _console;

    public GmpFloatCompareTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(3.14, 2.718, 1)]
    [InlineData(2.718, 3.14, -1)]
    [InlineData(3.14, 3.14, 0)]
    public void CompareBigFloat(double op1, double op2, int r)
    {
        Assert.Equal(r, GmpFloat.Compare(GmpFloat.From(op1), GmpFloat.From(op2)));
    }

    [Theory]
    [InlineData(3.14, 2.718, 1)]
    [InlineData(2.718, 3.14, -1)]
    [InlineData(3.14, 3.14, 0)]
    public void CompareDouble(double op1, double op2, int r)
    {
        Assert.Equal(r, GmpFloat.Compare(GmpFloat.From(op1), op2));
    }

    [Theory]
    [InlineData(3.14, 2, 1)]
    [InlineData(2.718, 4, -1)]
    [InlineData(-3, -3, 0)]
    public void CompareInt(double op1, int op2, int r)
    {
        Assert.Equal(r, GmpFloat.Compare(GmpFloat.From(op1), op2));
    }

    [Theory]
    [InlineData(3.14, 2, 1)]
    [InlineData(2.718, 4, -1)]
    [InlineData(65535, 65535, 0)]
    public void CompareUInt(double op1, uint op2, int r)
    {
        Assert.Equal(r, GmpFloat.Compare(GmpFloat.From(op1), op2));
    }

    [Theory]
    [InlineData(3.14, 2, 1)]
    [InlineData(2.718, 4, -1)]
    [InlineData(65535, 65535, 0)]
    public void CompareInteger(double op1, uint op2, int r)
    {
        Assert.Equal(r, GmpFloat.Compare(GmpFloat.From(op1), GmpInteger.From(op2)));
    }

    [Fact]
    public void EqualsNullTest()
    {
        GmpFloat equals = new GmpFloat();
        Assert.False(equals.Equals(null));
    }

    [Theory]
    [InlineData(3.14)]
    public void EqualsFloatAndDoubleTest(double val)
    {
        GmpFloat equals = GmpFloat.From(val);
        Assert.True(equals.Equals(GmpFloat.From(val)));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3.14)]
    public void EqualsIntegerAndIntTest(double val)
    {
        GmpInteger equals = GmpInteger.From(val);
        Assert.True(equals.Equals(GmpInteger.From(val)));
        Assert.True(equals.Equals((int)val));
        Assert.True(equals.Equals((uint)val));
    }

    [Theory]
    [InlineData("3.14")]
    [InlineData("233")]
    public void EqualsStringTest(string val)
    {
        GmpFloat equals = GmpFloat.Parse(val);
        Assert.False(equals.Equals(val));
    }

    [Theory]
    [InlineData(3.14, 3.14, true)]
    [InlineData(3.14, 4.29, false)]
    public void GetHashCodeTest(double op1, double op2, bool check)
    {
        int hashCodeOp1 = GmpFloat.From(op1).GetHashCode();
        int hashCodeOp2 = GmpFloat.From(op2).GetHashCode();
        Assert.Equal(check, hashCodeOp1 == hashCodeOp2);
    }


    [Theory]
    [InlineData(3.14, 2.718, true)]
    [InlineData(2.718, 3.14, false)]
    [InlineData(3.14, 3.14, false)]
    public void GreaterBigFloat(double op1, double op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > GmpFloat.From(op2));
        Assert.Equal(check, GmpFloat.From(op2) < GmpFloat.From(op1));
    }

    [Theory]
    [InlineData(3.14, 2.718, true)]
    [InlineData(2.718, 3.14, false)]
    [InlineData(3.14, 3.14, true)]
    public void GreaterOrEqualBigFloat(double op1, double op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) >= GmpFloat.From(op2));
        Assert.Equal(check, GmpFloat.From(op2) <= GmpFloat.From(op1));
        Assert.Equal(check, GmpFloat.From(op1) >= op2);
        Assert.Equal(check, GmpFloat.From(op2) <= op1);
    }

    [Theory]
    [InlineData(3, 2, true)]
    [InlineData(2, 4, false)]
    [InlineData(100, 100, false)]
    public void GreaterThanInt32(int op1, int op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > op2);
        Assert.Equal(!check, GmpFloat.From(op1) <= op2);
        Assert.Equal(check, GmpFloat.From(op2) < op1);
        Assert.Equal(!check, GmpFloat.From(op2) >= op1);
    }

    [Theory]
    [InlineData(3.14, 2.718, true)]
    [InlineData(2.718, 3.14, false)]
    [InlineData(3.14, 3.14, false)]
    public void GreaterDouble(double op1, double op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > op2);
        Assert.Equal(check, GmpFloat.From(op2) < op1);
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(-7, -7, false)]
    public void GreaterInt(double op1, int op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > op2);
        Assert.Equal(check, GmpFloat.From(op2) < op1);
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(113, 113, false)]
    public void GreaterUInt(double op1, uint op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > op2);
        Assert.Equal(check, GmpFloat.From(op2) < op1);
        Assert.Equal(check, op1 > GmpFloat.From(op2));
        Assert.Equal(check, op2 < GmpFloat.From(op1));

        Assert.Equal(!check, GmpFloat.From(op1) <= op2);
        Assert.Equal(!check, GmpFloat.From(op2) >= op1);
        Assert.Equal(!check, op1 <= GmpFloat.From(op2));
        Assert.Equal(!check, op2 >= GmpFloat.From(op1));
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(113, 113, false)]
    public void GreaterInteger(double op1, uint op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) > GmpInteger.From(op2));
        Assert.Equal(check, GmpFloat.From(op2) < GmpInteger.From(op1));

        Assert.Equal(!check, GmpFloat.From(op1) <= GmpInteger.From(op2));
        Assert.Equal(!check, GmpFloat.From(op2) >= GmpInteger.From(op1));
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(113, 113, false)]
    public void GreaterFloat(double op1, uint op2, bool check)
    {
        Assert.Equal(check, GmpInteger.From(op1) > GmpFloat.From(op2));
        Assert.Equal(check, GmpInteger.From(op2) < GmpFloat.From(op1));

        Assert.Equal(!check, GmpInteger.From(op1) <= GmpFloat.From(op2));
        Assert.Equal(!check, GmpInteger.From(op2) >= GmpFloat.From(op1));
    }


    [Theory]
    [InlineData(3.14, 2.718, true)]
    [InlineData(2.718, 3.14, false)]
    [InlineData(3.14, 3.14, false)]
    public void GreaterDoubleRev(double op1, double op2, bool check)
    {
        Assert.Equal(check, op1 > GmpFloat.From(op2));
        Assert.Equal(!check, op1 <= GmpFloat.From(op2));
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(-7, -7, false)]
    public void GreaterIntRev(int op1, double op2, bool check)
    {
        Assert.Equal(check, op1 > GmpFloat.From(op2));
        Assert.Equal(!check, op1 <= GmpFloat.From(op2));
    }

    [Theory]
    [InlineData(3.14, 2, false)]
    [InlineData(2.718, 4, true)]
    [InlineData(113, 113, false)]
    public void GreaterUIntRev(double op1, uint op2, bool check)
    {
        Assert.Equal(check, GmpFloat.From(op1) < op2);
        Assert.Equal(check, op1 < GmpFloat.From(op2));
        Assert.Equal(check, op2 > GmpFloat.From(op1));

        Assert.Equal(!check, GmpFloat.From(op1) >= op2);
        Assert.Equal(!check, op1 >= GmpFloat.From(op2));
        Assert.Equal(!check, op2 <= GmpFloat.From(op1));
    }

    [Theory]
    [InlineData(3.14, 2.718, true)]
    [InlineData(2.718, 3.14, false)]
    [InlineData(3.14, 3.14, true)]
    public void GreaterOrEqualDoubleRev(double op1, double op2, bool check)
    {
        Assert.Equal(check, op1 >= GmpFloat.From(op2));
        Assert.Equal(!check, op1 < GmpFloat.From(op2));
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(-7, -7, true)]
    public void GreaterOrEqualIntRev(int op1, double op2, bool check)
    {
        Assert.Equal(check, op1 >= GmpFloat.From(op2));
        Assert.Equal(!check, op1 < GmpFloat.From(op2));
    }

    [Theory]
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(113, 113, true)]
    public void GreaterOrEqualUIntRev(uint op1, double op2, bool check)
    {
        Assert.Equal(check, op1 >= GmpFloat.From(op2));
        Assert.Equal(!check, op1 < GmpFloat.From(op2));
    }

    [Theory]
    [InlineData("0", "-0", true)]
    [InlineData("16", "3", false)]
    public void EqualTest(string op1, string op2, bool check)
    {
        Assert.Equal(check, GmpFloat.Parse(op1) == GmpFloat.Parse(op2));
        Assert.Equal(check, GmpFloat.Parse(op1) == double.Parse(op2));
        Assert.Equal(check, double.Parse(op1) == GmpFloat.Parse(op2));
        Assert.Equal(check, int.Parse(op1) == GmpFloat.Parse(op2));
        Assert.Equal(check, uint.Parse(op1) == GmpFloat.Parse(op2));
        Assert.Equal(check, GmpFloat.Parse(op1) == int.Parse(op2));
        Assert.Equal(check, GmpFloat.Parse(op1) == uint.Parse(op2));
        Assert.Equal(check, GmpFloat.Parse(op1) == GmpInteger.Parse(op2));
        Assert.Equal(check, GmpInteger.Parse(op1) == GmpFloat.Parse(op2));

        Assert.Equal(!check, GmpFloat.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != double.Parse(op2));
        Assert.Equal(!check, double.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, int.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, uint.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != int.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != uint.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != GmpInteger.Parse(op2));
        Assert.Equal(!check, GmpInteger.Parse(op1) != GmpFloat.Parse(op2));
    }

    [Theory]
    [InlineData("0", "-0", 0)]
    [InlineData("2", "3", -1)]
    [InlineData("5", "1", 1)]
    [Obsolete]
    public void MpfEqualsTest(string op1, string op2, int check)
    {
        Assert.Equal(check, GmpFloat.MpfEquals(GmpFloat.Parse(op1), uint.Parse(op2)));
    }

    [Theory]
    [InlineData(100, 80, 0.2)]
    [InlineData(60, 30, 0.5)]
    [InlineData(80, 100, 0.25)]
    public void RelDiffInplaceTest(double op1, double op2, double check)
    {
        GmpFloat rop = new GmpFloat();
        GmpFloat.RelDiffInplace(rop, GmpFloat.From(op1), GmpFloat.From(op2));
        double res = System.Math.Abs((double)rop) - check;
        Assert.True(System.Math.Abs(res) < 0.000001);
    }

    [Theory]
    [InlineData(100, 80, 0.2)]
    [InlineData(60, 30, 0.5)]
    [InlineData(80, 100, 0.25)]
    public void RelDiffTest(double op1, double op2, double check)
    {
        GmpFloat res = GmpFloat.RelDiff(GmpFloat.From(op1), GmpFloat.From(op2));
        double resAbs = System.Math.Abs((double)res) - check;
        Assert.True(System.Math.Abs(resAbs) < 0.000001);
    }

    [Theory]
    [InlineData("-3.14", -1)]
    [InlineData("777777777777777777777777777777777777777777777777.999999999999999999999", 1)]
    [InlineData("0", 0)]
    public void SignTest(string val, int expectedValue)
    {
        GmpFloat f = GmpFloat.Parse(val);
        Assert.Equal(expectedValue, f.Sign);
    }



}