using System.Globalization;

namespace Sdcb.Arithmetic.Gmp.Tests
{
    public class NumberFormatterTests
    {
        [Theory]
        [InlineData("N0", 'N', 0)]
        [InlineData("A5B", 'A', 5)]
        [InlineData("C123D", 'C', 123)]
        [InlineData("E", 'E', 0)]
        [InlineData("", 'G', 0)]
        public void TestSplitLetterAndNumber(string input, char expectedLetter, int expectedNumber)
        {
            (char letter, int number) = NumberFormatter.SplitLetterAndNumber(input);

            Assert.Equal(expectedLetter, letter);
            Assert.Equal(expectedNumber, number);
        }

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
            DecimalStringParts result = NumberFormatter.SplitNumberString(numberString, decimalPosition);
            Assert.Equal(expectedIsNegative, result.IsNegative);
            Assert.Equal(expectedIntegerPart, result.IntegerPart);
            Assert.Equal(expectedDecimalPart, result.DecimalPart);
        }

        [Theory]
        [InlineData(null, 2)]
        public void TestSplitNumberString_ThrowsArgumentException(string numberString, int decimalPosition)
        {
            Assert.Throws<ArgumentNullException>(() => NumberFormatter.SplitNumberString(numberString, decimalPosition));
        }

        [Theory]
        [InlineData(false, "123", "4", 2, "123.40")]
        [InlineData(true, "0", "12345", 2, "-0.12")]
        [InlineData(false, "123456789", "9876543210", 4, "123,456,789.9876")]
        [InlineData(true, "123", "", 2, "-123.00")]
        [InlineData(false, "123456789", "", 0, "123,456,789")]
        [InlineData(false, "1234", "567", 2, "1,234.56")]
        [InlineData(true, "1234", "567", 2, "-1,234.56")]
        [InlineData(false, "123", "4567", 4, "123.4567")]
        [InlineData(false, "12345678901234567890", "12345678901234567890", 20, "12,345,678,901,234,567,890.12345678901234567890")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 5, "12,345,678,901,234,567,890.12345")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 10, "12,345,678,901,234,567,890.1234567890")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 8, "12,345,678,901,234,567,890.12345678")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 2, "12,345,678,901,234,567,890.12")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 1, "12,345,678,901,234,567,890.1")]
        [InlineData(false, "12345678901234567890", "1234567890123456789", 0, "12,345,678,901,234,567,890")]
        [InlineData(true, "12345678901234567890", "1234567890123456789", 10, "-12,345,678,901,234,567,890.1234567890")]
        [InlineData(true, "12345678901234567890", "1234567890123456789", 8, "-12,345,678,901,234,567,890.12345678")]
        [InlineData(true, "12345678901234567890", "1234567890123456789", 2, "-12,345,678,901,234,567,890.12")]
        public void TestFormatAsGroupedInteger(bool isNegative, string integerPart, string decimalPart, int decimalLength, string expected)
        {
            // Arrange
            DecimalStringParts parts = new DecimalStringParts(isNegative, integerPart, decimalPart);

            // Act
            string actual = parts.FormatN(decimalLength, NumberFormatInfo.InvariantInfo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(false, null, "123", 2)]
        [InlineData(false, "", "123", 2)]
        [InlineData(false, " ", "123", 2)]
        [InlineData(false, "123", null, 2)]
        public void TestFormatAsGroupedInteger_ThrowsException(bool isNegative, string integerPart, string decimalPart, int decimalLength)
        {
            // Arrange
            DecimalStringParts parts = new DecimalStringParts(isNegative, integerPart, decimalPart);

            // Act and Assert
            Assert.ThrowsAny<ArgumentException>(() => 
                parts.FormatN(decimalLength, NumberFormatInfo.InvariantInfo));
        }
    }
}
