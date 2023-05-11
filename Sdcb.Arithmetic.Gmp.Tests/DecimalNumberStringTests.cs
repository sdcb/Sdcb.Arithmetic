namespace Sdcb.Arithmetic.Gmp.Tests
{
    public class DecimalNumberStringTests
    {
        [Theory]
        [InlineData("123456", 2, false, "12", "3456")]
        [InlineData("-123456", 2, true, "12", "3456")]
        [InlineData("123456", 0, false, "0", "123456")]
        [InlineData("-123456", 0, true, "0", "123456")]
        [InlineData("0", 0, false, "0", "")]
        [InlineData("123456", 6, false, "123456", "")]
        [InlineData("-123456", 6, true, "123456", "")]
        [InlineData("0012345600", 2, false, "0", "123456")]
        [InlineData("-0012345600", 2, true, "0", "123456")]
        [InlineData("000123456000", 6, false, "123", "456")]
        [InlineData("-000123456000", 6, true, "123", "456")]
        [InlineData("7", 4, false, "7000", "")]
        public void TestSplitNumberString(string numberString, int decimalPosition, bool expectedIsNegative, string expectedIntegerPart, string expectedDecimalPart)
        {
            DecimalStringParts result = new DecimalNumberString(numberString, decimalPosition).SplitNumberString();
            Assert.Equal(expectedIsNegative, result.IsNegative);
            Assert.Equal(expectedIntegerPart, result.IntegerPart);
            Assert.Equal(expectedDecimalPart, result.DecimalPart);
        }

        [Theory]
        [InlineData(null, 2)]
        public void TestSplitNumberString_ThrowsArgumentException(string numberString, int decimalPosition)
        {
            Assert.Throws<ArgumentNullException>(() => new DecimalNumberString(numberString, decimalPosition).SplitNumberString());
        }
    }
}
