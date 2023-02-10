using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpRationalIOTest
{
    private readonly ITestOutputHelper _console;

    public GmpRationalIOTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(65535, 0, "65535")]
    [InlineData(0, 0, "0")]
    [InlineData(-3, 10, "-3")]
    [InlineData(255, 2, "11111111")]
    [InlineData(-255, 16, "-ff")]
    [InlineData(1.25, 10, "5/4")]
    [InlineData(0.2, 10, "3602879701896397/18014398509481984")]
    public void ToStringTest(double val, int opBase, string expected)
    {
        GmpRational z = GmpRational.From(val);
        Assert.Equal(expected, z.ToString(opBase));
    }

    [Theory]
    [InlineData("3/4", "3/4")]
    [InlineData("-6/8", "-3/4")]
    [InlineData("6/-8", "-3/4")]
    [InlineData("0/5", "0")]
    public void Canonicalize(string raw, string res)
    {
        GmpRational op = GmpRational.Parse(raw);
        op.Canonicalize();
        Assert.Equal(res, op.ToString());
    }

    [Fact]
    public void ConvertFromTest()
    {
        using GmpRational si = -5;
        using GmpRational d = (GmpRational)1.25;
        using GmpRational z = (GmpInteger)99;
        using GmpRational f = (GmpRational)(GmpFloat)99;

        Assert.Equal(-5, si.ToDouble());
        Assert.Equal(1.25, d.ToDouble());
        Assert.Equal(99, z.ToDouble());
        Assert.Equal(99, f.ToDouble());
    }

    [Fact]
    public void CloneTest()
    {
        using GmpRational q = GmpRational.From(3, 4);
        using GmpRational q2 = q.Clone();
        Assert.Equal(0.75, q2.ToDouble());

        q2.Den.Assign(8);
        Assert.Equal(0.125, q2.ToDouble());
        Assert.Equal(0.75, q.ToDouble());
    }
}