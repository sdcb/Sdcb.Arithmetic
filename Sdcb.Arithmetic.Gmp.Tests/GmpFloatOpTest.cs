using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpFloatOpTest
{
    private readonly ITestOutputHelper _console;

    public GmpFloatOpTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void Add()
    {
        GmpFloat r = GmpFloat.Parse("1.5") + GmpFloat.Parse("3.25");
        Assert.Equal(4.75, r.ToDouble());
    }

    [Fact]
    public void AddInt32()
    {
        GmpFloat r = GmpFloat.Parse("1.5") + 100;
        Assert.Equal(101.5, r.ToDouble());
    }

    [Fact]
    public void Subtract()
    {
        GmpFloat r = GmpFloat.From(7.25) - GmpFloat.From(3.125);
        Assert.Equal(4.125, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32()
    {
        GmpFloat r = GmpFloat.From(7.25) - 10;
        Assert.Equal(-2.75, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32Reverse()
    {
        GmpFloat r = 10 - GmpFloat.From(7.25);
        Assert.Equal(2.75, r.ToDouble());
    }

    [Fact]
    public void Multiply()
    {
        GmpFloat r = GmpFloat.From(2.5) * GmpFloat.From(2.5);
        Assert.Equal(6.25, r.ToDouble());
    }

    [Fact]
    public void MultiplyInt32()
    {
        GmpFloat r = GmpFloat.From(2.5) * 2147483647;
        Assert.Equal(5368709117.5, r.ToDouble());
    }

    [Fact]
    public void Divide()
    {
        GmpFloat r = GmpFloat.Parse(long.MinValue.ToString()) / GmpFloat.From(int.MinValue);
        Assert.Equal(1L << 32, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32()
    {
        GmpFloat r = GmpFloat.Parse((1L << 57).ToString()) / (1u << 31);
        _console.WriteLine(r.ToString());
        Assert.Equal(1 << 26, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32Reverse()
    {
        GmpFloat r = 5 / GmpFloat.From(1 << 10);
        Assert.Equal(0.0048828125, r.ToDouble());
    }

    [Fact]
    public void PowerInt32()
    {
        GmpFloat r = GmpFloat.From(2.5) ^ 10;
        Assert.Equal(9536.7431640625, r.ToDouble());
    }

    [Fact]
    public void Negate()
    {
        GmpFloat r = -GmpFloat.From(2.5);
        Assert.Equal(-2.5, r.ToDouble());
    }

    [Fact]
    public void AddEasier()
    {
        GmpFloat op1 = GmpFloat.From(1.5);
        GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(4.75, GmpFloat.Add(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void AddEasier2()
    {
        GmpFloat op1 = GmpFloat.From(1.5);
        uint op2_uint = 3;
        uint precision = 64;
        Assert.Equal(4.5, GmpFloat.Add(op1, op2_uint, precision).ToDouble());
    }

    [Fact]
    public void SubtractEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(1.5, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void SubtractEasier2()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(1.75, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void SubtractEasier3()
    {
        uint op1 = 7;
        GmpFloat op2 = GmpFloat.From(4.75);
        uint precision = 64;
        Assert.Equal(2.25, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void MultiplyEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(15.4375, GmpFloat.Multiply(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void MultiplyEasier2()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(14.25, GmpFloat.Multiply(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(1.4615384615384615, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier2()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(1.5833333333333333, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier3()
    {
        uint op1 = 6;
        GmpFloat op2 = GmpFloat.From(4.75);
        uint precision = 64;
        Assert.Equal(1.263157894736842, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void PowerEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(107.171875, GmpFloat.Power(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void NegateEasier()
    {
        GmpFloat op1 = GmpFloat.From(1.2345);
        uint precision = 64;
        Assert.Equal(-1.2345, GmpFloat.Negate(op1, precision).ToDouble());
    }

    [Fact]
    public void SqrtEasier()
    {
        GmpFloat op1 = GmpFloat.From(1.2345);
        uint precision = 64;
        Assert.Equal(1.1110805551354051, GmpFloat.Sqrt(op1, precision).ToDouble());
    }

    [Fact]
    public void SqrtEasier2()
    {
        uint op1 = 2;
        uint precision = 64;
        Assert.Equal(1.4142135623730950, GmpFloat.Sqrt(op1, precision).ToDouble());
    }

    [Fact]
    public void AbsEasier()
    {
        GmpFloat op1 = GmpFloat.From(-1.2345);
        uint precision = 64;
        Assert.Equal(1.2345, GmpFloat.Abs(op1, precision).ToDouble());

        op1 = GmpFloat.From(1.2345);
        Assert.Equal(1.2345, GmpFloat.Abs(op1, precision).ToDouble());

        op1 = GmpFloat.From(0);
        Assert.Equal(0, GmpFloat.Abs(op1, precision).ToDouble());
    }

    [Fact]
    public void Mul2ExpEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(38, GmpFloat.Mul2Exp(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void Div2ExpEasier()
    {
        GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(0.59375, GmpFloat.Div2Exp(op1, op2, precision).ToDouble());
    }
}