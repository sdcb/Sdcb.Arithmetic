using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>Represents a rational number using the GMP library, a large number library for arithmetic calculations.</summary>
/// <remarks>This class provides an implementation of rational numbers using the GMP library.</remarks>
public class GmpRational : IDisposable, IComparable, IComparable<GmpInteger>, IEquatable<GmpInteger>
{
    internal readonly Mpq_t Raw = new();

    #region Initialization and Assignment Functions

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpRational"/> class with a new <see cref="Mpq_t"/> instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="Mpq_t"/> instance is initialized by calling the <see cref="GmpLib.__gmpq_init"/> function from the GMP library.
    /// </remarks>
    public unsafe GmpRational()
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            GmpLib.__gmpq_init((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpRational"/> class with the specified <paramref name="raw"/> value.
    /// </summary>
    /// <param name="raw">The <see cref="Mpq_t"/> value to initialize the instance with.</param>
    /// <remarks>
    /// The <paramref name="raw"/> value is assigned to the <see cref="Raw"/> field of the instance.
    /// </remarks>
    public GmpRational(Mpq_t raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Canonicalize the current <see cref="GmpRational"/> instance, which means to reduce the fraction to its lowest terms.
    /// </summary>
    public unsafe void Canonicalize()
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_canonicalize((IntPtr)pthis);
        }
    }

    #region From

    /// <summary>
    /// Create a new instance of <see cref="GmpRational"/> from an existing <paramref name="op"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to create a new instance from.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> with the same value as <paramref name="op"/>.</returns>
    public static GmpRational From(GmpRational op)
    {
        GmpRational r = new();
        r.Assign(op);
        return r;
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpRational"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRational"/> that is a copy of this instance.</returns>
    public GmpRational Clone() => From(this);

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from a <see cref="GmpInteger"/> value.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> value to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static GmpRational From(GmpInteger op)
    {
        GmpRational r = new();
        r.Assign(op);
        return r;
    }

    /// <summary>
    /// Implicitly converts a <see cref="GmpInteger"/> instance to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static implicit operator GmpRational(GmpInteger op) => From(op);

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from a numerator <paramref name="num"/> and a denominator <paramref name="den"/>.
    /// </summary>
    /// <param name="num">The numerator of the rational number.</param>
    /// <param name="den">The denominator of the rational number.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the rational number.</returns>
    public static GmpRational From(uint num, uint den)
    {
        GmpRational r = new();
        r.Assign(num, den);
        return r;
    }

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from a numerator <paramref name="num"/> and a denominator <paramref name="den"/>.
    /// </summary>
    /// <param name="num">The numerator value of the rational number.</param>
    /// <param name="den">The denominator value of the rational number.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the rational number.</returns>
    public static GmpRational From(int num, uint den)
    {
        GmpRational r = new();
        r.Assign(num, den);
        return r;
    }

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from an integer <paramref name="num"/>.
    /// </summary>
    /// <param name="num">The numerator of the rational number.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the rational number with <paramref name="num"/> as numerator and 1 as denominator.</returns>
    public static GmpRational From(int num)
    {
        GmpRational r = new();
        r.Assign(num, 1);
        return r;
    }

    /// <summary>
    /// Implicitly converts an integer value to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static implicit operator GmpRational(int op) => From(op);

    /// <summary>
    /// Tries to parse the string representation of a rational number in the specified base and returns a value indicating whether the conversion succeeded.
    /// </summary>
    /// <param name="str">The string representation of the rational number to parse.</param>
    /// <param name="rop">When this method returns, contains the <see cref="GmpRational"/> equivalent of the string representation, if the conversion succeeded, or <see langword="null"/> if the conversion failed. The conversion fails if the <paramref name="str"/> parameter is <see langword="null"/>, is not of the correct format. This parameter is passed uninitialized.</param>
    /// <param name="base">The base of the number in <paramref name="str"/>, which must be between 2 and 62, or 0 to detect the base automatically. The default value is 0.</param>
    /// <returns><see langword="true"/> if the <paramref name="str"/> parameter was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static unsafe bool TryParse(string str, [MaybeNullWhen(returnValue: false)] out GmpRational rop, int @base = 0)
    {
        GmpRational r = new();
        byte[] strData = Encoding.UTF8.GetBytes(str);
        fixed (Mpq_t* pthis = &r.Raw)
        fixed (byte* strPtr = strData)
        {
            int ret = GmpLib.__gmpq_set_str((IntPtr)pthis, (IntPtr)strPtr, @base);
            if (ret != 0)
            {
                rop = null;
                r.Dispose();
                return false;
            }
            else
            {
                rop = r;
                return true;
            }
        }
    }

    /// <summary>
    /// Parses the string representation of a rational number and returns a new instance of <see cref="GmpRational"/> that represents the parsed value.
    /// </summary>
    /// <param name="str">The string representation of the rational number to parse.</param>
    /// <param name="base">The base of the number in <paramref name="str"/>. Default is 0, which means the base is determined by the prefix of the string (e.g. "0x" for hexadecimal).</param>
    /// <returns>A new instance of <see cref="GmpRational"/> that represents the parsed value.</returns>
    public static unsafe GmpRational Parse(string str, int @base = 0)
    {
        GmpRational r = new();
        r.Assign(str, @base);
        return r;
    }

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from a double-precision floating-point number <paramref name="val"/>.
    /// </summary>
    /// <param name="val">The double-precision floating-point number to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static GmpRational From(double val)
    {
        GmpRational r = new();
        r.Assign(val);
        return r;
    }

    /// <summary>
    /// Explicitly converts a double-precision floating-point number to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The double-precision floating-point number to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static explicit operator GmpRational(double op) => From(op);

    /// <summary>
    /// Create a <see cref="GmpRational"/> instance from a <see cref="GmpFloat"/> value.
    /// </summary>
    /// <param name="val">The <see cref="GmpFloat"/> value to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static GmpRational From(GmpFloat val)
    {
        GmpRational r = new();
        r.Assign(val);
        return r;
    }

    /// <summary>
    /// Explicitly converts a <see cref="GmpFloat"/> instance to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static explicit operator GmpRational(GmpFloat op) => From(op);
    #endregion

    #region Assign

    /// <summary>
    /// Assigns the value of <paramref name="op"/> to the current instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to assign from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op"/> is null.</exception>
    /// <remarks>
    /// This method sets the value of the current instance to the value of <paramref name="op"/>.
    /// </remarks>
    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpq_t* r = &op.Raw)
        {
            GmpLib.__gmpq_set((IntPtr)pthis, (IntPtr)r);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpInteger"/> to this <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to assign the value from.</param>
    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpz_t* r = &op.Raw)
        {
            GmpLib.__gmpq_set_z((IntPtr)pthis, (IntPtr)r);
        }
    }

    /// <summary>
    /// Assigns a new value to the current <see cref="GmpRational"/> instance with the specified numerator and denominator.
    /// </summary>
    /// <param name="num">The numerator of the rational number.</param>
    /// <param name="den">The denominator of the rational number.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="den"/> is zero.</exception>
    /// <remarks>
    /// This method sets the value of the current <see cref="GmpRational"/> instance to the rational number whose numerator is <paramref name="num"/> and denominator is <paramref name="den"/>.
    /// </remarks>
    public unsafe void Assign(uint num, uint den)
    {
        if (den == 0) throw new DivideByZeroException();
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_ui((IntPtr)pthis, num, den);
        }
    }

    /// <summary>
    /// Assigns a rational number represented by numerator <paramref name="num"/> and denominator <paramref name="den"/> to the current instance.
    /// </summary>
    /// <param name="num">The numerator of the rational number to assign.</param>
    /// <param name="den">The denominator of the rational number to assign.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="den"/> is zero.</exception>
    /// <remarks>
    /// This method sets the value of the current instance to the rational number represented by <paramref name="num"/> and <paramref name="den"/>.
    /// </remarks>
    public unsafe void Assign(int num, uint den)
    {
        if (den == 0) throw new DivideByZeroException();
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_si((IntPtr)pthis, num, den);
        }
    }

    /// <summary>
    /// Assigns a string representation of a rational number to the current instance.
    /// </summary>
    /// <param name="str">The string representation of the rational number.</param>
    /// <param name="base">The base of the number system used in the string representation. Default is 0, which means the base is determined by the prefix of the string (e.g. "0x" for hexadecimal).</param>
    /// <exception cref="ArgumentException">Thrown when the string cannot be parsed as a rational number.</exception>
    public unsafe void Assign(string str, int @base = 0)
    {
        byte[] strData = Encoding.UTF8.GetBytes(str);
        fixed (Mpq_t* pthis = &Raw)
        fixed (byte* strPtr = strData)
        {
            int ret = GmpLib.__gmpq_set_str((IntPtr)pthis, (IntPtr)strPtr, @base);
            if (ret != 0)
            {
                throw new ArgumentException($"Failed to parse {str}, base={@base} to GmpRational, __gmpq_set_str returns {ret}");
            }
        }
    }

    /// <summary>
    /// Assigns the value of a double-precision floating-point number to the current <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="val">The double-precision floating-point number to assign.</param>
    public unsafe void Assign(double val)
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_d((IntPtr)pthis, val);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpFloat"/> instance to this <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="val">The <see cref="GmpFloat"/> instance to assign.</param>
    /// <remarks>
    /// This method sets the value of this <see cref="GmpRational"/> instance to the value of <paramref name="val"/>.
    /// </remarks>
    public unsafe void Assign(GmpFloat val)
    {
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpf_t* pval = &val.Raw)
        {
            GmpLib.__gmpq_set_f((IntPtr)pthis, (IntPtr)pval);
        }
    }

    /// <summary>
    /// Swaps the values of two <see cref="GmpRational"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> instance.</param>
    /// <remarks>
    /// This method swaps the values of <paramref name="op1"/> and <paramref name="op2"/> by using the <see cref="GmpLib.__gmpq_swap(IntPtr, IntPtr)"/> function.
    /// </remarks>
    public static unsafe void Swap(GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_swap((IntPtr)p1, (IntPtr)p2);
        }
    }
    #endregion
    #endregion

    #region Conversion Functions

    /// <summary>
    /// Converts the current <see cref="GmpRational"/> instance to a double-precision floating-point number.
    /// </summary>
    /// <returns>A double-precision floating-point number that is equivalent to the current <see cref="GmpRational"/> instance.</returns>
    public unsafe double ToDouble()
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            return GmpLib.__gmpq_get_d((IntPtr)pthis);
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="GmpRational"/> instance to a <see cref="double"/> value.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to convert.</param>
    /// <returns>The <see cref="double"/> value converted from the <paramref name="op"/> instance.</returns>
    public static explicit operator double(GmpRational op) => op.ToDouble();

    /// <summary>
    /// Returns a string representation of the current <see cref="GmpFloat"/> object using base 10.
    /// </summary>
    /// <returns>A string representation of the current <see cref="GmpFloat"/> object using base 10.</returns>
    public override string ToString() => ToString(@base: 10);

    /// <summary>
    /// Converts the current <see cref="GmpRational"/> instance to a string representation in the specified <paramref name="base"/>.
    /// </summary>
    /// <param name="base">The base to use for the conversion.</param>
    /// <returns>A string representation of the current <see cref="GmpRational"/> instance in the specified <paramref name="base"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when unable to convert <see cref="GmpRational"/> to string.</exception>
    public unsafe string ToString(int @base)
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            IntPtr ret = GmpLib.__gmpq_get_str(IntPtr.Zero, @base, (IntPtr)ptr);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert GmpRational to string.");
            }

            try
            {
                return Marshal.PtrToStringUTF8(ret)!;
            }
            finally
            {
                GmpMemory.Free(ret);
            }
        }
    }
#endregion

    #region Dispose & Clear
    private bool _disposed;

    private unsafe void Clear()
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            GmpLib.__gmpq_clear((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="GmpRational"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
            }

            Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for <see cref="GmpRational"/> class.
    /// </summary>
    ~GmpRational()
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

    #region Arithmetic Functions

    /// <summary>
    /// Adds two <see cref="GmpRational"/> instances <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result of the addition.</param>
    /// <param name="op1">The first <see cref="GmpRational"/> instance to add.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> instance to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    /// <remarks>The addition is performed in place, meaning the value of <paramref name="rop"/> will be modified.</remarks>
    public static unsafe void AddInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_add((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Adds two <see cref="GmpRational"/> instances and returns the result as a new instance.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static GmpRational Add(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        AddInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Adds two <see cref="GmpRational"/> values and returns the result.
    /// </summary>
    /// <param name="op1">The first value to add.</param>
    /// <param name="op2">The second value to add.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> that represents the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static GmpRational operator +(GmpRational op1, GmpRational op2) => Add(op1, op2);

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpRational"/> instance to subtract from.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> instance to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="op1"/> will be modified.</remarks>
    public static unsafe void SubtractInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Subtracts two <see cref="GmpRational"/> numbers and returns the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the subtraction.</returns>
    public static GmpRational Subtract(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Subtracts two <see cref="GmpRational"/> values and returns the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> value to subtract.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> value to subtract.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the subtraction.</returns>
    public static GmpRational operator -(GmpRational op1, GmpRational op2) => Subtract(op1, op2);

    /// <summary>
    /// Multiplies two <see cref="GmpRational"/> instances and stores the result in the first instance.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpRational"/> instance to multiply.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> instance to multiply.</param>
    /// <remarks>The result of the multiplication is stored in <paramref name="rop"/>.</remarks>
    public static unsafe void MultiplyInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_mul((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Multiplies two <see cref="GmpRational"/> numbers and returns the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the multiplication.</returns>
    public static GmpRational Multiply(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Multiplies two <see cref="GmpRational"/> values and returns the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> value to multiply.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> value to multiply.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the multiplication.</returns>
    public static GmpRational operator *(GmpRational op1, GmpRational op2) => Multiply(op1, op2);

    /// <summary>
    /// Multiply the rational number <paramref name="op1"/> by 2 raised to the power of <paramref name="bitCount"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The rational number to store the result.</param>
    /// <param name="op1">The rational number to be multiplied.</param>
    /// <param name="bitCount">The power of 2 to multiply by.</param>
    /// <remarks>The operation is performed in-place, i.e., the value of <paramref name="rop"/> will be modified.</remarks>
    public static unsafe void Multiply2ExpInplace(GmpRational rop, GmpRational op1, uint bitCount)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpq_mul_2exp((IntPtr)pr, (IntPtr)p1, bitCount);
        }
    }

    /// <summary>
    /// Multiplies a <see cref="GmpRational"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="bitCount"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to multiply.</param>
    /// <param name="bitCount">The power of 2 to raise.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the multiplication.</returns>
    public static GmpRational Multiply2Exp(GmpRational op1, uint bitCount)
    {
        GmpRational rop = new();
        Multiply2ExpInplace(rop, op1, bitCount);
        return rop;
    }

    /// <summary>
    /// Shifts a <see cref="GmpRational"/> value to the left by a specified number of bits.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> value to shift.</param>
    /// <param name="bitCount">The number of bits to shift <paramref name="op1"/> to the left.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> that represents the value of <paramref name="op1"/> shifted to the left by <paramref name="bitCount"/> bits.</returns>
    /// <remarks>
    /// This operator is equivalent to calling <see cref="Multiply2Exp(GmpRational, uint)"/> method with <paramref name="op1"/> and <paramref name="bitCount"/> as arguments.
    /// </remarks>
    public static GmpRational operator <<(GmpRational op1, uint bitCount) => Multiply2Exp(op1, bitCount);

    /// <summary>
    /// Divides <paramref name="op1"/> by <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result of division.</param>
    /// <param name="op1">The <see cref="GmpRational"/> instance to be divided.</param>
    /// <param name="op2">The <see cref="GmpRational"/> instance to divide by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    /// <remarks>
    /// This method performs an in-place division operation, meaning that the value of <paramref name="rop"/> will be modified to store the result of the division.
    /// </remarks>
    public static unsafe void DivideInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_div((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Divide two <see cref="GmpRational"/> numbers <paramref name="op1"/> and <paramref name="op2"/> and return the result as a new <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the quotient of <paramref name="op1"/> divided by <paramref name="op2"/>.</returns>
    public static GmpRational Divide(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        DivideInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Divides two <see cref="GmpRational"/> values and returns the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the quotient of the division.</returns>
    public static GmpRational operator /(GmpRational op1, GmpRational op2) => Divide(op1, op2);

    /// <summary>
    /// Divide a <paramref name="op1"/> by 2 raised to the power of <paramref name="bitCount"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpRational"/> instance to be divided.</param>
    /// <param name="bitCount">The power of 2 to divide by.</param>
    /// <remarks>
    /// This method modifies the value of <paramref name="rop"/> in place.
    /// </remarks>
    public static unsafe void Divide2ExpInplace(GmpRational rop, GmpRational op1, uint bitCount)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpq_div_2exp((IntPtr)pr, (IntPtr)p1, bitCount);
        }
    }

    /// <summary>
    /// Divide a <paramref name="op1"/> by 2 raised to the power of <paramref name="bitCount"/> and return the result as a new instance of <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to divide.</param>
    /// <param name="bitCount">The number of bits to shift right.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the result of the division.</returns>
    public static GmpRational Divide2Exp(GmpRational op1, uint bitCount)
    {
        GmpRational rop = new();
        Divide2ExpInplace(rop, op1, bitCount);
        return rop;
    }

    /// <summary>
    /// Right-shifts the bits of a <see cref="GmpRational"/> instance by a specified number of bits.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to shift.</param>
    /// <param name="bitCount">The number of bits to shift the value of <paramref name="op1"/> to the right.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the shifted value.</returns>
    public static GmpRational operator >>(GmpRational op1, uint bitCount) => Divide2Exp(op1, bitCount);

    /// <summary>
    /// Negates the value of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpRational"/> instance to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op"/> is null.</exception>
    /// <remarks>
    /// This method modifies the value of <paramref name="rop"/> in place.
    /// </remarks>
    public static unsafe void NegateInplace(GmpRational rop, GmpRational op)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_neg((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Negates the current <see cref="GmpRational"/> instance in place.
    /// </summary>
    public void NegateInplace() => NegateInplace(this, this);

    /// <summary>
    /// Negates the specified <paramref name="op"/> <see cref="GmpRational"/> and returns a new instance of <see cref="GmpRational"/> representing the result.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> to negate.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the negated value of <paramref name="op"/>.</returns>
    public static GmpRational Negate(GmpRational op)
    {
        GmpRational rop = new();
        NegateInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Negates the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> to negate.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> that contains the negated value of <paramref name="op"/>.</returns>
    public static GmpRational operator -(GmpRational op) => Negate(op);

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpRational"/> instance in place.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpRational"/> instance to compute the absolute value.</param>
    /// <remarks>The absolute value of a rational number is the non-negative value of the number without regard to its sign.</remarks>
    public static unsafe void AbsInplace(GmpRational rop, GmpRational op)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_abs((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Returns the absolute value of a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to get the absolute value of.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the absolute value of <paramref name="op"/>.</returns>
    public static GmpRational Abs(GmpRational op)
    {
        GmpRational rop = new();
        AbsInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Inverts the value of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpRational"/> instance to store the result of the inversion.</param>
    /// <param name="op">The <see cref="GmpRational"/> instance to be inverted.</param>
    /// <remarks>
    /// The value of <paramref name="op"/> will be inverted and stored in <paramref name="rop"/>.
    /// The original value of <paramref name="op"/> will be lost.
    /// </remarks>
    public static unsafe void InvertInplace(GmpRational rop, GmpRational op)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_inv((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Inverts this <see cref="GmpRational"/> instance in place.
    /// </summary>
    public void InvertInplace() => InvertInplace(this, this);

    /// <summary>
    /// Inverts the given <paramref name="op"/> <see cref="GmpRational"/> and returns a new instance of <see cref="GmpRational"/> representing the result.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> to invert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the inverted value.</returns>
    public static GmpRational Invert(GmpRational op)
    {
        GmpRational rop = new();
        InvertInplace(rop, op);
        return rop;
    }
    #endregion

    #region Comparison Functions

    /// <summary>
    /// Determines whether the current <see cref="GmpInteger"/> object is equal to another <see cref="GmpInteger"/> object.
    /// </summary>
    /// <param name="other">The <see cref="GmpInteger"/> to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="GmpInteger"/> is equal to the current <see cref="GmpInteger"/>; otherwise, <c>false</c>.</returns>
    public bool Equals([AllowNull] GmpInteger other)
    {
        return other is not null && Compare(this, other) == 0;
    }

    /// <summary>
    /// Compares the current instance with another object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> type is not null, int, uint, GmpInteger, or GmpRational.</exception>
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            GmpInteger z => Compare(this, z),
            int i => Compare(this, i),
            uint ui => Compare(this, ui),
            GmpRational r => Compare(this, r),
            _ => throw new ArgumentException($"obj type must be null, int, uint, GmpInteger, GmpRational"),
        };
    }

    /// <summary>
    /// Compares this <see cref="GmpInteger"/> instance with another <see cref="GmpInteger"/> instance and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the other instance.
    /// </summary>
    /// <param name="other">The <see cref="GmpInteger"/> instance to compare with this instance.</param>
    /// <returns>A signed integer that indicates the relative values of this instance and <paramref name="other"/>. Return value is less than zero if this instance is less than <paramref name="other"/>, zero if this instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater than <paramref name="other"/> or <paramref name="other"/> is null.</returns>
    public int CompareTo([AllowNull] GmpInteger other)
    {
        return other is null ? 1 : Compare(this, other);
    }

    /// <summary>
    /// Compares two <see cref="GmpRational"/> objects and returns an integer that indicates whether the first object is less than, equal to, or greater than the second object.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_cmp((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is greater than the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is greater than the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator >(GmpRational op1, GmpRational op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is greater than or equal to the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the first <see cref="GmpRational"/> operand is greater than or equal to the second <see cref="GmpRational"/> operand; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpRational op1, GmpRational op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is less than the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns>true if the first operand is less than the second operand; otherwise, false.</returns>
    public static bool operator <(GmpRational op1, GmpRational op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is less than or equal to the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is less than or equal to the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator <=(GmpRational op1, GmpRational op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare, or null.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare, or null.</param>
    /// <returns>true if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator ==(GmpRational op1, GmpRational op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare, or null.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare, or null.</param>
    /// <returns>true if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator !=(GmpRational op1, GmpRational op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Compares the value of a <see cref="GmpRational"/> instance with a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to compare.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
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
    public static unsafe int Compare(GmpRational op1, GmpInteger op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_cmp_z((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Determines whether a <see cref="GmpRational"/> value is greater than a <see cref="GmpInteger"/> value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> value to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(GmpRational op1, GmpInteger op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is greater than or equal to the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is greater than or equal to the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the first <see cref="GmpRational"/> operand is less than the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns>true if the first operand is less than the second operand; otherwise, false.</returns>
    public static bool operator <(GmpRational op1, GmpInteger op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the value of a <see cref="GmpRational"/> object is less than or equal to the value of a <see cref="GmpInteger"/> object.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> object to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> object to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> and <see cref="GmpInteger"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator ==(GmpRational op1, GmpInteger op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> and <see cref="GmpInteger"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is greater than a <see cref="GmpRational"/> value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> value to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is greater than the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >(GmpInteger op1, GmpRational op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is greater than or equal to a <see cref="GmpRational"/> value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is less than a <see cref="GmpRational"/> value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(GmpInteger op1, GmpRational op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is less than or equal to a <see cref="GmpRational"/> value.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="op2">The <see cref="GmpRational"/> value to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpInteger"/> and <see cref="GmpRational"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(GmpInteger op1, GmpRational op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpInteger"/> and <see cref="GmpRational"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Compares a <see cref="GmpRational"/> instance with an unsigned rational number represented by numerator <paramref name="num"/> and denominator <paramref name="den"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to compare.</param>
    /// <param name="num">The numerator of the unsigned rational number to compare.</param>
    /// <param name="den">The denominator of the unsigned rational number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and the unsigned rational number. If the return value is less than zero, <paramref name="op1"/> is less than the unsigned rational number. If the return value is zero, <paramref name="op1"/> is equal to the unsigned rational number. If the return value is greater than zero, <paramref name="op1"/> is greater than the unsigned rational number.</returns>
    public static unsafe int Compare(GmpRational op1, uint num, uint den)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpq_cmp_ui((IntPtr)p1, num, den);
        }
    }

    /// <summary>
    /// Compares a <see cref="GmpRational"/> instance with a rational number represented by its numerator <paramref name="num"/> and denominator <paramref name="den"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to compare.</param>
    /// <param name="num">The numerator of the rational number to compare.</param>
    /// <param name="den">The denominator of the rational number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and the rational number represented by <paramref name="num"/> and <paramref name="den"/>.
    /// Return value is less than 0 if <paramref name="op1"/> is less than the rational number, 0 if they are equal, and greater than 0 if <paramref name="op1"/> is greater than the rational number.</returns>
    public static unsafe int Compare(GmpRational op1, int num, uint den)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpq_cmp_si((IntPtr)p1, num, den);
        }
    }

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static unsafe bool Equals(GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_equal((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="GmpRational"/> instance is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            GmpRational r => Equals(this, r),
            GmpInteger bi => Compare(this, bi) == 0,
            _ => false
        };
    }

    /// <summary>
    /// Returns the hash code for this <see cref="GmpRational"/> instance.
    /// </summary>
    /// <returns>An integer hash code.</returns>
    public override int GetHashCode() => Raw.GetHashCode();
    #endregion

    #region Applying Integer Functions to Rationals

    /// <summary>
    /// Gets or sets the numerator of the current <see cref="GmpRational"/> instance.
    /// </summary>
    /// <remarks>
    /// Setting the numerator will modify the current instance. The ownership of the numerator is transferred to the current instance.
    /// </remarks>
    /// <value>A new instance of <see cref="GmpInteger"/> representing the numerator of the current instance.</value>
    public unsafe GmpInteger Num
    {
        get => new(Raw.Num, isOwner: false);
        set
        {
            fixed (Mpq_t* pthis = &Raw)
            fixed (Mpz_t* pop = &value.Raw)
            {
                GmpLib.__gmpq_set_num((IntPtr)pthis, (IntPtr)pop);
            }
        }
    }

    /// <summary>
    /// Gets or sets the denominator of the current <see cref="GmpRational"/> instance.
    /// </summary>
    /// <remarks>
    /// The getter returns a new instance of <see cref="GmpInteger"/> representing the denominator of the current <see cref="GmpRational"/> instance.
    /// The setter sets the denominator of the current <see cref="GmpRational"/> instance to the value of <paramref name="value"/>.
    /// </remarks>
    public unsafe GmpInteger Den
    {
        get => new(Raw.Den, isOwner: false);
        set
        {
            fixed (Mpq_t* pthis = &Raw)
            fixed (Mpz_t* pop = &value.Raw)
            {
                GmpLib.__gmpq_set_den((IntPtr)pthis, (IntPtr)pop);
            }
        }
    }
    #endregion
}
