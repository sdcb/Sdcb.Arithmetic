using System;
using System.Globalization;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

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
            number = int.Parse(input[startIndex..endIndex]);
        }

        return (letter, number);
    }
}

/// <param name="NumberString">the whole string, might starts with '-' means negative, without decimal point</param>
/// <param name="DecimalPosition">the decimal point position</param>
internal record struct DecimalNumberString(string? NumberString, int DecimalPosition)
{
    /// <summary>
    /// Split number into integer part, decimal part and sign.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public DecimalStringParts SplitNumberString()
    {
        if (NumberString == null) throw new ArgumentNullException(nameof(DecimalPosition));

        bool isNegative = false;
        if (NumberString.StartsWith("-"))
        {
            isNegative = true;
            NumberString = NumberString[1..];
        }

        if (NumberString.Length < DecimalPosition)
        {
            NumberString += new string('0', DecimalPosition - NumberString.Length);
        }

        if (DecimalPosition < 0)
        {
            NumberString = new string('0', -DecimalPosition) + NumberString;
            DecimalPosition = 0;
        }

        string integerPart = NumberString[..DecimalPosition].TrimStart('0');
        string decimalPart = NumberString[DecimalPosition..].TrimEnd('0');

        if (integerPart.Length == 0)
        {
            integerPart = "0";
        }

        return new(isNegative, integerPart, decimalPart);
    }
}

internal record struct DecimalExpParts(bool IsNegative, string IntegerPart, string DecimalPart, int Exp)
{
    public readonly string FormatE(char E, int pad0, int decimalLength, NumberFormatInfo formatInfo)
    {
        // 截取所需的小数位数
        string adjustedDecimalPart = decimalLength > 0
            ? (DecimalPart.Length > decimalLength ? DecimalPart[..decimalLength] : DecimalPart.PadRight(decimalLength, '0'))
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
    public readonly string Format0(NumberFormatInfo formatInfo)
    {
        if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
        if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

        StringBuilder sb = new((IsNegative ? 1 : 0) + IntegerPart.Length + DecimalPart.Length + 1);

        if (IsNegative)
        {
            sb.Append(formatInfo.NegativeSign);
        }

        sb.Append(IntegerPart);
        AppendDecimalPart(DecimalPart.Length, formatInfo, sb);

        return sb.ToString();
    }

    public readonly string FormatN(int decimalLength, NumberFormatInfo formatInfo)
    {
        if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
        if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

        if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
        if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

        StringBuilder sb = new(1 + IntegerPart.Length + IntegerPart.Length / 3 + decimalLength + 1);

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
                sb.Append(DecimalPart[..decimalLength]);
            }
        }

        return sb.ToString();
    }

    public readonly string FormatF(int decimalLength, NumberFormatInfo formatInfo)
    {
        if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
        if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

        if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
        if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

        StringBuilder sb = new(1 + IntegerPart.Length + decimalLength + 1);

        if (IsNegative)
        {
            sb.Append(formatInfo.NegativeSign);
        }

        sb.Append(IntegerPart);
        AppendDecimalPart(decimalLength, formatInfo, sb);

        return sb.ToString();
    }

    public readonly string FormatC(int decimalLength, NumberFormatInfo formatInfo)
    {
        if (DecimalPart == "@Inf@") return IsNegative ? formatInfo.NegativeInfinitySymbol : formatInfo.PositiveInfinitySymbol;
        if (DecimalPart == "@NaN@") return formatInfo.NaNSymbol;

        if (string.IsNullOrWhiteSpace(IntegerPart)) throw new ArgumentException(nameof(IntegerPart));
        if (DecimalPart == null) throw new ArgumentNullException(nameof(DecimalPart));

        StringBuilder sb = new(1 + IntegerPart.Length + IntegerPart.Length / 3 + decimalLength + 1);

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
                sb.Append(DecimalPart[..decimalLength]);
            }
        }

        return IsNegative ? $"({sb})" : sb.ToString();
    }

    public readonly DecimalExpParts ToExpParts()
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
                combinedNumber = DecimalPart[firstNonZeroIndex..];
            }
        }

        // 找到整数部分的第一个非零数字
        string integerPartInExp = combinedNumber[..1];
        string decimalPartInExp = combinedNumber[1..];

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
                sb.Append(DecimalPart[..decimalLength]);
            }
        }
    }
}
