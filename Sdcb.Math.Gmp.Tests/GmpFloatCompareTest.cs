using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

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
    [InlineData(3.14, 2, true)]
    [InlineData(2.718, 4, false)]
    [InlineData(113, 113, false)]
    public void GreaterUIntRev(uint op1, double op2, bool check)
    {
        Assert.Equal(check, op1 > GmpFloat.From(op2));
        Assert.Equal(!check, op1 <= GmpFloat.From(op2));
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

        Assert.Equal(!check, GmpFloat.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != double.Parse(op2));
        Assert.Equal(!check, double.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, int.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, uint.Parse(op1) != GmpFloat.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != int.Parse(op2));
        Assert.Equal(!check, GmpFloat.Parse(op1) != uint.Parse(op2));
    }
}