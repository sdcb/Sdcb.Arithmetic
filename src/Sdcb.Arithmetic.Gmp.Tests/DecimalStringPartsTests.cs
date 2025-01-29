using System.Globalization;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class DecimalStringPartsTests
{
    [Fact]
    public void ToExpParts_PositiveNumberWithoutLeadingZeros_CorrectConversion()
    {
        // Arrange
        var decimalStringParts = new DecimalStringParts(false, "123", "456");

        // Act
        var result = decimalStringParts.ToExpParts();

        // Assert
        Assert.False(result.IsNegative);
        Assert.Equal("1", result.IntegerPart);
        Assert.Equal("23456", result.DecimalPart);
        Assert.Equal(2, result.Exp);
    }

    [Fact]
    public void ToExpParts_NegativeNumberWithLeadingZeros_CorrectConversion()
    {
        // Arrange
        var decimalStringParts = new DecimalStringParts(true, "0", "00123");

        // Act
        var result = decimalStringParts.ToExpParts();

        // Assert
        Assert.True(result.IsNegative);
        Assert.Equal("1", result.IntegerPart);
        Assert.Equal("23", result.DecimalPart);
        Assert.Equal(-3, result.Exp);
    }

    [Fact]
    public void ToExpParts_Zero_CorrectConversion()
    {
        // Arrange
        var decimalStringParts = new DecimalStringParts(false, "0", "");

        // Act
        var result = decimalStringParts.ToExpParts();

        // Assert
        Assert.False(result.IsNegative);
        Assert.Equal("0", result.IntegerPart);
        Assert.Equal("", result.DecimalPart);
        Assert.Equal(0, result.Exp);
    }

    [Fact]
    public void ToExpParts_OneDecimalDigit_CorrectConversion()
    {
        // Arrange
        var decimalStringParts = new DecimalStringParts(false, "0", "1");

        // Act
        var result = decimalStringParts.ToExpParts();

        // Assert
        Assert.False(result.IsNegative);
        Assert.Equal("1", result.IntegerPart);
        Assert.Equal("", result.DecimalPart);
        Assert.Equal(-1, result.Exp);
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
        DecimalStringParts parts = new(isNegative, integerPart, decimalPart);

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
        DecimalStringParts parts = new(isNegative, integerPart, decimalPart);

        // Act and Assert
        Assert.ThrowsAny<ArgumentException>(() =>
            parts.FormatN(decimalLength, NumberFormatInfo.InvariantInfo));
    }
}
