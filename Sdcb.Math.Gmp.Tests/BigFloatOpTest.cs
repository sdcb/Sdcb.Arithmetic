using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class BigFloatOpTest
{
    private readonly ITestOutputHelper _console;

    public BigFloatOpTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void Add()
    {
        BigFloat r = BigFloat.Parse("1.5") + BigFloat.Parse("3.25");
        Assert.Equal(4.75, r.ToDouble());
    }

    [Fact]
    public void AddInt32()
    {
        BigFloat r = BigFloat.Parse("1.5") + 100;
        Assert.Equal(101.5, r.ToDouble());
    }

    [Fact]
    public void Subtract()
    {
        BigFloat r = BigFloat.From(7.25) - BigFloat.From(3.125);
        Assert.Equal(4.125, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32()
    {
        BigFloat r = BigFloat.From(7.25) - 10;
        Assert.Equal(-2.75, r.ToDouble());
    }

    [Fact]
    public void SubtractInt32Reverse()
    {
        BigFloat r = 10 - BigFloat.From(7.25);
        Assert.Equal(2.75, r.ToDouble());
    }

    [Fact]
    public void Multiple()
    {
        BigFloat r = BigFloat.From(2.5) * BigFloat.From(2.5);
        Assert.Equal(6.25, r.ToDouble());
    }

    [Fact]
    public void MultipleInt32()
    {
        BigFloat r = BigFloat.From(2.5) * 2147483647;
        Assert.Equal(5368709117.5, r.ToDouble());
    }

    [Fact]
    public void Divide()
    {
        BigFloat r = BigFloat.Parse(long.MinValue.ToString()) / BigFloat.From(int.MinValue);
        Assert.Equal(1L << 32, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32()
    {
        BigFloat r = BigFloat.Parse((1L << 57).ToString()) / (1u << 31);
        _console.WriteLine(r.ToString());
        Assert.Equal(1 << 26, r.ToDouble());
    }

    [Fact]
    public void DivideUInt32Reverse()
    {
        BigFloat r = 5 / BigFloat.From(1 << 10);
        Assert.Equal(0.0048828125, r.ToDouble());
    }

    [Fact]
    public void PowerInt32()
    {
        BigFloat r = BigFloat.From(2.5) ^ 10;
        Assert.Equal(9536.7431640625, r.ToDouble());
    }

    [Fact]
    public void Negate()
    {
        BigFloat r = -BigFloat.From(2.5);
        Assert.Equal(-2.5, r.ToDouble());
    }
}