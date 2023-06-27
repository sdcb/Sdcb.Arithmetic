using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// An arbitrary-precision floating point number using the GNU Multiple Precision Arithmetic Library.
/// </summary>
public class GmpFloat : IDisposable, IFormattable, IEquatable<GmpFloat>, IComparable, IComparable<GmpFloat>
{
    /// <summary>
    /// The default precision for GmpFloat operations.
    /// </summary>
    public static uint DefaultPrecision
    {
        get => GmpLib.__gmpf_get_default_prec();
        set => GmpLib.__gmpf_set_default_prec(value);
    }

    internal Mpf_t Raw;

    #region Initialization functions

    public unsafe GmpFloat()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_init((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpFloat"/> class with the specified <see cref="Mpf_t "/> structure.
    /// </summary>
    /// <param name="raw">The <see cref="Mpf_t "/> structure to initialize this instance with.</param>
    public GmpFloat(Mpf_t raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpFloat"/> class.
    /// </summary>
    /// <param name="precision">The number of bits to use for the float's mantissa.</param>
    public unsafe GmpFloat(uint precision)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            if (precision == 0)
            {
                GmpLib.__gmpf_init((IntPtr)ptr);
            }
            else
            {
                GmpLib.__gmpf_init2((IntPtr)ptr, precision);
            }
        }
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpFloat"/> with the specified precision, or default precision if <paramref name="precision"/> is null.
    /// </summary>
    /// <param name="precision">The precision of the new instance, or null to use default precision.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> with the specified precision.</returns>
    internal static GmpFloat CreateWithNullablePrecision(uint? precision) => precision switch
    {
        null => new GmpFloat(),
        { } p => new GmpFloat(p)
    };
    #endregion

    #region Combined Initialization and Assignment Functions

    /// <summary>
    /// Creates a new instance of <see cref="GmpFloat"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpFloat"/> that is a copy of the current instance.</returns>
    public GmpFloat Clone()
    {
        GmpFloat rop = new(Precision);
        rop.Assign(this);
        return rop;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a integer <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
    /// </summary>
    /// <param name="val">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    /// <exception cref="System.ArithmeticException">Thrown when the conversion from <paramref name="val"/> to <see cref="GmpFloat"/> fails.</exception>
    public unsafe static GmpFloat From(int val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_si((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public static implicit operator GmpFloat(int val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a integer <paramref name="val"/> with specified <paramref name="precision"/> in bit.
    /// </summary>
    /// <param name="val">The integer value to convert.</param>
    /// <param name="precision">The precision in bit of the new <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(int val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from an unsigned integer <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
    /// </summary>
    /// <param name="val">The unsigned integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(uint val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_ui((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public static implicit operator GmpFloat(uint val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from an unsigned integer <paramref name="val"/> with a specified <paramref name="precision"/> in bit.
    /// </summary>
    /// <param name="val">The unsigned integer value to convert.</param>
    /// <param name="precision">The precision in bit of the new <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(uint val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a double-precision floating-point <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
    /// </summary>
    /// <param name="val">The double-precision floating-point value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(double val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_d((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public static implicit operator GmpFloat(double val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a double <paramref name="val"/> with specified <paramref name="precision"/> in bit.
    /// </summary>
    /// <param name="val">The double value to convert.</param>
    /// <param name="precision">The precision in bit of the new <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(double val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a <see cref="GmpInteger"/> <paramref name="val"/> with a specified <paramref name="precision"/> in bit.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> value to convert.</param>
    /// <param name="precision">The precision in bit of the resulting <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(GmpInteger val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a <see cref="GmpInteger"/> <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(GmpInteger val)
    {
        GmpFloat f = new(precision: (uint)Math.Abs(val.Raw.Size) * GmpLib.LimbBitSize);
        f.Assign(val);
        return f;
    }

    public static implicit operator GmpFloat(GmpInteger val) => From(val);

    /// <summary>
    /// Parses the string representation of a number in the specified base and returns a new instance of <see cref="GmpFloat"/> that represents that number.
    /// </summary>
    /// <param name="val">The string representation of the number to parse.</param>
    /// <param name="base">The base of the number to parse. Default is 10.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> that represents the parsed number.</returns>
    /// <exception cref="FormatException">Thrown when the string representation of the number is not in the correct format.</exception>
    public unsafe static GmpFloat Parse(string val, int @base = 10)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpLib.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, @base);
            if (ret != 0)
            {
                GmpLib.__gmpf_clear((IntPtr)ptr);
                throw new FormatException($"Failed to parse {val}, base={@base} to BigFloat, __gmpf_init_set_str returns {ret}");
            }
        }
        return new GmpFloat(raw);
    }

    /// <summary>
    /// Parses the string representation of a number in a specified base and precision, and returns a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="val">The string representation of the number to parse.</param>
    /// <param name="precision">The precision of the resulting <see cref="GmpFloat"/> in bits.</param>
    /// <param name="base">The base of the number to parse. Default is 10.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the parsed number.</returns>
    public static GmpFloat Parse(string val, uint precision, int @base = 10)
    {
        GmpFloat f = new(precision);
        f.Assign(val, @base);
        return f;
    }

    /// <summary>
    /// Tries to parse a string <paramref name="val"/> to a <see cref="GmpFloat"/> instance with the specified <paramref name="base"/>.
    /// </summary>
    /// <param name="val">The string to parse.</param>
    /// <param name="result">When this method returns, contains the <see cref="GmpFloat"/> instance equivalent to the numeric value or null if the conversion failed.</param>
    /// <param name="base">The base of the number in <paramref name="val"/>, which must be 2, 8, 10, or 16. Default is 10.</param>
    /// <returns>true if <paramref name="val"/> was converted successfully; otherwise, false.</returns>
    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out GmpFloat result, int @base = 10)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpLib.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, @base);
            if (rt != 0)
            {
                GmpLib.__gmpf_clear((IntPtr)ptr);
                result = null;
                return false;
            }
            else
            {
                result = new GmpFloat(raw);
                return true;
            }
        }
    }

    /// <summary>
    /// Tries to parse a string <paramref name="val"/> to a <see cref="GmpFloat"/> instance with the specified <paramref name="precision"/> and <paramref name="base"/>.
    /// </summary>
    /// <param name="val">The string to parse.</param>
    /// <param name="result">When this method returns, contains the <see cref="GmpFloat"/> instance equivalent to the string <paramref name="val"/>, or null if the conversion failed.</param>
    /// <param name="precision">The precision in bit of the <see cref="GmpFloat"/> instance to create.</param>
    /// <param name="base">The base of the number in <paramref name="val"/>. Default is 10.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out GmpFloat result, uint precision, int @base = 10)
    {
        GmpFloat f = new(precision);
        fixed (Mpf_t* pf = &f.Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(val);
            fixed (byte* opBytesPtr = opBytes)
            {
                int ret = GmpLib.__gmpf_set_str((IntPtr)pf, (IntPtr)opBytesPtr, @base);
                if (ret != 0)
                {
                    result = null;
                    f.Dispose();
                    return false;
                }
                else
                {
                    result = f;
                    return true;
                }
            }
        }
    }

    public unsafe uint Precision
    {
        get
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpLib.__gmpf_get_prec((IntPtr)ptr);
            }
        }
        set
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpLib.__gmpf_set_prec((IntPtr)ptr, value);
            }
        }
    }

    /// <summary>
    /// Sets the precision of the current <see cref="GmpFloat"/> instance using the raw number of bits.
    /// </summary>
    /// <param name="value">The number of bits to set as the precision.</param>
    /// <remarks>
    /// This method is obsolete, use <see cref="Precision"/> property instead.
    /// </remarks>
    [Obsolete("use Precision")]
    public unsafe void SetRawPrecision(uint value)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_set_prec_raw((IntPtr)ptr, value);
        }
    }
    #endregion

    #region Assignment functions

    /// <summary>
    /// Assign the value of another <see cref="GmpFloat"/> instance to this instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to assign from.</param>
    public unsafe void Assign(GmpFloat op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpf_t* pthat = &op.Raw)
        {
            GmpLib.__gmpf_set((IntPtr)pthis, (IntPtr)pthat);
        }
    }

    /// <summary>
    /// Assigns the unsigned integer <paramref name="op"/> to the current instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op">The unsigned integer value to assign.</param>
    public unsafe void Assign(uint op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_ui((IntPtr)pthis, op);
        }
    }

    /// <summary>
    /// Assigns an integer value <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The integer value to assign.</param>
    public unsafe void Assign(int op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_si((IntPtr)pthis, op);
        }
    }

    /// <summary>
    /// Assigns a double-precision floating-point value to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The double-precision floating-point value to assign.</param>
    /// <remarks>
    /// This method assigns the value of <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </remarks>
    public unsafe void Assign(double op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_d((IntPtr)pthis, op);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpInteger"/> <paramref name="op"/> to this <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to assign the value from.</param>
    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_z((IntPtr)pthis, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpRational"/> instance to this <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to assign from.</param>
    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_q((IntPtr)pthis, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a string <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The string representation of the value to assign.</param>
    /// <param name="base">The base of the number in the string representation. Default is 10.</param>
    /// <exception cref="FormatException">Thrown when the string <paramref name="op"/> cannot be parsed to a <see cref="GmpFloat"/> instance.</exception>
    public unsafe void Assign(string op, int @base = 10)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(op);
            fixed (byte* opBytesPtr = opBytes)
            {
                int ret = GmpLib.__gmpf_set_str((IntPtr)pthis, (IntPtr)opBytesPtr, @base);
                if (ret != 0)
                {
                    throw new FormatException($"Failed to parse \"{op}\", base={@base} to BigFloat, __gmpf_set_str returns {ret}");
                }
            }
        }
    }

    /// <summary>
    /// Swaps the values of two <see cref="GmpFloat"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance.</param>
    /// <remarks>
    /// This method swaps the values of <paramref name="op1"/> and <paramref name="op2"/> by reference.
    /// </remarks>
    public unsafe static void Swap(GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_swap((IntPtr)pop1, (IntPtr)pop2);
        }
    }
    #endregion

    #region Conversion Functions

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to a double-precision floating-point number.
    /// </summary>
    /// <returns>A double-precision floating-point number that represents the value of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe double ToDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_d((IntPtr)ptr);
        }
    }

    public static explicit operator double(GmpFloat op) => op.ToDouble();

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to an <see cref="ExpDouble"/> instance.
    /// </summary>
    /// <returns>An <see cref="ExpDouble"/> instance representing the same value as the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            int exp;
            double val = GmpLib.__gmpf_get_d_2exp((IntPtr)ptr, (IntPtr)(&exp));
            return new ExpDouble(exp, val);
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to a 32-bit signed integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that represents the value of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe int ToInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_si((IntPtr)ptr);
        }
    }

    public static explicit operator int(GmpFloat op) => op.ToInt32();

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to an unsigned 32-bit integer.
    /// </summary>
    /// <returns>An unsigned 32-bit integer representation of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe uint ToUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_ui((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the same value as the current <see cref="GmpFloat"/> instance.</returns>
    public GmpRational ToGmpRational() => GmpRational.From(this);

    /// <summary>Explicitly converts a GmpFloat value to an unsigned 32-bit integer.</summary>
    public static explicit operator uint(GmpFloat op) => op.ToUInt32();

    /// <summary>
    /// Returns a string representation of the current <see cref="GmpFloat"/> object using default format.
    /// </summary>
    /// <returns>A string representation of the current <see cref="GmpFloat"/> object.</returns>
    public override string ToString() => ToString(format: null);

    /// <summary>
    /// Converts the value of this <see cref="GmpFloat"/> instance to its equivalent string representation using the specified base.
    /// </summary>
    /// <param name="base">The base of the number system to use for the return value. Default is 10.</param>
    /// <returns>A string representation of the value of this instance in the specified base.</returns>
    public unsafe string ToString(int @base = 10)
    {
        return Prepare(@base).SplitNumberString().Format0(Thread.CurrentThread.CurrentCulture.NumberFormat);
    }

    /// <summary>
    /// Converts a decimal number string <paramref name="s"/> with exponent <paramref name="exp"/> to a formatted string using the current culture's number format.
    /// </summary>
    /// <param name="s">The decimal number string to convert.</param>
    /// <param name="exp">The exponent of the decimal number string.</param>
    /// <returns>A formatted string representation of the decimal number.</returns>
    internal static string ToString(string s, int exp)
    {
        return new DecimalNumberString(s, exp).SplitNumberString().Format0(Thread.CurrentThread.CurrentCulture.NumberFormat);
    }

    /// <summary>
    /// Prepare a <see cref="DecimalNumberString"/> instance from the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="base">The base to use for the conversion.</param>
    /// <returns>A new instance of <see cref="DecimalNumberString"/> representing the converted value.</returns>
    /// <exception cref="ArgumentException">Thrown when unable to convert <see cref="GmpFloat"/> to string.</exception>
    private unsafe DecimalNumberString Prepare(int @base)
    {
        const nint srcptr = 0;
        const int digits = 0;
        fixed (Mpf_t* ptr = &Raw)
        {
            int exp;
            IntPtr ret = default;
            try
            {
                ret = GmpLib.__gmpf_get_str(srcptr, (IntPtr)(&exp), 10, digits, (IntPtr)ptr);
                if (ret == IntPtr.Zero)
                {
                    throw new ArgumentException($"Unable to convert BigInteger to string.");
                }

                string s = Marshal.PtrToStringAnsi(ret)!;
                return new(s, exp);
            }
            finally
            {
                if (ret != IntPtr.Zero)
                {
                    GmpMemory.Free(ret);
                }
            }
        }
    }

    /// <summary>
    /// Converts the value of the current <see cref="GmpFloat"/> object to its equivalent string representation using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A format string containing formatting specifications.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the value of the current <see cref="GmpFloat"/> object as specified by <paramref name="format"/> and <paramref name="formatProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="format"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not one of the supported format strings: "N", "F", "E", "G".</exception>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        NumberFormatInfo numberFormat = (NumberFormatInfo)(formatProvider ?? Thread.CurrentThread.CurrentCulture).GetFormat(typeof(NumberFormatInfo))!;

#pragma warning disable CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
        return format switch
        {
            null or "" => Prepare(10).SplitNumberString().Format0(numberFormat),
            { Length: > 0 } x => x switch
            {
                [char c, .. var rest] => (type: char.ToUpperInvariant(c), len: int.TryParse(rest, out int r) ? new int?(r) : null)
            } switch
            {
                ('N', var len) => Prepare(10).SplitNumberString().FormatN(len ?? 2, numberFormat),
                ('F', var len) => Prepare(10).SplitNumberString().FormatF(len ?? 2, numberFormat),
                ('E', var len) c => Prepare(10).SplitNumberString().ToExpParts().FormatE(c.type, 3, len ?? 6, numberFormat),
                ('G', var len) => this switch
                {
                    var _ when CompareAbs(this, 1e-5) < 0 || CompareAbs(this, 1e16) > 0
                        => Prepare(10).SplitNumberString().ToExpParts().FormatE('e', 2, len ?? 6, numberFormat),
                    _ => Prepare(10).SplitNumberString().FormatF(len ?? 2, numberFormat),
                },
                //('C', var len) => NumberFormatter.SplitNumberString(Prepare(10)).FormatC(len ?? 2, numberFormat),
                //('D', var rest) => ToStringBase10(format, numberFormat),
                //('P', var rest) => ToStringBase10(format, numberFormat),
                //('R', var rest) => ToStringBase10(format, numberFormat),
                //('X', var rest) => ToStringBase10(format, numberFormat),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Supported: N, F, E, G"),
        };
#pragma warning restore CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
    }

    /// <summary>
    /// Compares the absolute value of a <see cref="GmpFloat"/> instance <paramref name="op1"/> with a double-precision floating-point number <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The double-precision floating-point number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <para>Value Meaning</para>
    /// <para>Less than zero: The absolute value of <paramref name="op1"/> is less than <paramref name="op2"/>.</para>
    /// <para>Zero: The absolute value of <paramref name="op1"/> equals <paramref name="op2"/>.</para>
    /// <para>Greater than zero: The absolute value of <paramref name="op1"/> is greater than <paramref name="op2"/>.</para>
    /// </returns>
    public static int CompareAbs(GmpFloat op1, double op2)
    {
        int raw = op1.Raw.Size;
        try
        {
            op1.Raw.Size = Math.Abs(op1.Raw.Size);
            return Compare(op1, op2);
        }
        finally
        {
            op1.Raw.Size = raw;
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public GmpInteger ToGmpInteger() => GmpInteger.From(this);
    #endregion

    #region Arithmetic Functions
    #region Arithmetic Functions - Raw inplace functions

    /// <summary>
    /// Adds two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of the addition.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    /// <remarks>The result is stored in <paramref name="rop"/> and the original values of <paramref name="op1"/> and <paramref name="op2"/> are not modified.</remarks>
    public static unsafe void AddInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_add((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to add.</param>
    /// <param name="op2">The unsigned integer to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The precision of <paramref name="rop"/> is not changed.</remarks>
    public static unsafe void AddInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_add_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="op1"/> will be modified.</remarks>
    public static unsafe void SubtractInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_sub((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to subtract from.</param>
    /// <param name="op2">The unsigned integer value to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The precision of <paramref name="rop"/> will be the same as <paramref name="op1"/>.</remarks>
    public static unsafe void SubtractInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_sub_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="rop"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to subtract from and store the result in.</param>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="rop"/> or <paramref name="op2"/> has already been disposed.</exception>
    /// <exception cref="ArithmeticException">Thrown when the operation results in an arithmetic error.</exception>
    /// <remarks>
    /// This method modifies the value of <paramref name="rop"/> in place.
    /// </remarks>
    public static unsafe void SubtractInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_sub((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Multiplies two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// <seealso cref="GmpFloat"/>
    /// <seealso cref="GmpLib"/>
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of multiplication.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to multiply.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to multiply.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the input parameters is null.</exception>
    /// <remarks>The input parameters are fixed to avoid garbage collection during the operation.</remarks>
    public static unsafe void MultiplyInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_mul((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Multiplies a <see cref="GmpFloat"/> instance <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of the multiplication.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The unsigned integer value to multiply by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The multiplication is performed in-place, meaning the value of <paramref name="op1"/> is modified.</remarks>
    public static unsafe void MultiplyInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Divides <paramref name="op1"/> by <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of the division.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to divide by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="op1"/> will be modified.</remarks>
    /// <remarks>The precision of the result is determined by the precision of <paramref name="op1"/> and <paramref name="op2"/>.</remarks>
    /// <remarks>If <paramref name="op2"/> is zero, the result will be NaN (not-a-number).</remarks>
    /// <remarks>If <paramref name="op1"/> is NaN, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> is infinity and <paramref name="op2"/> is infinity, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> is infinity and <paramref name="op2"/> is zero, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is zero and <paramref name="op2"/> is infinity, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is infinity and <paramref name="op2"/> is not infinity or NaN, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is infinity, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is zero and <paramref name="op2"/> is not infinity or NaN, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is zero, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both NaN, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with opposite signs, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both zero, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with the same sign, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is not infinity or NaN, the result will be the quotient of <paramref name="op1"/> and <paramref name="op2"/>.</remarks>
    /// <remarks>If <paramref name="op1"/> is infinity and <paramref name="op2"/> is not infinity or NaN, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is infinity, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is zero and <paramref name="op2"/> is not infinity or NaN, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is zero, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both NaN, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with opposite signs, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both zero, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with the same sign, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is not infinity or NaN, the result will be the quotient of <paramref name="op1"/> and <paramref name="op2"/>.</remarks>
    /// <remarks>If <paramref name="op1"/> is infinity and <paramref name="op2"/> is not infinity or NaN, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is infinity, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is zero and <paramref name="op2"/> is not infinity or NaN, the result will be zero.</remarks>
    /// <remarks>If <paramref name="op1"/> is not infinity or NaN and <paramref name="op2"/> is zero, the result will be infinity.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both NaN, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with opposite signs, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both zero, the result will be NaN.</remarks>
    /// <remarks>If <paramref name="op1"/> and <paramref name="op2"/> are both infinity with the same sign, the result will be infinity.</remarks>
    public static unsafe void DivideInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_div((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Divides <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The unsigned integer to divide <paramref name="op1"/> by.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="op2"/> is zero.</exception>
    /// <remarks>The precision of <paramref name="rop"/> is determined by the precision of <paramref name="op1"/>.</remarks>
    public static unsafe void DivideInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_div_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Divides an unsigned integer <paramref name="op1"/> by a <see cref="GmpFloat"/> <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of the division.</param>
    /// <param name="op1">The unsigned integer to be divided.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to divide by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="rop"/> or <paramref name="op2"/> has already been disposed.</exception>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="op2"/> is zero.</exception>
    /// <remarks>
    /// This method modifies the value of <paramref name="rop"/> in place.
    /// </remarks>
    public static unsafe void DivideInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_div((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Raises <paramref name="op1"/> to the power of <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be raised to the power.</param>
    /// <param name="op2">The power to raise <paramref name="op1"/> to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="rop"/> will be modified.</remarks>
    public static unsafe void PowerInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_pow_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Negates the value of <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The value of <paramref name="op1"/> is not modified.</remarks>
    public static unsafe void NegateInplace(GmpFloat rop, GmpFloat op1)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_neg((IntPtr)prop, (IntPtr)pop1);
        }
    }

    /// <summary>
    /// Calculates the square root of the <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to calculate the square root from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op"/> is null.</exception>
    /// <remarks>The <paramref name="rop"/> and <paramref name="op"/> instances must have the same precision.</remarks>
    public static unsafe void SqrtInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Calculates the square root of an unsigned integer <paramref name="op"/> and stores the result in the <paramref name="rop"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The unsigned integer to calculate the square root.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> is null.</exception>
    /// <remarks>The precision of the result is determined by the precision of <paramref name="rop"/>.</remarks>
    public static unsafe void SqrtInplace(GmpFloat rop, uint op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        {
            GmpLib.__gmpf_sqrt_ui((IntPtr)prop, op);
        }
    }

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpFloat"/> instance in place.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to compute the absolute value.</param>
    /// <remarks>The result is stored in <paramref name="rop"/> and <paramref name="op"/> is not modified.</remarks>
    public static unsafe void AbsInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_abs((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Multiply <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The result will be stored in <paramref name="rop"/> and <paramref name="op1"/> will not be modified.</remarks>
    /// <remarks>The precision of <paramref name="rop"/> will be the same as the precision of <paramref name="op1"/>.</remarks>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="rop"/> will be modified.</remarks>
    /// <remarks>The function is unsafe because it uses pointers.</remarks>
    public static unsafe void Mul2ExpInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_2exp((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Divide a <see cref="GmpFloat"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The power of 2 to divide <paramref name="op1"/> by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op2"/> is negative.</exception>
    /// <remarks>The result is stored in <paramref name="rop"/> and <paramref name="op1"/> is not modified.</remarks>
    public static unsafe void Div2ExpInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_div_2exp((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    #endregion

    #region Arithmetic Functions - Easier functions

    /// <summary>
    /// Adds two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to add.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision of the result will be the maximum of the precisions of <paramref name="op1"/> and <paramref name="op2"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the precisions of <paramref name="op1"/> and <paramref name="op2"/> are not equal and <paramref name="precision"/> is set to 0.</exception>
    /// <remarks>The result will have a precision of <paramref name="precision"/> bits if it is set to a non-zero value, otherwise it will have a precision equal to the maximum of the precisions of <paramref name="op1"/> and <paramref name="op2"/>.</remarks>
    public static unsafe GmpFloat Add(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to a <paramref name="op1"/> <see cref="GmpFloat"/> instance and returns a new instance of <see cref="GmpFloat"/> with the result.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to add to.</param>
    /// <param name="op2">The unsigned integer value to add.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op1"/> is used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpFloat Add(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to subtract.</param>
    /// <param name="precision">The precision of the result in bits. If set to 0, the precision will be set to the default precision.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <see cref="GmpFloat"/> <paramref name="op1"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> to subtract from.</param>
    /// <param name="op2">The unsigned integer value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision will be set to <see cref="GmpFloat.DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts a <paramref name="op2"/> from an unsigned integer <paramref name="op1"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Multiplies two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision will be set to the default precision.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <remarks>The result will have a precision of <paramref name="precision"/> bits. If <paramref name="precision"/> is set to 0, the default precision will be used.</remarks>
    public static unsafe GmpFloat Multiply(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Multiplies a <see cref="GmpFloat"/> instance <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and returns the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to multiply.</param>
    /// <param name="op2">The unsigned integer to multiply by.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    public static unsafe GmpFloat Multiply(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divide two <see cref="GmpFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and return the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <param name="precision">The precision of the result in bits. If set to 0, the precision will be set to default.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and return the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The unsigned integer value to divide <paramref name="op1"/> by.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divide an unsigned integer <paramref name="op1"/> by a <see cref="GmpFloat"/> <paramref name="op2"/> and return the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer to divide.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> to divide by.</param>
    /// <param name="precision">The precision in bit of the result, default to 0 which means using the precision of <paramref name="op2"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> to the <paramref name="op2"/> integer, and returns a new instance of <see cref="GmpFloat"/> with the result.
    /// </summary>
    /// <param name="op1">The base value.</param>
    /// <param name="op2">The exponent value.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the power operation.</returns>
    public static unsafe GmpFloat Power(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        PowerInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Negates a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to negate.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision will be set to <see cref="GmpFloat.DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the negated value of <paramref name="op1"/>.</returns>
    public static unsafe GmpFloat Negate(GmpFloat op1, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        NegateInplace(rop, op1);
        return rop;
    }

    /// <summary>
    /// Calculates the square root of a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to calculate the square root of.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the square root of <paramref name="op"/>.</returns>
    public static unsafe GmpFloat Sqrt(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Computes the square root of a non-negative integer <paramref name="op"/> and returns the result as a new instance of <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op">The non-negative integer to compute the square root of.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the default precision is used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the square root of <paramref name="op"/>.</returns>
    public static unsafe GmpFloat Sqrt(uint op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to compute the absolute value of.</param>
    /// <param name="precision">The precision of the result in bits. If set to 0, the precision will default to <see cref="GmpFloat.DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the absolute value of <paramref name="op"/>.</returns>
    public static unsafe GmpFloat Abs(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AbsInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Multiply a <see cref="GmpFloat"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to multiply.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the default precision is used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    public static unsafe GmpFloat Mul2Exp(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        Mul2ExpInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divide a <see cref="GmpFloat"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to divide.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision of the result, default to 0 which means use the precision of <paramref name="op1"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="op2"/> is negative.</exception>
    /// <remarks>The result precision is determined by <paramref name="precision"/>. If it is 0, the precision of <paramref name="op1"/> is used.</remarks>
    public static unsafe GmpFloat Div2Exp(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        Div2ExpInplace(rop, op1, op2);
        return rop;
    }
    #endregion

    #region Arithmetic Functions - Operators
    public static unsafe GmpFloat operator +(GmpFloat op1, GmpFloat op2) => Add(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator +(GmpFloat op1, uint op2) => Add(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator -(GmpFloat op1, GmpFloat op2) => Subtract(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator -(GmpFloat op1, uint op2) => Subtract(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator -(uint op1, GmpFloat op2) => Subtract(op1, op2, op2.Precision);

    public static unsafe GmpFloat operator *(GmpFloat op1, GmpFloat op2) => Multiply(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator *(GmpFloat op1, uint op2) => Multiply(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator /(GmpFloat op1, GmpFloat op2) => Divide(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator /(GmpFloat op1, uint op2) => Divide(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator /(uint op1, GmpFloat op2) => Divide(op1, op2, op2.Precision);

    public static unsafe GmpFloat operator ^(GmpFloat op1, uint op2) => Power(op1, op2, op1.Precision);

    public static unsafe GmpFloat operator -(GmpFloat op1) => Negate(op1, op1.Precision);

    #endregion
    #endregion

    #region Comparison Functions

    /// <summary>
    /// Determines whether the current <see cref="GmpFloat"/> object is equal to another <see cref="GmpFloat"/> object.
    /// </summary>
    /// <param name="other">The <see cref="GmpFloat"/> to compare with the current object.</param>
    /// <returns><c>true</c> if the current <see cref="GmpFloat"/> object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
    public bool Equals([AllowNull] GmpFloat other)
    {
        return other is not null && Compare(this, other) == 0;
    }

    /// <summary>
    /// Compares the current <see cref="GmpFloat"/> instance with another object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a valid type.</exception>
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            uint ui => Compare(this, ui),
            int i => Compare(this, i),
            double d => Compare(this, d),
            GmpFloat f => Compare(this, f),
            GmpInteger z => Compare(this, z),
            _ => throw new ArgumentException("Invalid type", nameof(obj))
        };
    }

    /// <summary>
    /// Compares the current <see cref="GmpFloat"/> object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings:
    /// Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.
    /// Zero This object is equal to <paramref name="other"/>.
    /// Greater than zero This object is greater than <paramref name="other"/>. -or- <paramref name="other"/> is null.</returns>
    public int CompareTo([AllowNull] GmpFloat other)
    {
        return other is null ? 1 : Compare(this, other);
    }

    /// <summary>
    /// Compares two <see cref="GmpFloat"/> instances and returns an integer that indicates whether the first instance is less than, equal to, or greater than the second instance.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            return GmpLib.__gmpf_cmp((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="GmpFloat"/> instance is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        GmpFloat bf => Compare(this, bf) == 0,
        GmpInteger bi => Compare(this, bi) == 0,
        double d => Compare(this, d) == 0,
        int i => Compare(this, i) == 0,
        uint ui => Compare(this, ui) == 0,
        _ => false
    };

    /// <summary>
    /// Returns the hash code for this <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <returns>An integer hash code.</returns>
    public override int GetHashCode() => Raw.GetHashCode();

    /// <summary>Determines whether two <see cref="GmpFloat"/> instances are equal.</summary>
    public static bool operator ==(GmpFloat left, GmpFloat right) => Compare(left, right) == 0;

    /// <summary>Determines whether two <see cref="GmpFloat"/> instances are not equal.</summary>
    public static bool operator !=(GmpFloat left, GmpFloat right) => Compare(left, right) != 0;

    /// <summary>Determines whether one <see cref="GmpFloat"/> instance is greater than another.</summary>
    public static bool operator >(GmpFloat left, GmpFloat right) => Compare(left, right) > 0;

    /// <summary>Determines whether one <see cref="GmpFloat"/> instance is less than another.</summary>
    public static bool operator <(GmpFloat left, GmpFloat right) => Compare(left, right) < 0;

    /// <summary>Determines whether one <see cref="GmpFloat"/> instance is greater than or equal to another.</summary>
    public static bool operator >=(GmpFloat left, GmpFloat right) => Compare(left, right) >= 0;

    /// <summary>Determines whether one <see cref="GmpFloat"/> instance is less than or equal to another.</summary>
    public static bool operator <=(GmpFloat left, GmpFloat right) => Compare(left, right) <= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is equal to a double.</summary>
    public static bool operator ==(GmpFloat left, double right) => Compare(left, right) == 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is not equal to a double.</summary>
    public static bool operator !=(GmpFloat left, double right) => Compare(left, right) != 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than a double.</summary>
    public static bool operator >(GmpFloat left, double right) => Compare(left, right) > 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than a double.</summary>
    public static bool operator <(GmpFloat left, double right) => Compare(left, right) < 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than or equal to a double.</summary>
    public static bool operator >=(GmpFloat left, double right) => Compare(left, right) >= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than or equal to a double.</summary>
    public static bool operator <=(GmpFloat left, double right) => Compare(left, right) <= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is equal to an int.</summary>
    public static bool operator ==(GmpFloat left, int right) => Compare(left, right) == 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is not equal to an int.</summary>
    public static bool operator !=(GmpFloat left, int right) => Compare(left, right) != 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than an int.</summary>
    public static bool operator >(GmpFloat left, int right) => Compare(left, right) > 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than an int.</summary>
    public static bool operator <(GmpFloat left, int right) => Compare(left, right) < 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than or equal to an int.</summary>
    public static bool operator >=(GmpFloat left, int right) => Compare(left, right) >= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than or equal to an int.</summary>
    public static bool operator <=(GmpFloat left, int right) => Compare(left, right) <= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is equal to a uint.</summary>
    public static bool operator ==(GmpFloat left, uint right) => Compare(left, right) == 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is not equal to a uint.</summary>
    public static bool operator !=(GmpFloat left, uint right) => Compare(left, right) != 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than a uint.</summary>
    public static bool operator >(GmpFloat left, uint right) => Compare(left, right) > 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than a uint.</summary>
    public static bool operator <(GmpFloat left, uint right) => Compare(left, right) < 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than or equal to a uint.</summary>
    public static bool operator >=(GmpFloat left, uint right) => Compare(left, right) >= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than or equal to a uint.</summary>
    public static bool operator <=(GmpFloat left, uint right) => Compare(left, right) <= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is equal to a <see cref="GmpInteger"/>.</summary>
    public static bool operator ==(GmpFloat left, GmpInteger right) => Compare(left, right) == 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is not equal to a <see cref="GmpInteger"/>.</summary>
    public static bool operator !=(GmpFloat left, GmpInteger right) => Compare(left, right) != 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than a <see cref="GmpInteger"/>.</summary>
    public static bool operator >(GmpFloat left, GmpInteger right) => Compare(left, right) > 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than a <see cref="GmpInteger"/>.</summary>
    public static bool operator <(GmpFloat left, GmpInteger right) => Compare(left, right) < 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is greater than or equal to a <see cref="GmpInteger"/>.</summary>
    public static bool operator >=(GmpFloat left, GmpInteger right) => Compare(left, right) >= 0;

    /// <summary>Determines whether a <see cref="GmpFloat"/> instance is less than or equal to a <see cref="GmpInteger"/>.</summary>
    public static bool operator <=(GmpFloat left, GmpInteger right) => Compare(left, right) <= 0;

    /// <summary>Determines whether a double is equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator ==(double left, GmpFloat right) => right == left;

    /// <summary>Determines whether a double is not equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator !=(double left, GmpFloat right) => right != left;

    /// <summary>Determines whether a double is greater than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >(double left, GmpFloat right) => right < left;

    /// <summary>Determines whether a double is less than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <(double left, GmpFloat right) => right > left;

    /// <summary>Determines whether a double is greater than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >=(double left, GmpFloat right) => right <= left;

    /// <summary>Determines whether a double is less than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <=(double left, GmpFloat right) => right >= left;

    /// <summary>Determines whether an int is equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator ==(int left, GmpFloat right) => right == left;

    /// <summary>Determines whether an int is not equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator !=(int left, GmpFloat right) => right != left;

    /// <summary>Determines whether an int is greater than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >(int left, GmpFloat right) => right < left;

    /// <summary>Determines whether an int is less than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <(int left, GmpFloat right) => right > left;

    /// <summary>Determines whether an int is greater than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >=(int left, GmpFloat right) => right <= left;

    /// <summary>Determines whether an int is less than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <=(int left, GmpFloat right) => right >= left;

    /// <summary>Determines whether a uint is equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator ==(uint left, GmpFloat right) => right == left;

    /// <summary>Determines whether a uint is not equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator !=(uint left, GmpFloat right) => right != left;

    /// <summary>Determines whether a uint is greater than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >(uint left, GmpFloat right) => right < left;

    /// <summary>Determines whether a uint is less than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <(uint left, GmpFloat right) => right > left;

    /// <summary>Determines whether a uint is greater than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >=(uint left, GmpFloat right) => right <= left;

    /// <summary>Determines whether a uint is less than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <=(uint left, GmpFloat right) => right >= left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator ==(GmpInteger left, GmpFloat right) => right == left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is not equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator !=(GmpInteger left, GmpFloat right) => right != left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is greater than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >(GmpInteger left, GmpFloat right) => right < left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is less than a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <(GmpInteger left, GmpFloat right) => right > left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is greater than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator >=(GmpInteger left, GmpFloat right) => right <= left;

    /// <summary>Determines whether a <see cref="GmpInteger"/> is less than or equal to a <see cref="GmpFloat"/> instance.</summary>
    public static bool operator <=(GmpInteger left, GmpFloat right) => right >= left;

    /// <summary>
    /// Compares two <see cref="GmpFloat"/> and <see cref="GmpInteger"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> instance to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, GmpInteger op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            return GmpLib.__gmpf_cmp_z((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Compares a <see cref="GmpFloat"/> instance <paramref name="op1"/> with a double <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The double value to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>.
    /// Return value is less than 0 if <paramref name="op1"/> is less than <paramref name="op2"/>,
    /// 0 if <paramref name="op1"/> is equal to <paramref name="op2"/>,
    /// and greater than 0 if <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, double op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_d((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compares a <see cref="GmpFloat"/> instance with an integer value.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The integer value to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    ///     <listheader>
    ///         <term>Value</term>
    ///         <description>Meaning</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Less than zero</term>
    ///         <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term>Zero</term>
    ///         <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term>Greater than zero</term>
    ///         <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    ///     </item>
    /// </list>
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, int op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_si((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compares a <see cref="GmpFloat"/> instance with an unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The unsigned integer value to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="GmpFloat"/> instance is equal to the specified unsigned integer.
    /// </summary>
    /// <remarks>
    /// This function is mathematically ill-defined and should not be used.
    /// </remarks>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The unsigned integer to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <para>Value Meaning</para>
    /// <para>0     <paramref name="op1"/> is equal to <paramref name="op2"/>.</para>
    /// <para>-1    <paramref name="op1"/> is less than <paramref name="op2"/>.</para>
    /// <para>+1    <paramref name="op1"/> is greater than <paramref name="op2"/>.</para>
    /// </returns>
    [Obsolete("This function is mathematically ill-defined and should not be used.")]
    public static unsafe int MpfEquals(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Calculates the relative difference between two <see cref="GmpFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> value to calculate the relative difference.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> value to calculate the relative difference.</param>
    /// <remarks>
    /// The relative difference is calculated as (op1 - op2) / ((|op1| + |op2|) / 2).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="rop"/>, <paramref name="op1"/> or <paramref name="op2"/> has already been disposed.</exception>
    public static unsafe void RelDiffInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_reldiff((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Calculates the relative difference between two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the relative difference between <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpFloat RelDiff(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2);
        return rop;
    }

    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;

    #endregion

    #region Misc Functions

    /// <summary>
    /// Rounds up the <paramref name="op"/> value to the nearest integer and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to round up.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op"/> is null.</exception>
    /// <remarks>The <paramref name="rop"/> instance will be modified to store the result.</remarks>
    public static unsafe void CeilInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_ceil((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Returns the smallest integral value greater than or equal to the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to calculate the ceiling value.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision will be set to <see cref="GmpFloat.DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the ceiling value of <paramref name="op"/>.</returns>
    public static GmpFloat Ceil(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        CeilInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds down the value of <paramref name="op"/> to the nearest integer and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to round down.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op"/> is null.</exception>
    /// <remarks>
    /// The value of <paramref name="op"/> is not changed.
    /// </remarks>
    public static unsafe void FloorInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_floor((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Returns the largest <see cref="GmpFloat"/> less than or equal to the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> to floor.</param>
    /// <param name="precision">The precision in bits of the result. If set to 0, the precision will be set to <see cref="GmpFloat.DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the largest integer less than or equal to <paramref name="op"/>.</returns>
    public static GmpFloat Floor(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> <see cref="GmpFloat"/> instance to the nearest integer towards zero, and stores the result in the <paramref name="rop"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to round.</param>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="rop"/> or <paramref name="op"/> is null.</exception>
    /// <remarks>
    /// This method modifies the <paramref name="rop"/> instance in place.
    /// </remarks>
    public static unsafe void RoundInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_trunc((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Rounds the specified <paramref name="op"/> to the nearest integer, using the specified <paramref name="precision"/> if provided.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to round.</param>
    /// <param name="precision">The precision to use for the result, in bits. If not provided, the default precision will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the rounded value.</returns>
    public static GmpFloat Round(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        RoundInplace(rop, op);
        return rop;
    }

    public unsafe bool IsInteger
    {
        get
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpLib.__gmpf_integer_p((IntPtr)ptr) != 0;
            }
        }
    }

    /// <summary>
    /// Check if the value of this <see cref="GmpFloat"/> instance can be represented as a 32-bit signed integer.
    /// </summary>
    /// <returns><c>true</c> if the value can be represented as a 32-bit signed integer, <c>false</c> otherwise.</returns>
    public unsafe bool FitsInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sint_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Check if the value of this <see cref="GmpFloat"/> instance can be represented as an unsigned 32-bit integer.
    /// </summary>
    /// <returns><c>true</c> if the value can be represented as an unsigned 32-bit integer, <c>false</c> otherwise.</returns>
    public unsafe bool FitsUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_uint_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Check if the value of this <see cref="GmpFloat"/> instance can be represented as a 16-bit signed integer.
    /// </summary>
    /// <returns><c>true</c> if the value can be represented as a 16-bit signed integer, <c>false</c> otherwise.</returns>
    public unsafe bool FitsInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sshort_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Check if the current <see cref="GmpFloat"/> instance can be safely converted to an unsigned 16-bit integer.
    /// </summary>
    /// <returns><c>true</c> if the instance can be safely converted to an unsigned 16-bit integer, <c>false</c> otherwise.</returns>
    public unsafe bool FitsUInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_ushort_p((IntPtr)ptr) != 0;
        }
    }
    #endregion

    #region Dispose and Clear
    private bool _disposed;

    /// <summary>
    /// Clear the <see cref="GmpFloat"/> instance and free the memory allocated by GMP library.
    /// </summary>
    private unsafe void Clear()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_clear((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="GmpFloat"/> class.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            Clear();
            _disposed = true;
        }
    }

    ~GmpFloat()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Obsoleted Random
    /// <summary>
    /// Generates a random <see cref="GmpFloat"/> instance with the specified maximum limb count and exponent, and assigns it to the input <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to assign the generated random value to.</param>
    /// <param name="maxLimbCount">The maximum number of limbs (in base 2) to use for the generated value.</param>
    /// <param name="maxExp">The maximum exponent to use for the generated value.</param>
    /// <remarks>
    /// This method is obsolete, use <see cref="GmpRandom"/> instead.
    /// </remarks>
    /// <seealso cref="GmpRandom"/>
    /// <exception cref="ArgumentNullException"><paramref name="rop"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLimbCount"/> or <paramref name="maxExp"/> is less than or equal to 0.</exception>
    /// <exception cref="ArithmeticException">An error occurred while generating the random value.</exception>
    [Obsolete("use GmpRandom")]
    public static unsafe void Random2Inplace(GmpFloat rop, int maxLimbCount, int maxExp)
    {
        fixed (Mpf_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpf_random2((IntPtr)ptr, maxLimbCount, maxExp);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpFloat"/> with the specified <paramref name="precision"/> and generates a random number with the specified maximum limb count and maximum exponent.
    /// </summary>
    /// <param name="precision">The precision of the new <see cref="GmpFloat"/> instance.</param>
    /// <param name="maxLimbCount">The maximum limb count of the generated random number.</param>
    /// <param name="maxExp">The maximum exponent of the generated random number.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the generated random number.</returns>
    /// <remarks>This method is obsolete, use <see cref="GmpRandom"/> instead.</remarks>
    [Obsolete("use GmpRandom")]
    public static GmpFloat Random2(uint precision, int maxLimbCount, int maxExp)
    {
        GmpFloat rop = new(precision);
        Random2Inplace(rop, maxLimbCount, maxExp);
        return rop;
    }
    #endregion
}


/// <summary>
/// This struct represents a GMP floating-point number with arbitrary precision.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public record struct Mpf_t
{
    /// <summary>
    /// The number of significant bits in the number.
    /// </summary>
    public int Precision;
    /// <summary>
    /// The size of the data block in bytes.
    /// </summary>
    public int Size;
    /// <summary>
    /// The exponent of the number.
    /// </summary>
    public int Exponent;
    /// <summary>
    /// A pointer to the data block of the number.
    /// </summary>
    public IntPtr Limbs;

    /// <summary>
    /// The size of the struct in bytes.
    /// </summary>
    public static int RawSize => Marshal.SizeOf<Mpf_t>();


    private readonly unsafe Span<int> GetLimbData() => new((void*)Limbs, Precision - 1);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Size);
        c.Add(Exponent);
        foreach (int i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}

/// <summary>
/// Represents a value consisting of an exponent and a double-precision floating-point number.
/// </summary>
public record struct ExpDouble(int Exp, double Value);
