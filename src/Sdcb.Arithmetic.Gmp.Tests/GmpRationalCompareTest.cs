using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpRationalCompareTest
{
    private readonly ITestOutputHelper _console;

    public GmpRationalCompareTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData("3/4", "6/8", 0)]
    [InlineData("2/4", "6/8", -1)]
    [InlineData("7/8", "6/8", 1)]
    public void FastCompare(string op1str, string op2str, int res)
    {
        GmpRational op1 = GmpRational.Parse(op1str);
        GmpRational op2 = GmpRational.Parse(op2str);
        Assert.Equal(res, GmpRational.Compare(op1, op2));
    }
}