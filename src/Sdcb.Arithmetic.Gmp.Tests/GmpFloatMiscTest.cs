using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpFloatMiscTest
{
    private readonly ITestOutputHelper _console;

    public GmpFloatMiscTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(3.14, 4)]
    [InlineData(4, 4)]
    [InlineData(-1.3, -1)]
    public void CeilInplaceTest(double op1, double op2)
    {
        GmpFloat resOp1 = GmpFloat.From(op1);
        GmpFloat resOp2 = new();
        GmpFloat.CeilInplace(resOp2, resOp1);
        Assert.Equal(op2, resOp2.ToDouble());
    }

    [Theory]
    [InlineData(3.14, 4)]
    [InlineData(4, 4)]
    [InlineData(-1.3, -1)]
    public void CeilTest(double op1, double op2)
    {
        GmpFloat resOp1 = GmpFloat.From(op1);
        Assert.Equal(GmpFloat.From(op2), GmpFloat.Ceil(resOp1));
    }
}