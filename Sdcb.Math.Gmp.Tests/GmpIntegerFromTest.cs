using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests;

public class GmpIntegerFromTest
{
    private readonly ITestOutputHelper _console;

    public GmpIntegerFromTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void FromBigInteger()
    {
        string num = "1234567890123456789012345678901234567890";
        using GmpInteger a = GmpInteger.Parse(num);
        using GmpInteger b = GmpInteger.From(a);
        Assert.Equal(num, b.ToString());
    }

    [Fact]
    public void FromInt()
    {
        int num = -2147483647;
        using GmpInteger b = GmpInteger.From(num);
        Assert.Equal(num, b.ToInt32());
    }

    [Fact]
    public void FromUInt()
    {
        uint num = 2147483647;
        using GmpInteger b = GmpInteger.From(num);
        Assert.Equal(num, b.ToUInt32());
    }

    [Fact]
    public void FromDouble()
    {
        double num = 3.14;
        using GmpInteger b = GmpInteger.From(num);
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
        if (GmpInteger.TryParse(num, out GmpInteger? result, @base))
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
            GmpInteger r = GmpInteger.Parse(num, @base);
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
        GmpInteger a = GmpInteger.From(42);
        GmpInteger b = new();
        b.Assign(a);
        Assert.Equal(42, b.ToInt32());
    }

    [Fact]
    public void AssignUInt()
    {
        GmpInteger b = new();
        b.Assign(42u);
        Assert.Equal(42u, b.ToUInt32());
    }

    [Fact]
    public void AssignInt()
    {
        GmpInteger b = new();
        b.Assign(-2147483647);
        Assert.Equal(-2147483647, b.ToInt32());
    }

    [Fact]
    public void AssignDouble()
    {
        GmpInteger b = new();
        b.Assign(3.14);
        Assert.Equal(3, b.ToInt32());
    }

    [Fact]
    public void AssignBigRaltional()
    {
        GmpRational a = GmpRational.From(12, 4);
        GmpInteger b = new();
        b.Assign(a);
        Assert.Equal(3, b.ToInt32());
    }

    [Fact]
    public void AssignBigFloat()
    {
        GmpFloat a = GmpFloat.From(65535.125);
        GmpInteger b = new();
        b.Assign(a);
        Assert.Equal(65535, a.ToInt32());
    }

    [Fact]
    public void AssignString()
    {
        string num = "1234567890123456789012345678901234567890";
        GmpInteger b = new();
        b.Assign(num);
        Assert.Equal(num, b.ToString());
    }
}