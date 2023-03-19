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

        /// <summary>
        /// Split number into integer part, decimal part and sign.
        /// </summary>
        /// <param name="numberString">the whole string, might starts with '-' means negative, without decimal point</param>
        /// <param name="decimalPosition">the decimal point position</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static DecimalStringParts SplitNumberString(string numberString, int decimalPosition)
        {
            if (numberString == null) throw new ArgumentNullException(nameof(numberString));

            bool isNegative = false;
            if (numberString.StartsWith("-"))
            {
                isNegative = true;
                numberString = numberString.Substring(1);
            }

            if (numberString.Length < decimalPosition)
            {
                numberString = numberString + new string('0', decimalPosition - numberString.Length);
            }

            if (decimalPosition < 0)
            {
                numberString = new string('0', -decimalPosition) + numberString;
                decimalPosition = 0;
            }

            string integerPart = numberString.Substring(0, decimalPosition).TrimStart('0');
            string decimalPart = numberString.Substring(decimalPosition).TrimEnd('0');

            if (integerPart.Length == 0)
            {
                integerPart = "0";
            }

            return new (isNegative, integerPart, decimalPart);
        }
    }

    internal record struct DecimalExpParts(bool IsNegative, string IntegerPart, string DecimalPart, int Exp)
    {
        public string FormatE(char E, int pad0, int decimalLength, NumberFormatInfo formatInfo)
        {
            // 截取所需的小数位数
            string adjustedDecimalPart = decimalLength > 0
                ? (DecimalPart.Length > decimalLength ? DecimalPart.Substring(0, decimalLength) : DecimalPart.PadRight(decimalLength, '0'))
                : "";

            // 构建科学计数法字符串
            string sign = IsNegative ? formatInfo.NegativeSign : "";
            string formattedNumber = $"{sign}{IntegerPart}";
            if (decimalLength > 0)
            {
                formattedNumber += $"{formatInfo.NumberDecimalSeparator}{adjustedDecimalPart}";
            }
            string exponentSign = Exp >= 0 ? formatInfo.PositiveSign : formatInfo.NegativeSign;
            string exp = Math.Abs(Exp).ToString(new string('0', pad0));
            string formattedExponent = $"{E}{exponentSign}{exp}";

            return formattedNumber + formattedExponent;
        }
    }

    internal record struct DecimalStringParts(bool IsNegative, string IntegerPart, string DecimalPart)
    {
        public string Format0(NumberFormatInfo formatInfo)
        {
            if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
            if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

            StringBuilder sb = new StringBuilder((IsNegative ? 1 : 0) + IntegerPart.Length + DecimalPart.Length + 1);

            if (IsNegative)
            {
                sb.Append(formatInfo.NegativeSign);
            }

            sb.Append(IntegerPart);
            AppendDecimalPart(DecimalPart.Length, formatInfo, sb);

            return sb.ToString();
        }

        public string FormatN(int decimalLength, NumberFormatInfo formatInfo)
        {
            if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
            if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

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
            if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
            if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

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

        public string FormatC(int decimalLength, NumberFormatInfo formatInfo)
        {
            if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
            if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

            if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
            if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

            StringBuilder sb = new StringBuilder(1 + IntegerPart.Length + IntegerPart.Length / 3 + decimalLength + 1);

            sb.Append(formatInfo.CurrencySymbol);

            for (int i = 0; i < IntegerPart.Length; ++i)
            {
                sb.Append(IntegerPart[i]);
                if ((IntegerPart.Length - i - 1) % formatInfo.NumberGroupSizes[0] == 0 && i != IntegerPart.Length - 1)
                {
                    sb.Append(formatInfo.CurrencyGroupSeparator);
                }
            }

            if (decimalLength != 0)
            {
                if (DecimalPart.Length <= decimalLength)
                {
                    sb.Append(formatInfo.CurrencyDecimalSeparator);
                    sb.Append(DecimalPart);
                    sb.Append('0', decimalLength - DecimalPart.Length);
                }
                else if (DecimalPart.Length > decimalLength)
                {
                    sb.Append(formatInfo.CurrencyDecimalSeparator);
                    sb.Append(DecimalPart.Substring(0, decimalLength));
                }
            }

            return IsNegative ? $"({sb})" : sb.ToString();
        }

        public DecimalExpParts ToExpParts()
        {
            string combinedNumber = IntegerPart + DecimalPart;
            int exp = IntegerPart.Length - 1;

            // 如果整数部分为0，需要找到第一个非零数字来计算指数
            if (IntegerPart == "0")
            {
                int firstNonZeroIndex = -1;
                for (int i = 0; i < DecimalPart.Length; i++)
                {
                    if (DecimalPart[i] != '0')
                    {
                        firstNonZeroIndex = i;
                        break;
                    }
                }

                if (firstNonZeroIndex != -1)
                {
                    exp = -firstNonZeroIndex - 1;
                    combinedNumber = DecimalPart.Substring(firstNonZeroIndex);
                }
            }

            // 找到整数部分的第一个非零数字
            string integerPartInExp = combinedNumber.Substring(0, 1);
            string decimalPartInExp = combinedNumber.Substring(1);

            return new DecimalExpParts(IsNegative, integerPartInExp, decimalPartInExp, exp);
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
