using System.Globalization;

namespace Sdcb.Arithmetic.Gmp.Tests
{
    public class DecimalExpPartsTests
    {
        [Theory]
        [InlineData(false, "1", "23456", 2, 3, "1.234E+002")]
        [InlineData(true, "1", "23456", 2, 3, "-1.234E+002")]
        [InlineData(false, "1", "23", -2, 3, "1.230E-002")]
        [InlineData(true, "1", "23", -2, 3, "-1.230E-002")]
        [InlineData(false, "1", "23", 1, 1, "1.2E+001")]
        [InlineData(false, "1", "23456789", 4, 0, "1E+004")]
        public void FormatE_TestCases_CorrectFormat(
            bool isNegative,
            string integerPart,
            string decimalPart,
            int exp,
            int decimalLength,
            string expectedResult)
        {
            // Arrange
            var decimalExpParts = new DecimalExpParts(isNegative, integerPart, decimalPart, exp);
            var formatInfo = CultureInfo.InvariantCulture.NumberFormat;

            // Act
            string result = decimalExpParts.FormatE('E', 3, decimalLength, formatInfo);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
