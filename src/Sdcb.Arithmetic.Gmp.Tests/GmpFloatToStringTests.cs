using System.Globalization;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class GmpFloatToStringTests
{
    private readonly ITestOutputHelper _console;

    public GmpFloatToStringTests(ITestOutputHelper console)
    {
        _console = console;
    }

    [Theory]
    [InlineData(12345.6789, "12,345.67")]  // Test case with default number of decimal places
    [InlineData(12345.6789, "12,345", 0)]  // Test case with zero decimal places
    [InlineData(12345.6789, "12,345.678", 3)]  // Test case with custom number of decimal places
    [InlineData(0, "0.00")]  // Test case with zero value
    [InlineData(-12345.6789, "-12,345.67")]  // Test case with negative value
    public void ToStringWithNFormat_ReturnsExpectedString(double input, string expectedOutput, int? decimalPlaces = null)
    {
        // Arrange
        var formatString = decimalPlaces.HasValue ? $"N{decimalPlaces.Value}" : "N";
        using GmpFloat f = GmpFloat.From(input);

        // Act
        var actualOutput = f.ToString(formatString);

        // Assert
        Assert.Equal(expectedOutput, actualOutput);
    }

    [Theory]
    [InlineData(12345.6789, "12345.67")]  // Test case with default number of decimal places
    [InlineData(12345.6789, "12345", 0)]  // Test case with zero decimal places
    [InlineData(12345.6789, "12345.678", 3)]  // Test case with custom number of decimal places
    [InlineData(0, "0.00")]  // Test case with zero value
    [InlineData(-12345.6789, "-12345.67")]  // Test case with negative value
    public void ToStringWithFFormat_ReturnsExpectedString(double input, string expectedOutput, int? decimalPlaces = null)
    {
        // Arrange
        var formatString = decimalPlaces.HasValue ? $"F{decimalPlaces.Value}" : "F";
        using GmpFloat f = GmpFloat.From(input);

        // Act
        var actualOutput = f.ToString(formatString);

        // Assert
        Assert.Equal(expectedOutput, actualOutput);
    }

    [Theory]
    [InlineData(12345.6789, "1.234567E+004")]  // Test case with default format
    [InlineData(12345.6789, "1E+004", 0)]  // Test case with zero decimal places
    [InlineData(12345.6789, "1.234567E+004", 6)]  // Test case with custom number of decimal places
    [InlineData(0, "0.000000E+000")]  // Test case with zero value
    [InlineData(-12345.6789, "-1.234567E+004")]  // Test case with negative value
    public void ToStringWithEFormat_ReturnsExpectedString(double input, string expectedOutput, int? decimalPlaces = null)
    {
        // Arrange
        var formatString = decimalPlaces.HasValue ? $"E{decimalPlaces.Value}" : "E";
        using GmpFloat f = GmpFloat.From(input);

        // Act
        var actualOutput = f.ToString(formatString);

        // Assert
        Assert.Equal(expectedOutput, actualOutput);
    }
}