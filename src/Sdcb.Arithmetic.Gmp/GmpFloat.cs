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
    /// Gets or sets the default precision used for new <see cref="GmpFloat"/> instances, in bits.
    /// </summary>
    /// <value>The default precision in bits.</value>
    public static uint DefaultPrecision
    {
        get => (uint)GmpLib.__gmpf_get_default_prec().Value;
        set => GmpLib.__gmpf_set_default_prec(new CULong(value));
    }

    internal Mpf_t Raw;

    #region Initialization functions

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpFloat"/> class with <see cref="DefaultPrecision"/>.
    /// </summary>
    public unsafe GmpFloat()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_init((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpFloat"/> class with the specified <paramref name="raw"/> value.
    /// </summary>
    /// <param name="raw">The <see cref="Mpf_t"/> value to assign to the new instance.</param>
    public GmpFloat(Mpf_t raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpFloat"/> class with the specified precision.
    /// </summary>
    /// <param name="precision">The precision in bits. If set to 0, the <see cref="DefaultPrecision"/> is used.</param>
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
                GmpLib.__gmpf_init2((IntPtr)ptr, new CULong(precision));
            }
        }
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpFloat"/> with the specified precision, or <see cref="DefaultPrecision"/> if <paramref name="precision"/> is null.
    /// </summary>
    /// <param name="precision">The precision of the new instance, or null to use <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> with the specified precision.</returns>
    internal static GmpFloat CreateWithNullablePrecision(uint? precision) => precision switch
    {
        null => new GmpFloat(),
        { } p => new GmpFloat(p)
    };
    #endregion

    #region Combined Initialization and Assignment Functions

    /// <summary>
    /// Creates a deep copy of the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <returns>A new <see cref="GmpFloat"/> instance with the same value and precision as the current instance.</returns>
    public GmpFloat Clone()
    {
        GmpFloat rop = new(Precision);
        rop.Assign(this);
        return rop;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a integer <paramref name="val"/> with <see cref="DefaultPrecision"/>.
    /// </summary>
    /// <param name="val">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(int val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_si((IntPtr)ptr, new CLong(val));
        return new GmpFloat(raw);
    }

    /// <summary>
    /// Implicitly converts an <see cref="int"/> value to a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="val">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public static implicit operator GmpFloat(int val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a integer <paramref name="val"/> with the specified <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="val">The integer value to convert.</param>
    /// <param name="precision">The precision in bits for the resulting <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value with the specified precision.</returns>
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
        GmpLib.__gmpf_init_set_ui((IntPtr)ptr, new CULong(val));
        return new GmpFloat(raw);
    }

    /// <summary>
    /// Implicitly convert a <see cref="uint"/> value to a <see cref="GmpFloat"/> instance, precision default to <see cref="DefaultPrecision"/> in bit.
    /// </summary>
    /// <param name="val">The unsigned integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public static implicit operator GmpFloat(uint val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from an unsigned integer <paramref name="val"/> with the specified <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="val">The unsigned integer value to convert.</param>
    /// <param name="precision">The desired precision in bits.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value with the specified precision.</returns>
    public unsafe static GmpFloat From(uint val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a double-precision floating-point number <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
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

    /// <summary>
    /// Implicitly converts a <see cref="double"/> to a <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="val">The double value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public static implicit operator GmpFloat(double val) => From(val);

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a double <paramref name="val"/> with the specified <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="val">The double value to convert.</param>
    /// <param name="precision">The precision in bits for the resulting <see cref="GmpFloat"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value with the specified precision.</returns>
    public unsafe static GmpFloat From(double val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a <see cref="GmpInteger"/> <paramref name="val"/> with the specified <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> value to convert.</param>
    /// <param name="precision">The precision of the resulting <see cref="GmpFloat"/> in bits.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value with the specified precision.</returns>
    public unsafe static GmpFloat From(GmpInteger val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Create a <see cref="GmpFloat"/> instance from a <see cref="GmpInteger"/> <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> value to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public unsafe static GmpFloat From(GmpInteger val)
    {
        GmpFloat f = new(precision: (uint)Math.Abs(val.Raw.Size) * GmpLib.LimbBitSize);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Implicitly converts a <see cref="GmpInteger"/> to a <see cref="GmpFloat"/> with the same value.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public static implicit operator GmpFloat(GmpInteger val) => From(val);

    /// <summary>
    /// Parse a string representation of a floating-point number in the specified base and create a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="val">The string representation of the floating-point number.</param>
    /// <param name="base">The base of the number system, default is 10 (decimal).</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the parsed value.</returns>
    /// <exception cref="FormatException">Thrown when the string cannot be parsed to a floating-point number in the specified base.</exception>
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
                throw new FormatException($"Failed to parse {val}, base={@base} to {nameof(GmpFloat)}, {nameof(GmpLib.__gmpf_init_set_str)} returns {ret}");
            }
        }
        return new GmpFloat(raw);
    }

    /// <summary>
    /// Parse a string representation of a number into a <see cref="GmpFloat"/> with specified precision and base.
    /// </summary>
    /// <param name="val">The string representation of the number to parse.</param>
    /// <param name="precision">The desired precision in bits for the resulting <see cref="GmpFloat"/>.</param>
    /// <param name="base">The base of the number to parse. Default is 10 (decimal).</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the parsed value.</returns>
    public static GmpFloat Parse(string val, uint precision, int @base = 10)
    {
        GmpFloat f = new(precision);
        f.Assign(val, @base);
        return f;
    }

    /// <summary>
    /// Tries to parse a string representation of a floating-point number into a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="val">The string representation of the floating-point number to parse.</param>
    /// <param name="result">When this method returns, contains the <see cref="GmpFloat"/> equivalent to the floating-point number contained in <paramref name="val"/>, if the conversion succeeded, or null if the conversion failed.</param>
    /// <param name="base">The base of the number system to use for parsing, default is 10.</param>
    /// <returns>true if the parsing succeeded; false otherwise.</returns>
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
    /// Tries to parse a <see cref="GmpFloat"/> from a string <paramref name="val"/> with specified <paramref name="precision"/> and <paramref name="base"/>.
    /// </summary>
    /// <param name="val">The string value to parse.</param>
    /// <param name="result">The resulting <see cref="GmpFloat"/> if the parsing is successful, otherwise null.</param>
    /// <param name="precision">The precision of the resulting <see cref="GmpFloat"/> in bits.</param>
    /// <param name="base">The base of the number system used in the input string, default to 10.</param>
    /// <returns>True if the parsing is successful, otherwise false.</returns>
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

    /// <summary>
    /// Gets or sets the precision of the <see cref="GmpFloat"/> instance in bits.
    /// </summary>
    /// <value>
    /// The precision in bits.
    /// </value>
    public unsafe uint Precision
    {
        get
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return (uint)GmpLib.__gmpf_get_prec((IntPtr)ptr).Value;
            }
        }
        set
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpLib.__gmpf_set_prec((IntPtr)ptr, new CULong(value));
            }
        }
    }

    /// <summary>
    /// Sets the raw precision of the current <see cref="GmpFloat"/> instance. This method is obsolete, use <see cref="Precision"/> property instead.
    /// </summary>
    /// <param name="value">The new raw precision value.</param>
    /// <remarks>
    /// This method is marked as obsolete and may be removed in future versions.
    /// </remarks>
    [Obsolete("use Precision")]
    public unsafe void SetRawPrecision(uint value)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_set_prec_raw((IntPtr)ptr, new CULong(value));
        }
    }
    #endregion

    #region Assignment functions

    /// <summary>
    /// Assigns the value of the specified <see cref="GmpFloat"/> <paramref name="op"/> to the current instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to assign its value from.</param>
    public unsafe void Assign(GmpFloat op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpf_t* pthat = &op.Raw)
        {
            GmpLib.__gmpf_set((IntPtr)pthis, (IntPtr)pthat);
        }
    }

    /// <summary>
    /// Assigns the value of an unsigned integer <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The unsigned integer value to assign.</param>
    public unsafe void Assign(uint op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_ui((IntPtr)pthis, new CULong(op));
        }
    }

    /// <summary>
    /// Assigns the <paramref name="op"/> integer value to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The integer value to assign.</param>
    public unsafe void Assign(int op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_si((IntPtr)pthis, new CLong(op));
        }
    }

    /// <summary>
    /// Assigns the value of a double <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The double value to assign.</param>
    public unsafe void Assign(double op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_d((IntPtr)pthis, op);
        }
    }

    /// <summary>
    /// Assigns the value of the specified <see cref="GmpInteger"/> <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> value to assign.</param>
    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_z((IntPtr)pthis, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpRational"/> <paramref name="op"/> to the current <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> value to assign.</param>
    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_q((IntPtr)pthis, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a string <paramref name="op"/> to the current <see cref="GmpFloat"/> instance, with an optional number <paramref name="base"/>.
    /// </summary>
    /// <param name="op">The string representation of the value to assign.</param>
    /// <param name="base">The optional number base of the value to assign, default is 10.</param>
    /// <exception cref="FormatException">Thrown when the string cannot be parsed to a <see cref="GmpFloat"/>.</exception>
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
    /// Swaps the values of two <see cref="GmpFloat"/> instances, <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance.</param>
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
    /// Converts the current <see cref="GmpFloat"/> instance to a <see cref="double"/> value.
    /// </summary>
    /// <returns>A <see cref="double"/> representation of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe double ToDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_d((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Converts the value of the specified <see cref="GmpFloat"/> to its equivalent double-precision floating-point number.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> to be converted.</param>
    /// <returns>A double-precision floating-point number equivalent to the value of <paramref name="op"/>.</returns>
    public static explicit operator double(GmpFloat op) => op.ToDouble();

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to an <see cref="ExpDouble"/> representation.
    /// </summary>
    /// <returns>An <see cref="ExpDouble"/> instance representing the current <see cref="GmpFloat"/> value.</returns>
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
    /// <returns>A 32-bit signed integer representation of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe int ToInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return (int)GmpLib.__gmpf_get_si((IntPtr)ptr).Value;
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="GmpFloat"/> instance to an <see cref="int"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <returns>An <see cref="int"/> value that represents the converted value of the <see cref="GmpFloat"/> instance.</returns>
    public static explicit operator int(GmpFloat op) => op.ToInt32();

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to an unsigned 32-bit integer.
    /// </summary>
    /// <returns>An unsigned 32-bit integer representing the value of the current <see cref="GmpFloat"/> instance.</returns>
    public unsafe uint ToUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return (uint)GmpLib.__gmpf_get_ui((IntPtr)ptr).Value;
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpFloat"/> instance to a <see cref="GmpRational"/> representation.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public GmpRational ToGmpRational() => GmpRational.From(this);

    /// <summary>
    /// Explicitly converts a <see cref="GmpFloat"/> to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <returns>An unsigned 32-bit integer representing the converted value.</returns>
    public static explicit operator uint(GmpFloat op) => op.ToUInt32();

    /// <summary>
    /// Returns a string representation of the current <see cref="GmpFloat"/> value using the default format.
    /// </summary>
    /// <returns>A string representation of the current <see cref="GmpFloat"/> value.</returns>
    public override string ToString() => ToString(format: null);

    /// <summary>
    /// Converts the <see cref="GmpFloat"/> value to its string representation in the specified base.
    /// </summary>
    /// <param name="base">The base of the number system to use for the string representation (default is 10).</param>
    /// <returns>A string representation of the <see cref="GmpFloat"/> value in the specified base.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the base is less than 2 or greater than 62.</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the base is less than 2 or greater than 62.</exception>
    private unsafe DecimalNumberString Prepare(int @base)
    {
        if (@base < 2 || @base > 62) throw new ArgumentOutOfRangeException(nameof(@base), $"@base parameter must in range [2, 62].");

        const nint srcptr = 0;
        const int digits = 0;
        fixed (Mpf_t* ptr = &Raw)
        {
            int exp;
            IntPtr ret = default;
            try
            {
                ret = GmpLib.__gmpf_get_str(srcptr, (IntPtr)(&exp), @base, digits, (IntPtr)ptr);
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
    /// Converts the numeric value of this instance to its equivalent string representation using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">Allowed values: N, F, E, G</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the value of this instance as specified by <paramref name="format"/> and <paramref name="formatProvider"/>.</returns>
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
    /// Compares the absolute values of two numbers, a <see cref="GmpFloat"/> <paramref name="op1"/> and a <see cref="double"/> <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first operand of type <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The second operand of type <see cref="double"/>.</param>
    /// <returns>A signed integer that indicates the relative values of the absolute values of <paramref name="op1"/> and <paramref name="op2"/>, &lt;0 if <paramref name="op1"/> is less than <paramref name="op2"/>, 0 if they are equal, or >0 if <paramref name="op1"/> is greater than <paramref name="op2"/>.</returns>
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
    /// Converts the current <see cref="GmpFloat"/> instance to a <see cref="GmpInteger"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public GmpInteger ToGmpInteger() => GmpInteger.From(this);
    #endregion

    #region Arithmetic Functions
    #region Arithmetic Functions - Raw inplace functions

    /// <summary>
    /// Add two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to add.</param>
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
    /// Adds an unsigned integer <paramref name="op2"/> to a <see cref="GmpFloat"/> <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be added.</param>
    /// <param name="op2">The unsigned integer to be added.</param>
    public static unsafe void AddInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_add_ui((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The minuend <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The subtrahend <see cref="GmpFloat"/> instance.</param>
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
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <see cref="GmpFloat"/> <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The minuend <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The subtrahend unsigned integer value.</param>
    public static unsafe void SubtractInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_sub_ui((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Subtracts a <paramref name="op2"/> from an unsigned integer <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to subtract.</param>
    public static unsafe void SubtractInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_sub((IntPtr)prop, new CULong(op1), (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Multiplies two <see cref="GmpFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The result operand where the multiplication result will be stored.</param>
    /// <param name="op1">The first operand to be multiplied.</param>
    /// <param name="op2">The second operand to be multiplied.</param>
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
    /// Multiply <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The unsigned integer operand.</param>
    public static unsafe void MultiplyInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_ui((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Divides the values of <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The dividend <see cref="GmpFloat"/> instance.</param>
    /// <param name="op2">The divisor <see cref="GmpFloat"/> instance.</param>
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
    /// Divides a <see cref="GmpFloat"/> <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The unsigned integer to divide by.</param>
    public static unsafe void DivideInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_div_ui((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Divides an unsigned integer <paramref name="op1"/> by a <see cref="GmpFloat"/> <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result of the division.</param>
    /// <param name="op1">The unsigned integer to divide.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> instance to divide by.</param>
    public static unsafe void DivideInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_div((IntPtr)prop, new CULong(op1), (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Computes the power of a <see cref="GmpFloat"/> <paramref name="op1"/> raised to an unsigned integer <paramref name="op2"/>, and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> to store the result.</param>
    /// <param name="op1">The base <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The exponent as an unsigned integer.</param>
    public static unsafe void PowerInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_pow_ui((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Negate the value of <paramref name="op1"/> and store the result in <paramref name="rop"/> inplace.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the negated result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be negated.</param>
    public static unsafe void NegateInplace(GmpFloat rop, GmpFloat op1)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_neg((IntPtr)prop, (IntPtr)pop1);
        }
    }

    /// <summary>
    /// Computes the square root of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance whose square root will be computed.</param>
    public static unsafe void SqrtInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Computes the square root of an unsigned integer <paramref name="op"/> and stores the result in the existing <see cref="GmpFloat"/> instance <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op">The unsigned integer value to compute the square root of.</param>
    public static unsafe void SqrtInplace(GmpFloat rop, uint op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        {
            GmpLib.__gmpf_sqrt_ui((IntPtr)prop, new CULong(op));
        }
    }

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpFloat"/> in-place.
    /// </summary>
    /// <param name="rop">The result of the absolute value operation.</param>
    /// <param name="op">The input <see cref="GmpFloat"/> to compute the absolute value of.</param>
    public static unsafe void AbsInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_abs((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Multiplies the <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The exponent to which 2 will be raised.</param>
    public static unsafe void Mul2ExpInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_2exp((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to be divided.</param>
    /// <param name="op2">The exponent to which 2 is raised.</param>
    public static unsafe void Div2ExpInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_div_2exp((IntPtr)prop, (IntPtr)pop1, new CULong(op2));
        }
    }

    #endregion

    #region Arithmetic Functions - Easier functions

    /// <summary>
    /// Adds two <see cref="GmpFloat"/> instances, <paramref name="op1"/> and <paramref name="op2"/>, and returns the result with the specified <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="op1">The first operand in the addition operation.</param>
    /// <param name="op2">The second operand in the addition operation.</param>
    /// <param name="precision">The desired precision of the result in bits. Default is 0, which uses the <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpFloat Add(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Adds a <see cref="GmpFloat"/> and an unsigned integer, returning a new <see cref="GmpFloat"/> with the specified precision.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The unsigned integer operand to add.</param>
    /// <param name="precision">The desired precision of the result in bits. Default is 0, which uses the precision of the first operand.</param>
    /// <returns>A new <see cref="GmpFloat"/> representing the sum of the two operands with the specified precision.</returns>
    public static unsafe GmpFloat Add(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts two <see cref="GmpFloat"/> instances and returns the result with the specified precision.
    /// </summary>
    /// <param name="op1">The minuend.</param>
    /// <param name="op2">The subtrahend.</param>
    /// <param name="precision">The precision of the result in bits. If 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <see cref="GmpFloat"/> <paramref name="op1"/> and returns the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> to subtract from.</param>
    /// <param name="op2">The unsigned integer to subtract.</param>
    /// <param name="precision">The precision of the result in bits. If 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts a <paramref name="op1"/> and a <see cref="GmpFloat"/> <paramref name="op2"/> with an optional <paramref name="precision"/> in bits.
    /// </summary>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> value to subtract.</param>
    /// <param name="precision">The optional precision in bits for the result. Default is 0, which uses the <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat Subtract(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Multiplies two <see cref="GmpFloat"/> instances and returns the result.
    /// </summary>
    /// <param name="op1">The first operand of the multiplication.</param>
    /// <param name="op2">The second operand of the multiplication.</param>
    /// <param name="precision">The precision of the result in bits, default to 0 which means use <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the multiplication.</returns>
    public static unsafe GmpFloat Multiply(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Multiplies a <see cref="GmpFloat"/> <paramref name="op1"/> with an unsigned integer <paramref name="op2"/> and returns the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> operand to multiply.</param>
    /// <param name="op2">The unsigned integer operand to multiply.</param>
    /// <param name="precision">The optional precision of the result in bits. If not specified or set to 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    public static unsafe GmpFloat Multiply(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divides two <see cref="GmpFloat"/> instances and returns the result as a new <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The dividend <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The divisor <see cref="GmpFloat"/>.</param>
    /// <param name="precision">The precision of the result in bits. If not specified, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divides a <see cref="GmpFloat"/> by an unsigned integer and returns the result with the specified precision.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> dividend.</param>
    /// <param name="op2">The uint divisor.</param>
    /// <param name="precision">The desired precision in bits. Default is 0, which uses the precision of <paramref name="op1"/>.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divides an unsigned integer <paramref name="op1"/> by a <see cref="GmpFloat"/> <paramref name="op2"/> and returns the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op1">The unsigned integer dividend.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> divisor.</param>
    /// <param name="precision">The optional precision of the result in bits. If not specified, the <see cref="DefaultPrecision"/> is used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the division.</returns>
    public static unsafe GmpFloat Divide(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Calculates the power of a <see cref="GmpFloat"/> number raised to an unsigned integer exponent.
    /// </summary>
    /// <param name="op1">The base <see cref="GmpFloat"/> number.</param>
    /// <param name="op2">The unsigned integer exponent.</param>
    /// <param name="precision">The precision of the result in bits. If not specified, <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the power operation.</returns>
    public static unsafe GmpFloat Power(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        PowerInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Negate the value of a <see cref="GmpFloat"/> instance <paramref name="op1"/> and create a new instance with the result.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to negate.</param>
    /// <param name="precision">Optional precision for the result; if not provided, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the negated value.</returns>
    public static unsafe GmpFloat Negate(GmpFloat op1, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        NegateInplace(rop, op1);
        return rop;
    }

    /// <summary>
    /// Calculates the square root of a <see cref="GmpFloat"/> instance <paramref name="op"/> with the specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to calculate the square root of.</param>
    /// <param name="precision">The precision of the result in bits, default is 0 which uses the <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the square root of the input value.</returns>
    public static unsafe GmpFloat Sqrt(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Calculate the square root of a given unsigned integer <paramref name="op"/> with specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The unsigned integer value to calculate the square root of.</param>
    /// <param name="precision">The desired precision of the result, in bits. Default is 0, which uses the <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the square root of the input value.</returns>
    public static unsafe GmpFloat Sqrt(uint op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Calculate the absolute value of a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The input <see cref="GmpFloat"/> value.</param>
    /// <param name="precision">The desired precision of the result in bits. If set to 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the absolute value of the input value.</returns>
    public static unsafe GmpFloat Abs(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AbsInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Multiplies the given <see cref="GmpFloat"/> <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and returns the result.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> value to multiply.</param>
    /// <param name="op2">The exponent to which 2 is raised.</param>
    /// <param name="precision">The precision of the result in bits. If 0, <see cref="DefaultPrecision"/> is used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the multiplication.</returns>
    public static unsafe GmpFloat Mul2Exp(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        Mul2ExpInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divide a <see cref="GmpFloat"/> <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> with an optional specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="op1">The dividend <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The exponent to raise 2 to.</param>
    /// <param name="precision">The optional precision in bits, default to 0 which means using <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat Div2Exp(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        Div2ExpInplace(rop, op1, op2);
        return rop;
    }
    #endregion

    #region Arithmetic Functions - Operators

    /// <summary>
    /// Adds two <see cref="GmpFloat"/> instances and returns the result with the precision of <paramref name="op1"/>.
    /// </summary>
    /// <param name="op1">The first operand to add.</param>
    /// <param name="op2">The second operand to add.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the sum of the two operands.</returns>
    public static unsafe GmpFloat operator +(GmpFloat op1, GmpFloat op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds a <see cref="GmpFloat"/> and a <see cref="uint"/> together, returning a new <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The first operand of type <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The second operand of type <see cref="uint"/>.</param>
    /// <returns>A new <see cref="GmpFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpFloat operator +(GmpFloat op1, uint op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts two <see cref="GmpFloat"/> instances and returns the result.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the subtraction.</returns>
    public static unsafe GmpFloat operator -(GmpFloat op1, GmpFloat op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <see cref="GmpFloat"/> <paramref name="op1"/> with specified precision.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The unsigned integer operand.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the subtraction.</returns>
    public static unsafe GmpFloat operator -(GmpFloat op1, uint op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts a <see cref="GmpFloat"/> from an unsigned integer, with the precision of the result set to the precision of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer to subtract from.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> to subtract.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the result of the subtraction.</returns>
    public static unsafe GmpFloat operator -(uint op1, GmpFloat op2) => Subtract(op1, op2, op2.Precision);

    /// <summary>
    /// Multiply two <see cref="GmpFloat"/> instances with the specified precision.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    public static unsafe GmpFloat operator *(GmpFloat op1, GmpFloat op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiply a <see cref="GmpFloat"/> with an unsigned integer.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The unsigned integer operand.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the multiplication.</returns>
    public static unsafe GmpFloat operator *(GmpFloat op1, uint op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Divides two <see cref="GmpFloat"/> instances and returns the result as a new <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="op1">The dividend <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The divisor <see cref="GmpFloat"/>.</param>
    /// <returns>A new <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat operator /(GmpFloat op1, GmpFloat op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divides a <see cref="GmpFloat"/> <paramref name="op1"/> by an unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The dividend of type <see cref="GmpFloat"/>.</param>
    /// <param name="op2">The divisor of type <see cref="uint"/>.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat operator /(GmpFloat op1, uint op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divides an unsigned integer <paramref name="op1"/> by a <see cref="GmpFloat"/> <paramref name="op2"/> with the specified precision.
    /// </summary>
    /// <param name="op1">The unsigned integer dividend.</param>
    /// <param name="op2">The <see cref="GmpFloat"/> divisor.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the result of the division.</returns>
    public static unsafe GmpFloat operator /(uint op1, GmpFloat op2) => Divide(op1, op2, op2.Precision);

    /// <summary>
    /// Raises a <see cref="GmpFloat"/> value to the power of a specified unsigned integer value.
    /// </summary>
    /// <param name="op1">The base <see cref="GmpFloat"/> value.</param>
    /// <param name="op2">The exponent as an unsigned integer value.</param>
    /// <returns>The result of raising <paramref name="op1"/> to the power of <paramref name="op2"/>, as a new <see cref="GmpFloat"/> instance.</returns>
    public static unsafe GmpFloat operator ^(GmpFloat op1, uint op2) => Power(op1, op2, op1.Precision);

    /// <summary>
    /// Negates the given <paramref name="op1"/> <see cref="GmpFloat"/> instance, preserving its precision.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to negate.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the negation of <paramref name="op1"/>.</returns>
    public static unsafe GmpFloat operator -(GmpFloat op1) => Negate(op1, op1.Precision);

    #endregion
    #endregion

    #region Comparison Functions
    /// <summary>
    /// Determines whether the current <see cref="GmpFloat"/> instance is equal to the specified <paramref name="other"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="GmpFloat"/> instance to compare with the current instance.</param>
    /// <returns><c>true</c> if the current instance is equal to the specified instance; otherwise, <c>false</c>.</returns>
    public bool Equals([AllowNull] GmpFloat other)
    {
        return other is not null && Compare(this, other) == 0;
    }

    /// <summary>
    /// Compares the current <see cref="GmpFloat"/> instance to the specified <paramref name="obj"/> and returns an integer that indicates their relative order.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="GmpFloat"/> instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings:
    /// Less than zero: This instance precedes <paramref name="obj"/> in the sort order.
    /// Zero: This instance occurs in the same position in the sort order as <paramref name="obj"/>.
    /// Greater than zero: This instance follows <paramref name="obj"/> in the sort order.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a valid type for comparison.</exception>
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
    /// Compares the current <see cref="GmpFloat"/> instance to the specified <paramref name="other"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="GmpFloat"/> instance to compare with the current instance.</param>
    /// <returns>A signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Less than zero: This instance is less than <paramref name="other"/>. Zero: This instance is equal to <paramref name="other"/>. Greater than zero: This instance is greater than <paramref name="other"/>.</returns>
    public int CompareTo([AllowNull] GmpFloat other)
    {
        return other is null ? 1 : Compare(this, other);
    }

    /// <summary>
    /// Compares two <see cref="GmpFloat"/> instances and returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative order of <paramref name="op1"/> and <paramref name="op2"/>:
    /// Less than zero if <paramref name="op1"/> is less than <paramref name="op2"/>,
    /// Zero if <paramref name="op1"/> is equal to <paramref name="op2"/>,
    /// Greater than zero if <paramref name="op1"/> is greater than <paramref name="op2"/>.</returns>
    public static unsafe int Compare(GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            return GmpLib.__gmpf_cmp((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="GmpFloat"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="GmpFloat"/>; otherwise, false.</returns>
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
    /// <returns>An integer hash code for this instance.</returns>
    public override int GetHashCode() => Raw.GetHashCode();

    /// <summary>
    /// Determines whether two <see cref="GmpFloat"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns>True if the two instances are equal, otherwise false.</returns>
    public static bool operator ==(GmpFloat left, GmpFloat right) => Compare(left, right) == 0;

    /// <summary>
    /// Determines whether two <see cref="GmpFloat"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="right">The second <see cref="GmpFloat"/> instance to compare.</param>
    /// <returns>True if the two instances are not equal; otherwise, false.</returns>
    public static bool operator !=(GmpFloat left, GmpFloat right) => Compare(left, right) != 0;

    /// <summary>
    /// Determines if the <paramref name="left"/> <see cref="GmpFloat"/> is greater than the <paramref name="right"/> <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>True if the <paramref name="left"/> <see cref="GmpFloat"/> is greater than the <paramref name="right"/> <see cref="GmpFloat"/>, otherwise false.</returns>
    public static bool operator >(GmpFloat left, GmpFloat right) => Compare(left, right) > 0;

    /// <summary>
    /// Determines if the value of a <see cref="GmpFloat"/> instance is less than the value of another instance.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>True if the value of <paramref name="left"/> is less than the value of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <(GmpFloat left, GmpFloat right) => Compare(left, right) < 0;

    /// <summary>
    /// Determines if the value of the <paramref name="left"/> <see cref="GmpFloat"/> is greater than or equal to the value of the <paramref name="right"/> <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>True if the value of the <paramref name="left"/> <see cref="GmpFloat"/> is greater than or equal to the value of the <paramref name="right"/> <see cref="GmpFloat"/>, otherwise false.</returns>
    public static bool operator >=(GmpFloat left, GmpFloat right) => Compare(left, right) >= 0;

    /// <summary>
    /// Determines whether a specified <see cref="GmpFloat"/> is less than or equal to another specified <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The first <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns>true if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(GmpFloat left, GmpFloat right) => Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> instance is equal to a <see cref="double"/> value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance.</param>
    /// <param name="right">The <see cref="double"/> value.</param>
    /// <returns>true if the two values are equal; otherwise, false.</returns>
    public static bool operator ==(GmpFloat left, double right) => Compare(left, right) == 0;

    /// <summary>
    /// Determines whether two <see cref="GmpFloat"/> and <see cref="double"/> values are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The second <see cref="double"/> value to compare.</param>
    /// <returns>true if the values are not equal; otherwise, false.</returns>
    public static bool operator !=(GmpFloat left, double right) => Compare(left, right) != 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> value is greater than a <see cref="double"/> value.
    /// </summary>
    /// <param name="left">The left <see cref="GmpFloat"/> value.</param>
    /// <param name="right">The right <see cref="double"/> value.</param>
    /// <returns>True if the left <see cref="GmpFloat"/> value is greater than the right <see cref="double"/> value, otherwise false.</returns>
    public static bool operator >(GmpFloat left, double right) => Compare(left, right) > 0;

    /// <summary>
    /// Determines whether the specified <see cref="GmpFloat"/> value is less than the specified double value.
    /// </summary>
    /// <param name="left">The first <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The double value to compare with the first <see cref="GmpFloat"/> value.</param>
    /// <returns>true if the first <see cref="GmpFloat"/> value is less than the double value; otherwise, false.</returns>
    public static bool operator <(GmpFloat left, double right) => Compare(left, right) < 0;

    /// <summary>
    /// Determines whether the <see cref="GmpFloat"/> value <paramref name="left"/> is greater than or equal to the <see cref="double"/> value <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The left <see cref="GmpFloat"/> operand.</param>
    /// <param name="right">The right <see cref="double"/> operand.</param>
    /// <returns>True if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, False.</returns>
    public static bool operator >=(GmpFloat left, double right) => Compare(left, right) >= 0;

    /// <summary>
    /// Determines whether a specified <see cref="GmpFloat"/> instance is less than or equal to a specified double value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="right">The double value to compare.</param>
    /// <returns>true if the value of <paramref name="left"/> is less than or equal to the value of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(GmpFloat left, double right) => Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> instance is equal to an integer.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance.</param>
    /// <param name="right">The integer value to compare.</param>
    /// <returns>true if the <see cref="GmpFloat"/> instance is equal to the integer; otherwise, false.</returns>
    public static bool operator ==(GmpFloat left, int right) => Compare(left, right) == 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> value is not equal to an integer value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The integer value to compare.</param>
    /// <returns>True if the two values are not equal, otherwise false.</returns>
    public static bool operator !=(GmpFloat left, int right) => Compare(left, right) != 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> instance is greater than an integer value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="right">The integer value to compare.</param>
    /// <returns>True if the <see cref="GmpFloat"/> instance is greater than the integer value, otherwise false.</returns>
    public static bool operator >(GmpFloat left, int right) => Compare(left, right) > 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> value is less than an integer value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The integer value to compare.</param>
    /// <returns>True if the <see cref="GmpFloat"/> value is less than the integer value, otherwise False.</returns>
    public static bool operator <(GmpFloat left, int right) => Compare(left, right) < 0;

    /// <summary>
    /// Determines if the value of a <see cref="GmpFloat"/> is greater than or equal to an integer value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="right">The integer value to compare.</param>
    /// <returns>True if the <see cref="GmpFloat"/> value is greater than or equal to the integer value, otherwise false.</returns>
    public static bool operator >=(GmpFloat left, int right) => Compare(left, right) >= 0;

    /// <summary>
    /// Determines whether a specified <see cref="GmpFloat"/> is less than or equal to a specified integer.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The integer to compare.</param>
    /// <returns>true if the value of <paramref name="left"/> is less than or equal to the value of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(GmpFloat left, int right) => Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> instance is equal to a <see cref="uint"/> value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="right">The <see cref="uint"/> value to compare.</param>
    /// <returns><c>true</c> if the <see cref="GmpFloat"/> and <see cref="uint"/> values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(GmpFloat left, uint right) => Compare(left, right) == 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> and a <see cref="uint"/> are not equal.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The <see cref="uint"/> to compare.</param>
    /// <returns>True if the two values are not equal; otherwise, false.</returns>
    public static bool operator !=(GmpFloat left, uint right) => Compare(left, right) != 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> value is greater than an unsigned integer value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The unsigned integer value to compare.</param>
    /// <returns>True if the <see cref="GmpFloat"/> value is greater than the unsigned integer value, false otherwise.</returns>
    public static bool operator >(GmpFloat left, uint right) => Compare(left, right) > 0;

    /// <summary>
    /// Determines whether the <see cref="GmpFloat"/> value of <paramref name="left"/> is less than the <see cref="uint"/> value of <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The left <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="right">The right <see cref="uint"/> value to compare.</param>
    /// <returns>True if the value of <paramref name="left"/> is less than the value of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <(GmpFloat left, uint right) => Compare(left, right) < 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> value is greater than or equal to a <see cref="uint"/> value.
    /// </summary>
    /// <param name="left">The left <see cref="GmpFloat"/> operand.</param>
    /// <param name="right">The right <see cref="uint"/> operand.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> is greater than or equal to the <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpFloat left, uint right) => Compare(left, right) >= 0;

    /// <summary>
    /// Determines if the value of a <see cref="GmpFloat"/> is less than or equal to a uint.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The uint to compare.</param>
    /// <returns>true if the value of <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(GmpFloat left, uint right) => Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether two specified instances of <see cref="GmpFloat"/> and <see cref="GmpInteger"/> are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>true if the values of <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
    public static bool operator ==(GmpFloat left, GmpInteger right) => Compare(left, right) == 0;

    /// <summary>
    /// Determines whether two instances of <see cref="GmpFloat"/> and <see cref="GmpInteger"/> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="GmpFloat"/>.</param>
    /// <param name="right">The second instance of <see cref="GmpInteger"/>.</param>
    /// <returns>true if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(GmpFloat left, GmpInteger right) => Compare(left, right) != 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> is greater than a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The <see cref="GmpInteger"/> to compare.</param>
    /// <returns>True if the <paramref name="left"/> is greater than <paramref name="right"/>, otherwise false.</returns>
    public static bool operator >(GmpFloat left, GmpInteger right) => Compare(left, right) > 0;

    /// <summary>
    /// Determines if a <see cref="GmpFloat"/> is less than a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The <see cref="GmpInteger"/> to compare.</param>
    /// <returns>True if <paramref name="left"/> is less than <paramref name="right"/>, otherwise false.</returns>
    public static bool operator <(GmpFloat left, GmpInteger right) => Compare(left, right) < 0;

    /// <summary>
    /// Determines if the value of a <see cref="GmpFloat"/> is greater than or equal to the value of a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> to compare.</param>
    /// <param name="right">The <see cref="GmpInteger"/> to compare.</param>
    /// <returns>True if the value of <paramref name="left"/> is greater than or equal to the value of <paramref name="right"/>, otherwise false.</returns>
    public static bool operator >=(GmpFloat left, GmpInteger right) => Compare(left, right) >= 0;

    /// <summary>
    /// Determines if the <see cref="GmpFloat"/> value is less than or equal to the <see cref="GmpInteger"/> value.
    /// </summary>
    /// <param name="left">The <see cref="GmpFloat"/> value.</param>
    /// <param name="right">The <see cref="GmpInteger"/> value.</param>
    /// <returns>True if the <see cref="GmpFloat"/> value is less than or equal to the <see cref="GmpInteger"/> value, otherwise false.</returns>
    public static bool operator <=(GmpFloat left, GmpInteger right) => Compare(left, right) <= 0;

    /// <summary>
    /// Determines whether a <see cref="double"/> value is equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="double"/> value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(double left, GmpFloat right) => right == left;

    /// <summary>
    /// Determines whether a <see cref="double"/> and a <see cref="GmpFloat"/> are not equal.
    /// </summary>
    /// <param name="left">The double value to compare.</param>
    /// <param name="right">The GmpFloat value to compare.</param>
    /// <returns>true if the specified values are not equal; otherwise, false.</returns>
    public static bool operator !=(double left, GmpFloat right) => right != left;

    /// <summary>
    /// Determines whether a <see cref="double"/> value is greater than a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="double"/> value.</param>
    /// <param name="right">The right-hand side <see cref="GmpFloat"/> value.</param>
    /// <returns>true if the <paramref name="left"/> is greater than the <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >(double left, GmpFloat right) => right < left;

    /// <summary>
    /// Determines if a <see cref="double"/> value is less than a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The double value to compare.</param>
    /// <param name="right">The GmpFloat value to compare.</param>
    /// <returns>True if the double value is less than the GmpFloat value, otherwise false.</returns>
    public static bool operator <(double left, GmpFloat right) => right > left;

    /// <summary>
    /// Determines if a <see cref="double"/> value is greater than or equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="double"/> value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>true if the <see cref="double"/> value is greater than or equal to the <see cref="GmpFloat"/> value; otherwise, false.</returns>
    public static bool operator >=(double left, GmpFloat right) => right <= left;

    /// <summary>
    /// Determines whether a <see cref="double"/> value is less than or equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="double"/> value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>True if the <see cref="double"/> value is less than or equal to the <see cref="GmpFloat"/> value, otherwise false.</returns>
    public static bool operator <=(double left, GmpFloat right) => right >= left;

    /// <summary>
    /// Determines whether an integer <paramref name="left"/> is equal to a <see cref="GmpFloat"/> <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The integer value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>True if <paramref name="left"/> is equal to <paramref name="right"/>, otherwise false.</returns>
    public static bool operator ==(int left, GmpFloat right) => right == left;

    /// <summary>
    /// Determines whether a <see cref="int"/> value is not equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The integer value to compare.</param>
    /// <param name="right">The GmpFloat value to compare.</param>
    /// <returns>True if the <paramref name="left"/> is not equal to the <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator !=(int left, GmpFloat right) => right != left;

    /// <summary>
    /// Determines whether a specified integer is greater than a specified <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The integer to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> to compare.</param>
    /// <returns>true if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >(int left, GmpFloat right) => right < left;

    /// <summary>
    /// Determines if the given integer <paramref name="left"/> is less than the specified <see cref="GmpFloat"/> <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The integer value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>true if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <(int left, GmpFloat right) => right > left;

    /// <summary>
    /// Determines whether a specified integer is greater than or equal to a specified <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The integer value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>true if the integer value is greater than or equal to the <see cref="GmpFloat"/> value; otherwise, false.</returns>
    public static bool operator >=(int left, GmpFloat right) => right <= left;

    /// <summary>
    /// Determines if a given integer <paramref name="left"/> is less than or equal to a <see cref="GmpFloat"/> <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The integer value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>True if the integer value is less than or equal to the <see cref="GmpFloat"/> value, otherwise false.</returns>
    public static bool operator <=(int left, GmpFloat right) => right >= left;

    /// <summary>
    /// Determines if a <see cref="uint"/> value is equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The unsigned integer value to compare.</param>
    /// <param name="right">The GmpFloat value to compare.</param>
    /// <returns>True if the values are equal, false otherwise.</returns>
    public static bool operator ==(uint left, GmpFloat right) => right == left;

    /// <summary>
    /// Determines if a <see cref="uint"/> value is not equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The uint value to compare.</param>
    /// <param name="right">The GmpFloat value to compare.</param>
    /// <returns>True if the values are not equal, false otherwise.</returns>
    public static bool operator !=(uint left, GmpFloat right) => right != left;

    /// <summary>
    /// Determines if a <see cref="uint"/> value is greater than a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The unsigned integer value.</param>
    /// <param name="right">The GmpFloat value.</param>
    /// <returns>true if the <paramref name="left"/> is greater than the <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >(uint left, GmpFloat right) => right < left;

    /// <summary>
    /// Determines if a <see cref="uint"/> value is less than a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The left-hand side <see cref="uint"/> value.</param>
    /// <param name="right">The right-hand side <see cref="GmpFloat"/> value.</param>
    /// <returns>True if the <paramref name="left"/> is less than <paramref name="right"/>, otherwise false.</returns>
    public static bool operator <(uint left, GmpFloat right) => right > left;

    /// <summary>
    /// Determines if a <see cref="uint"/> value is greater than or equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="uint"/> value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>True if the <paramref name="left"/> is greater than or equal to the <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator >=(uint left, GmpFloat right) => right <= left;

    /// <summary>
    /// Determines whether a <see cref="uint"/> value is less than or equal to a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="uint"/> value.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value.</param>
    /// <returns>true if the <paramref name="left"/> value is less than or equal to the <paramref name="right"/> value; otherwise, false.</returns>
    public static bool operator <=(uint left, GmpFloat right) => right >= left;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> and a <see cref="GmpFloat"/> are equal.
    /// </summary>
    /// <param name="left">The <see cref="GmpInteger"/> to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> to compare.</param>
    /// <returns>True if the <see cref="GmpInteger"/> and <see cref="GmpFloat"/> are equal, otherwise false.</returns>
    public static bool operator ==(GmpInteger left, GmpFloat right) => right == left;

    /// <summary>
    /// Determines whether two instances of <see cref="GmpInteger"/> and <see cref="GmpFloat"/> are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>true if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(GmpInteger left, GmpFloat right) => right != left;

    /// <summary>
    /// Determines if a <see cref="GmpInteger"/> is greater than a <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The <see cref="GmpInteger"/> to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> to compare.</param>
    /// <returns>True if the <see cref="GmpInteger"/> is greater than the <see cref="GmpFloat"/>; otherwise, false.</returns>
    public static bool operator >(GmpInteger left, GmpFloat right) => right < left;

    /// <summary>
    /// Determines if a <see cref="GmpInteger"/> is less than a <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The <see cref="GmpInteger"/> to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> to compare.</param>
    /// <returns>True if the <see cref="GmpInteger"/> is less than the <see cref="GmpFloat"/>; otherwise, false.</returns>
    public static bool operator <(GmpInteger left, GmpFloat right) => right > left;

    /// <summary>
    /// Determines if the <see cref="GmpInteger"/> value is greater than or equal to the <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="left">The <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="right">The <see cref="GmpFloat"/> value to compare.</param>
    /// <returns>True if the <see cref="GmpInteger"/> value is greater than or equal to the <see cref="GmpFloat"/> value, otherwise false.</returns>
    public static bool operator >=(GmpInteger left, GmpFloat right) => right <= left;

    /// <summary>
    /// Determines whether a specified <see cref="GmpInteger"/> is less than or equal to another specified <see cref="GmpFloat"/>.
    /// </summary>
    /// <param name="left">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="right">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns>true if the value of <paramref name="left"/> is less than or equal to the value of <paramref name="right"/>; otherwise, false.</returns>
    public static bool operator <=(GmpInteger left, GmpFloat right) => right >= left;

    /// <summary>
    /// Compares a <see cref="GmpFloat"/> <paramref name="op1"/> with a <see cref="GmpInteger"/> <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> operand.</param>
    /// <returns>
    /// A negative value if <paramref name="op1"/> is less than <paramref name="op2"/>,
    /// zero if they are equal, and a positive value if <paramref name="op1"/> is greater than <paramref name="op2"/>.
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
    /// Compares two <see cref="GmpFloat"/> instances, <paramref name="op1"/> and a <paramref name="op2"/> of type double.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The second double value to compare.</param>
    /// <returns>A negative value if op1 is less than op2, zero if op1 is equal to op2, and a positive value if op1 is greater than op2.</returns>
    public static unsafe int Compare(GmpFloat op1, double op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_d((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compares the value of a <see cref="GmpFloat"/> instance <paramref name="op1"/> to an integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The integer value to compare.</param>
    /// <returns>
    /// A negative value if <paramref name="op1"/> is less than <paramref name="op2"/>,
    /// zero if they are equal, and a positive value if <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, int op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_si((IntPtr)pop1, new CLong(op2));
        }
    }

    /// <summary>
    /// Compare two <see cref="GmpFloat"/> instances, <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to compare.</param>
    /// <returns>
    /// A negative value if <paramref name="op1"/> is less than <paramref name="op2"/>,
    /// zero if they are equal, and a positive value if <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Compares a <see cref="GmpFloat"/> value with an unsigned integer value. This function is mathematically ill-defined and should not be used.
    /// </summary>
    /// <param name="op1">The <see cref="GmpFloat"/> value to compare.</param>
    /// <param name="op2">The unsigned integer value to compare.</param>
    /// <returns>A negative value if <paramref name="op1"/> is less than <paramref name="op2"/>, zero if they are equal, and a positive value if <paramref name="op1"/> is greater than <paramref name="op2"/>.</returns>
    /// <remarks>This function is marked as obsolete and should not be used.</remarks>
    [Obsolete("This function is mathematically ill-defined and should not be used.")]
    public static unsafe int MpfEquals(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Computes the relative difference between two <see cref="GmpFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op1">The first <see cref="GmpFloat"/> value to compute the relative difference.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> value to compute the relative difference.</param>
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
    /// Calculate the relative difference between two <see cref="GmpFloat"/> instances, <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand.</param>
    /// <param name="precision">The precision of the result in bits. If set to 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the relative difference.</returns>
    public static unsafe GmpFloat RelDiff(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Gets the sign of the <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <value>Returns -1 if the value is negative, 1 if the value is positive, and 0 if the value is zero.</value>
    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;

    #endregion

    #region Misc Functions

    /// <summary>
    /// Computes the smallest integer greater than or equal to the given <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to compute the ceiling value for.</param>
    public static unsafe void CeilInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_ceil((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Calculate the smallest integer, greater than or equal to the specified <paramref name="op"/> and store the result in a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The input <see cref="GmpFloat"/> value.</param>
    /// <param name="precision">The precision of the result in bits. If set to 0, the <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance containing the result of the ceiling operation.</returns>
    public static GmpFloat Ceil(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        CeilInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Computes the largest integer not greater than <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance where the result will be stored.</param>
    /// <param name="op">The <see cref="GmpFloat"/> instance to compute the floor of.</param>
    public static unsafe void FloorInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_floor((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Calculate the largest integer value less than or equal to the given <paramref name="op"/> and return the result as a new <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The input <see cref="GmpFloat"/> value.</param>
    /// <param name="precision">The precision of the result in bits (optional, default is 0).</param>
    /// <returns>A new <see cref="GmpFloat"/> instance representing the floor value of the input.</returns>
    public static GmpFloat Floor(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds the given <paramref name="op"/> <see cref="GmpFloat"/> value towards zero and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the rounded result.</param>
    /// <param name="op">The <see cref="GmpFloat"/> value to round.</param>
    public static unsafe void RoundInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_trunc((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Rounds a <see cref="GmpFloat"/> value to the specified precision.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> value to round.</param>
    /// <param name="precision">The precision in bits to round the value to. Default is 0, which uses the current precision of the input value.</param>
    /// <returns>A new <see cref="GmpFloat"/> instance with the rounded value.</returns>
    public static GmpFloat Round(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        RoundInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Gets a value indicating whether the current <see cref="GmpFloat"/> instance represents an integer value.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current instance represents an integer value; otherwise, <c>false</c>.
    /// </value>
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
    /// Determines whether the current <see cref="GmpFloat"/> value fits into an Int32.
    /// </summary>
    /// <returns>True if the value fits into an Int32, otherwise false.</returns>
    public unsafe bool FitsInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sint_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="GmpFloat"/> value can be represented as an unsigned 32-bit integer.
    /// </summary>
    /// <returns>Returns true if the value can be represented as an unsigned 32-bit integer, otherwise false.</returns>
    public unsafe bool FitsUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_uint_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="GmpFloat"/> value can be represented as a 16-bit signed integer.
    /// </summary>
    /// <returns><c>true</c> if the value can be represented as a 16-bit signed integer; otherwise, <c>false</c>.</returns>
    public unsafe bool FitsInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sshort_p((IntPtr)ptr) != 0;
        }
    }

    /// <summary>
    /// Determines if the <see cref="GmpFloat"/> value fits into a UInt16 without truncation or rounding.
    /// </summary>
    /// <returns>True if the value fits into a UInt16, otherwise false.</returns>
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
    /// Dispose the current <see cref="GmpFloat"/> instance, clearing its resources and marking it as disposed.
    /// </summary>
    /// <param name="disposing">Indicates whether the method call comes from a Dispose method (true) or a finalizer (false).</param>
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

    /// <summary>
    /// Finalizes an instance of the <see cref="GmpFloat"/> class and releases unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Do not modify this code. Instead, put cleanup code in the Dispose(bool disposing) method.
    /// </remarks>
    ~GmpFloat()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes the current <see cref="GmpFloat"/> instance and releases associated resources.
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
    /// Obsolete. Populates the provided <see cref="GmpFloat"/> instance with a random value using the specified maximum limb count and maximum exponent.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to populate with a random value.</param>
    /// <param name="maxLimbCount">The maximum number of limbs for the random value.</param>
    /// <param name="maxExp">The maximum exponent for the random value.</param>
    /// <remarks>Use <see cref="GmpRandom"/> instead.</remarks>
    /// <seealso cref="GmpRandom"/>
    [Obsolete("use GmpRandom")]
    public static unsafe void Random2Inplace(GmpFloat rop, int maxLimbCount, int maxExp)
    {
        fixed (Mpf_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpf_random2((IntPtr)ptr, new CLong(maxLimbCount), new CLong(maxExp));
        }
    }

    /// <summary>
    /// Generates a random <see cref="GmpFloat"/> with the specified <paramref name="precision"/>, <paramref name="maxLimbCount"/>, and <paramref name="maxExp"/>. This method is obsolete and it is recommended to use <see cref="GmpRandom"/> instead.
    /// </summary>
    /// <param name="precision">The precision of the resulting GmpFloat in bits.</param>
    /// <param name="maxLimbCount">The maximum number of limbs in the generated GmpFloat.</param>
    /// <param name="maxExp">The maximum exponent in the generated GmpFloat.</param>
    /// <returns>A random <see cref="GmpFloat"/> with the specified parameters.</returns>
    /// <remarks>This method is marked as obsolete and should not be used in new code.</remarks>
    [Obsolete("use GmpRandom")]
    public static GmpFloat Random2(uint precision, int maxLimbCount, int maxExp)
    {
        GmpFloat rop = new(precision);
        Random2Inplace(rop, maxLimbCount, maxExp);
        return rop;
    }
    #endregion
}
