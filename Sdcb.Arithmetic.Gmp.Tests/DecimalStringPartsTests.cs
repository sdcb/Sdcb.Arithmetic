namespace Sdcb.Arithmetic.Gmp.Tests
{
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
    }
}
