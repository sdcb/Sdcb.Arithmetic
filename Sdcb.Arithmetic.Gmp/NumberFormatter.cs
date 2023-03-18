using System;
using System.Globalization;
using System.Text;

namespace Sdcb.Arithmetic.Gmp
{
    internal static class NumberFormatter
    {
        public static (char letter, int number) SplitLetterAndNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return ('G', 0);
            }

            char letter = input[0];
            int number = 0;
            int startIndex = 1;

            if (input.Length > 1 && char.IsDigit(input[1]))
            {
                int endIndex = startIndex;
                while (endIndex < input.Length && char.IsDigit(input[endIndex]))
                {
                    endIndex++;
                }
                number = int.Parse(input.Substring(startIndex, endIndex - startIndex));
            }

            return (letter, number);
        }

        public static (bool isNegative, string integerPart, string decimalPart) SplitNumberString(string numberString, int decimalPosition)
        {
            if (string.IsNullOrEmpty(numberString) || decimalPosition < 0)
            {
                throw new ArgumentException("Invalid input.");
            }

            bool isNegative = false;
            if (numberString.StartsWith("-"))
            {
                isNegative = true;
                numberString = numberString.Substring(1);
            }

            if (numberString.Length == 0 || !IsAllDigits(numberString))
            {
                throw new ArgumentException("Invalid input: not all characters are digits.");
            }

            string integerPart = numberString.Substring(0, decimalPosition).TrimStart('0');
            string decimalPart = numberString.Substring(decimalPosition).TrimEnd('0');

            if (integerPart.Length == 0)
            {
                integerPart = "0";
            }

            return (isNegative, integerPart, decimalPart);

            static bool IsAllDigits(string str)
            {
                foreach (char c in str)
                {
                    if (!char.IsDigit(c))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static string FormatAsGroupedInteger(bool isNegative, string integerPart, string decimalPart, int decimalLength, NumberFormatInfo formatInfo)
        {
            if (string.IsNullOrWhiteSpace(integerPart)) throw new ArgumentException(nameof(integerPart));
            if (decimalPart == null) throw new ArgumentNullException(nameof(decimalPart));

            StringBuilder sb = new StringBuilder(1 + integerPart.Length + integerPart.Length / 3 + decimalLength + 1);

            if (isNegative)
            {
                sb.Append(formatInfo.NegativeSign);
            }

            for (int i = 0; i < integerPart.Length; ++i)
            {
                sb.Append(integerPart[i]);
                if ((integerPart.Length - i - 1) % formatInfo.NumberGroupSizes[0] == 0 && i != integerPart.Length - 1)
                {
                    sb.Append(formatInfo.NumberGroupSeparator);
                }
            }

            if (decimalLength != 0)
            {
                if (decimalPart.Length <= decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(decimalPart);
                    sb.Append('0', decimalLength - decimalPart.Length);
                }
                else if (decimalPart.Length > decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(decimalPart.Substring(0, decimalLength));
                }
            }

            return sb.ToString();
        }
    }
}
