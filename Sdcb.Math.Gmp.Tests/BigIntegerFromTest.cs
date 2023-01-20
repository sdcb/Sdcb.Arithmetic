using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class BigIntegerFromTest
{
    private readonly ITestOutputHelper _console;

    public BigIntegerFromTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void FromBigInteger()
    {
        string num = "1234567890123456789012345678901234567890";
        using BigInteger a = BigInteger.Parse(num);
        using BigInteger b = BigInteger.From(a);
        Assert.Equal(num, b.ToString());
    }

    [Fact]
    public void FromInt()
    {
        int num = -2147483647;
        using BigInteger b = BigInteger.From(num);
        Assert.Equal(num, b.ToInt32());
    }

    [Fact]
    public void FromUInt()
    {
        uint num = 2147483647;
        using BigInteger b = BigInteger.From(num);
        Assert.Equal(num, b.ToUInt32());
    }

    [Fact]
    public void FromDouble()
    {
        double num = 3.14;
        using BigInteger b = BigInteger.From(num);
        Assert.Equal((int)num, b.ToDouble());
    }

    [Theory]
    [InlineData("1234", 0, true)]
    [InlineData("-1234567890123456789012345678901234567890", 0, true)]
    [InlineData("-ff", 16, true)]
    [InlineData("LiGuilin", 62, true)]
    [InlineData("LiGuilin", 63, false)]
    [InlineData("What's the f**k?", 62, false)]
    public void TryParse(string num, int @base, bool good)
    {
        if (BigInteger.TryParse(num, out BigInteger? result, @base))
        {
            Assert.True(good);
            Assert.Equal(num, result.ToString(@base));
        }
        else
        {
            Assert.False(good);
        }
    }

    [Theory]
    [InlineData("1234", 0, true)]
    [InlineData("-1234567890123456789012345678901234567890", 0, true)]
    [InlineData("-ff", 16, true)]
    [InlineData("LiGuilin", 62, true)]
    [InlineData("LiGuilin", 63, false)]
    [InlineData("What's the f**k?", 62, false)]
    public void Parse(string num, int @base, bool good)
    {
        try
        {
            BigInteger r = BigInteger.Parse(num, @base);
            Assert.True(good);
            Assert.Equal(num, r.ToString(@base));
        }
        catch (FormatException)
        {
            Assert.False(good);
        }
    }

    [Fact]
    public void AssignBigInteger()
    {
        BigInteger a = BigInteger.From(42);
        BigInteger b = new();
        b.Assign(a);
        Assert.Equal(42, b.ToInt32());
    }

    [Fact]
    public void AssignUInt()
    {
        BigInteger b = new();
        b.Assign(42u);
        Assert.Equal(42u, b.ToUInt32());
    }

    [Fact]
    public void AssignInt()
    {
        BigInteger b = new();
        b.Assign(-2147483647);
        Assert.Equal(-2147483647, b.ToInt32());
    }

    [Fact]
    public void AssignDouble()
    {
        BigInteger b = new();
        b.Assign(3.14);
        Assert.Equal(3, b.ToInt32());
    }

    [Fact]
    public void AssignBigRaltional()
    {
        BigRational a = BigRational.From(12, 4);
        BigInteger b = new();
        b.Assign(a);
        Assert.Equal(3, b.ToInt32());
    }

    [Fact]
    public void AssignBigFloat()
    {
        BigFloat a = BigFloat.From(65535.125);
        BigInteger b = new();
        b.Assign(a);
        Assert.Equal(65535, a.ToInt32());
    }

    [Fact]
    public void AssignString()
    {
        string num = "1234567890123456789012345678901234567890";
        BigInteger b = new();
        b.Assign(num);
        Assert.Equal(num, b.ToString());
    }
}