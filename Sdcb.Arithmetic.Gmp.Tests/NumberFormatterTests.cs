using System.Globalization;

namespace Sdcb.Arithmetic.Gmp.Tests;

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
}
