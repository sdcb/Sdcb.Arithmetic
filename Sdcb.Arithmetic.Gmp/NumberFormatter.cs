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

        public static DecimalStringParts SplitNumberString((string numberString, int decimalPosition) v) => SplitNumberString(v.numberString, v.decimalPosition);

        public static DecimalStringParts SplitNumberString(string numberString, int decimalPosition)
        {
            if (numberString == null || decimalPosition < 0)
            {
                throw new ArgumentException("Invalid input.");
            }

            bool isNegative = false;
            if (numberString.StartsWith("-"))
            {
                isNegative = true;
                numberString = numberString.Substring(1);
            }

            if (numberString.Length > 0 && !IsAllDigits(numberString))
            {
                throw new ArgumentException("Invalid input: not all characters are digits.");
            }

            string integerPart = numberString.Substring(0, decimalPosition).TrimStart('0');
            string decimalPart = numberString.Substring(decimalPosition).TrimEnd('0');

            if (integerPart.Length == 0)
            {
                integerPart = "0";
            }

            return new (isNegative, integerPart, decimalPart);

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
    }

    internal record struct DecimalStringParts(bool IsNegative, string IntegerPart, string DecimalPart)
    {
        public string FormatN(int decimalLength, NumberFormatInfo formatInfo)
        {
            if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
            if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

            StringBuilder sb = new StringBuilder(1 + IntegerPart.Length + IntegerPart.Length / 3 + decimalLength + 1);

            if (IsNegative)
            {
                sb.Append(formatInfo.NegativeSign);
            }

            for (int i = 0; i < IntegerPart.Length; ++i)
            {
                sb.Append(IntegerPart[i]);
                if ((IntegerPart.Length - i - 1) % formatInfo.NumberGroupSizes[0] == 0 && i != IntegerPart.Length - 1)
                {
                    sb.Append(formatInfo.NumberGroupSeparator);
                }
            }

            if (decimalLength != 0)
            {
                if (DecimalPart.Length <= decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(DecimalPart);
                    sb.Append('0', decimalLength - DecimalPart.Length);
                }
                else if (DecimalPart.Length > decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(DecimalPart.Substring(0, decimalLength));
                }
            }

            return sb.ToString();
        }

        public string FormatF(int decimalLength, NumberFormatInfo formatInfo)
        {
            if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
            if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

            StringBuilder sb = new StringBuilder(1 + IntegerPart.Length + decimalLength + 1);

            if (IsNegative)
            {
                sb.Append(formatInfo.NegativeSign);
            }

            sb.Append(IntegerPart);
            AppendDecimalPart(decimalLength, formatInfo, sb);

            return sb.ToString();
        }

        private readonly void AppendDecimalPart(int decimalLength, NumberFormatInfo formatInfo, StringBuilder sb)
        {
            if (decimalLength != 0)
            {
                if (DecimalPart.Length <= decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(DecimalPart);
                    sb.Append('0', decimalLength - DecimalPart.Length);
                }
                else if (DecimalPart.Length > decimalLength)
                {
                    sb.Append(formatInfo.NumberDecimalSeparator);
                    sb.Append(DecimalPart.Substring(0, decimalLength));
                }
            }
        }
    }
}
