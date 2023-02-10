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
        using GmpFloat r = GmpFloat.Parse("1.5") + GmpFloat.Parse("3.25");
        Assert.Equal(4.75, r.ToDouble());
    }

    [Fact]
    public void AddInt32()
    {
        using GmpFloat r = GmpFloat.Parse("1.5") + 100;
        Assert.Equal(101.5, r.ToDouble());
    }

    [Fact]
    public void Subtract()
    {
        using GmpFloat r = GmpFloat.From(7.25) - GmpFloat.From(3.125);
        Assert.Equal(4.125, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32()
    {
        using GmpFloat r = GmpFloat.From(7.25) - 10;
        Assert.Equal(-2.75, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32Reverse()
    {
        using GmpFloat r = 10 - GmpFloat.From(7.25);
        Assert.Equal(2.75, r.ToDouble());
    }

    [Fact]
    public void Multiply()
    {
        using GmpFloat r = GmpFloat.From(2.5) * GmpFloat.From(2.5);
        Assert.Equal(6.25, r.ToDouble());
    }

    [Fact]
    public void MultiplyInt32()
    {
        using GmpFloat r = GmpFloat.From(2.5) * 2147483647;
        Assert.Equal(5368709117.5, r.ToDouble());
    }

    [Fact]
    public void Divide()
    {
        using GmpFloat r = GmpFloat.Parse(long.MinValue.ToString()) / GmpFloat.From(int.MinValue);
        Assert.Equal(1L << 32, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32()
    {
        using GmpFloat r = GmpFloat.Parse((1L << 57).ToString()) / (1u << 31);
        _console.WriteLine(r.ToString());
        Assert.Equal(1 << 26, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32Reverse()
    {
        using GmpFloat r = 5 / GmpFloat.From(1 << 10);
        Assert.Equal(0.0048828125, r.ToDouble());
    }

    [Fact]
    public void PowerInt32()
    {
        using GmpFloat r = GmpFloat.From(2.5) ^ 10;
        Assert.Equal(9536.7431640625, r.ToDouble());
    }

    [Fact]
    public void Negate()
    {
        using GmpFloat r = -GmpFloat.From(2.5);
        Assert.Equal(-2.5, r.ToDouble());
    }

    [Fact]
    public void AddEasier()
    {
        using GmpFloat op1 = GmpFloat.From(1.5);
        using GmpFloat op2 = GmpFloat.From(3.25);
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
        using GmpFloat op1 = GmpFloat.From(4.75);
        using GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(1.5, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void SubtractEasier2()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(1.75, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void SubtractEasier3()
    {
        uint op1 = 7;
        using GmpFloat op2 = GmpFloat.From(4.75);
        uint precision = 64;
        Assert.Equal(2.25, GmpFloat.Subtract(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void MultiplyEasier()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        using GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(15.4375, GmpFloat.Multiply(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void MultiplyEasier2()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(14.25, GmpFloat.Multiply(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        using GmpFloat op2 = GmpFloat.From(3.25);
        uint precision = 64;
        Assert.Equal(1.4615384615384615, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier2()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(1.5833333333333333, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void DivideEasier3()
    {
        uint op1 = 6;
        using GmpFloat op2 = GmpFloat.From(4.75);
        uint precision = 64;
        Assert.Equal(1.263157894736842, GmpFloat.Divide(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void PowerEasier()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(107.171875, GmpFloat.Power(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void NegateEasier()
    {
        using GmpFloat op1 = GmpFloat.From(1.2345);
        uint precision = 64;
        Assert.Equal(-1.2345, GmpFloat.Negate(op1, precision).ToDouble());
    }

    [Fact]
    public void SqrtEasier()
    {
        using GmpFloat op1 = GmpFloat.From(1.2345);
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

        op1.Assign(1.2345);
        Assert.Equal(1.2345, GmpFloat.Abs(op1, precision).ToDouble());

        op1.Assign(0);
        Assert.Equal(0, GmpFloat.Abs(op1, precision).ToDouble());
    }

    [Fact]
    public void Mul2ExpEasier()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(38, GmpFloat.Mul2Exp(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void Div2ExpEasier()
    {
        using GmpFloat op1 = GmpFloat.From(4.75);
        uint op2 = 3;
        uint precision = 64;
        Assert.Equal(0.59375, GmpFloat.Div2Exp(op1, op2, precision).ToDouble());
    }

    [Fact]
    public void OperatorFromTest()
    {
        using GmpFloat si = -65535;
        using GmpFloat ui = 2147483647;
        using GmpFloat z = GmpInteger.Parse(new string('9', 999));
        Assert.Equal(-65535, (int)si);
        Assert.Equal(2147483647u, (uint)ui);
        Assert.Equal(new string('9', 999), z.ToString());
    }

    [Fact]
    public void CloneTest()
    {
        using GmpFloat f = GmpFloat.From(3.14, 108);
        using GmpFloat f2 = f.Clone();
        Assert.Equal(3.14, f2.ToDouble());
        Assert.Equal(f.Precision, f2.Precision);

        f2.Assign(2.718);
        Assert.Equal(2.718, f2.ToDouble());
        Assert.Equal(3.14, f.ToDouble());
    }
}