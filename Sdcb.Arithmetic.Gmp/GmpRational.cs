using System;
using System.Diagnostics.CodeAnalysis;

using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

public class GmpRational : IDisposable
{
    public IntPtr Raw;
    private bool _disposed = false;
    private readonly bool _isOwner;

    #region Initialization and Assignment Functions
    public unsafe GmpRational()
    {
        Raw = Mpq_t.Alloc();
        GmpLib.__gmpq_init(Raw);
        _isOwner = true;
    }

    public GmpRational(IntPtr raw, bool isOwner)
    {
        Raw = raw;
        _isOwner = isOwner;
    }

    /// <summary>
    /// <para>Remove any factors that are common to the numerator and denominator of op, and make the denominator positive.</para>
    /// <para>example: 5/-10 -> -1/2</para>
    /// </summary>
    public unsafe void Canonicalize()
    {
        GmpLib.__gmpq_canonicalize(Raw);
    }

    #region From
    public static GmpRational From(GmpRational op)
    {
        GmpRational r = new();
        r.Assign(op);
        return r;
    }

    public static GmpRational From(GmpInteger op)
    {
        GmpRational r = new();
        r.Assign(op);
        return r;
    }

    public static GmpRational From(uint num, uint den)
    {
        GmpRational r = new();
        r.Assign(num, den);
        return r;
    }

    public static GmpRational From(int num, uint den)
    {
        GmpRational r = new();
        r.Assign(num, den);
        return r;
    }

    /// <summary>
    /// <para>Set rop from a null-terminated string str in the given base.</para>
    /// <para>The string can be an integer like “41” or a fraction like “41/152”. The fraction must be in canonical form (see Rational Number Functions), or if not then mpq_canonicalize must be called.</para>
    /// <para>The numerator and optional denominator are parsed the same as in mpz_set_str (see Assigning Integers). White space is allowed in the string, and is simply ignored. The base can vary from 2 to 62, or if base is 0 then the leading characters are used: 0x or 0X for hex, 0b or 0B for binary, 0 for octal, or decimal otherwise. Note that this is done separately for the numerator and denominator, so for instance 0xEF/100 is 239/100, whereas 0xEF/0x100 is 239/256.</para>
    /// </summary>
    public static unsafe bool TryParse(string str, [MaybeNullWhen(returnValue: false)] out GmpRational rop, int @base = 0)
    {
        GmpRational r = new();
        byte[] strData = Encoding.UTF8.GetBytes(str);
        fixed (byte* strPtr = strData)
        {
            int ret = GmpLib.__gmpq_set_str(r.Raw, (IntPtr)strPtr, @base);
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
    /// <para>Set rop from a null-terminated string str in the given base.</para>
    /// <para>The string can be an integer like “41” or a fraction like “41/152”. The fraction must be in canonical form (see Rational Number Functions), or if not then mpq_canonicalize must be called.</para>
    /// <para>The numerator and optional denominator are parsed the same as in mpz_set_str (see Assigning Integers). White space is allowed in the string, and is simply ignored. The base can vary from 2 to 62, or if base is 0 then the leading characters are used: 0x or 0X for hex, 0b or 0B for binary, 0 for octal, or decimal otherwise. Note that this is done separately for the numerator and denominator, so for instance 0xEF/100 is 239/100, whereas 0xEF/0x100 is 239/256.</para>
    /// </summary>
    public static unsafe GmpRational Parse(string str, int @base = 0)
    {
        GmpRational r = new();
        r.Assign(str, @base);
        return r;
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public static GmpRational From(double val)
    {
        GmpRational r = new();
        r.Assign(val);
        return r;
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public static GmpRational From(GmpFloat val)
    {
        GmpRational r = new();
        r.Assign(val);
        return r;
    }
    #endregion

    #region Assign

    public unsafe void Assign(GmpRational op)
    {
        GmpLib.__gmpq_set(Raw, op.Raw);
    }

    public unsafe void Assign(GmpInteger op)
    {
        GmpLib.__gmpq_set_z(Raw, op.Raw);
    }

    public unsafe void Assign(uint num, uint den)
    {
        GmpLib.__gmpq_set_ui(Raw, num, den);
    }

    public unsafe void Assign(int num, uint den)
    {
        GmpLib.__gmpq_set_si(Raw, num, den);
    }

    /// <summary>
    /// <para>Set rop from a null-terminated string str in the given base.</para>
    /// <para>The string can be an integer like “41” or a fraction like “41/152”. The fraction must be in canonical form (see Rational Number Functions), or if not then mpq_canonicalize must be called.</para>
    /// <para>The numerator and optional denominator are parsed the same as in mpz_set_str (see Assigning Integers). White space is allowed in the string, and is simply ignored. The base can vary from 2 to 62, or if base is 0 then the leading characters are used: 0x or 0X for hex, 0b or 0B for binary, 0 for octal, or decimal otherwise. Note that this is done separately for the numerator and denominator, so for instance 0xEF/100 is 239/100, whereas 0xEF/0x100 is 239/256.</para>
    /// </summary>
    public unsafe void Assign(string str, int @base = 0)
    {
        byte[] strData = Encoding.UTF8.GetBytes(str);
        fixed (byte* strPtr = strData)
        {
            int ret = GmpLib.__gmpq_set_str(Raw, (IntPtr)strPtr, @base);
            if (ret != 0)
            {
                throw new ArgumentException($"Failed to parse {str}, base={@base} to GmpRational, __gmpq_set_str returns {ret}");
            }
        }
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public unsafe void Assign(double val)
    {
        GmpLib.__gmpq_set_d(Raw, val);
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public unsafe void Assign(GmpFloat val)
    {
        GmpLib.__gmpq_set_f(Raw, val.Raw);
    }

    public static unsafe void Swap(GmpRational op1, GmpRational op2)
    {
        GmpLib.__gmpq_swap(op1.Raw, op2.Raw);
    }

    public unsafe GmpRational Clone()
    {
        GmpRational r = new();
        r.Assign(this);
        return r;
    }
    #endregion
    #endregion

    #region Conversion Functions
    /// <summary>
    /// <para>Convert op to a double, truncating if necessary (i.e. rounding towards zero).</para>
    /// <para>If the exponent from the conversion is too big or too small to fit a double then the result is system dependent. For too big an infinity is returned when available. For too small 0.0 is normally returned. Hardware overflow, underflow and denorm traps may or may not occur.</para>
    /// </summary>
    public unsafe double ToDouble()
    {
        return GmpLib.__gmpq_get_d(Raw);
    }

    /// <summary>
    /// <para>Convert op to a double, truncating if necessary (i.e. rounding towards zero).</para>
    /// <para>If the exponent from the conversion is too big or too small to fit a double then the result is system dependent. For too big an infinity is returned when available. For too small 0.0 is normally returned. Hardware overflow, underflow and denorm traps may or may not occur.</para>
    /// </summary>
    public static explicit operator double(GmpRational op) => op.ToDouble();

    /// <exception cref="ArgumentException"></exception>
    public override string ToString() => ToString(@base: 10);

    /// <summary>
    /// <para>Convert op to a string of digits in base base. The base argument may vary from 2 to 62 or from -2 to -36. The string will be of the form ‘num/den’, or if the denominator is 1 then just ‘num’.</para>
    /// </summary>
    /// <param name="base">For base in the range 2..36, digits and lower-case letters are used; for -2..-36, digits and upper-case letters are used; for 37..62, digits, upper-case letters, and lower-case letters (in that significance order) are used.</param>
    /// <exception cref="ArgumentException"></exception>
    public unsafe string ToString(int @base)
    {
        IntPtr ret = GmpLib.__gmpq_get_str(IntPtr.Zero, @base, Raw);
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
    #endregion

    #region Dispose & Clear
    private unsafe void Clear()
    {
        GmpLib.__gmpq_clear(Raw);
        Marshal.FreeHGlobal(Raw);
        Raw = IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
            }

            if (_isOwner) Clear();
            _disposed = true;
        }
    }

    ~GmpRational()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Arithmetic Functions
    public static unsafe void AddInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        GmpLib.__gmpq_add(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpRational Add(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        AddInplace(rop, op1, op2);
        return rop;
    }

    public static GmpRational operator +(GmpRational op1, GmpRational op2) => Add(op1, op2);

    public static unsafe void SubtractInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        GmpLib.__gmpq_sub(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpRational Subtract(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static GmpRational operator -(GmpRational op1, GmpRational op2) => Subtract(op1, op2);

    public static unsafe void MultiplyInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        GmpLib.__gmpq_mul(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpRational Multiply(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    public static GmpRational operator *(GmpRational op1, GmpRational op2) => Multiply(op1, op2);

    public static unsafe void Multiply2ExpInplace(GmpRational rop, GmpRational op1, uint bitCount)
    {
        GmpLib.__gmpq_mul_2exp(rop.Raw, op1.Raw, bitCount);
    }

    public static GmpRational Multiply2Exp(GmpRational op1, uint bitCount)
    {
        GmpRational rop = new();
        Multiply2ExpInplace(rop, op1, bitCount);
        return rop;
    }

    public static GmpRational operator <<(GmpRational op1, uint bitCount) => Multiply2Exp(op1, bitCount);

    public static unsafe void DivideInplace(GmpRational rop, GmpRational op1, GmpRational op2)
    {
        GmpLib.__gmpq_div(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpRational Divide(GmpRational op1, GmpRational op2)
    {
        GmpRational rop = new();
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static GmpRational operator /(GmpRational op1, GmpRational op2) => Divide(op1, op2);

    public static unsafe void Divide2ExpInplace(GmpRational rop, GmpRational op1, uint bitCount)
    {
        GmpLib.__gmpq_div_2exp(rop.Raw, op1.Raw, bitCount);
    }

    public static GmpRational Divide2Exp(GmpRational op1, uint bitCount)
    {
        GmpRational rop = new();
        Divide2ExpInplace(rop, op1, bitCount);
        return rop;
    }

    public static GmpRational operator >>(GmpRational op1, uint bitCount) => Divide2Exp(op1, bitCount);

    public static unsafe void NegateInplace(GmpRational rop, GmpRational op)
    {
        GmpLib.__gmpq_neg(rop.Raw, op.Raw);
    }

    public void NegateInplace() => NegateInplace(this, this);

    public static GmpRational Negate(GmpRational op)
    {
        GmpRational rop = new();
        NegateInplace(rop, op);
        return rop;
    }

    public static GmpRational operator -(GmpRational op) => Negate(op);

    public static unsafe void AbsInplace(GmpRational rop, GmpRational op)
    {
        GmpLib.__gmpq_abs(rop.Raw, op.Raw);
    }

    public static GmpRational Abs(GmpRational op)
    {
        GmpRational rop = new();
        AbsInplace(rop, op);
        return rop;
    }

    public static unsafe void InvertInplace(GmpRational rop, GmpRational op)
    {
        GmpLib.__gmpq_inv(rop.Raw, op.Raw);
    }

    public void InvertInplace() => InvertInplace(this, this);

    public static GmpRational Invert(GmpRational op)
    {
        GmpRational rop = new();
        InvertInplace(rop, op);
        return rop;
    }
    #endregion

    #region Comparison Functions
    /// <summary>
    /// <para>Compare op1 and op2. Return a positive value if op1 &gt; op2, zero if op1 = op2, and a negative value if op1 &lt; op2.</para>
    /// <para>To determine if two rationals are equal, mpq_equal is faster than mpq_cmp.</para>
    /// </summary>
    public static unsafe int Compare(GmpRational op1, GmpRational op2)
    {
        return GmpLib.__gmpq_cmp(op1.Raw, op2.Raw);
    }

    public static bool operator >(GmpRational op1, GmpRational op2) => Compare(op1, op2) > 0;
    public static bool operator >=(GmpRational op1, GmpRational op2) => Compare(op1, op2) >= 0;
    public static bool operator <(GmpRational op1, GmpRational op2) => Compare(op1, op2) < 0;
    public static bool operator <=(GmpRational op1, GmpRational op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(GmpRational op1, GmpRational op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpRational op1, GmpRational op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// <para>Compare op1 and op2. Return a positive value if op1 &gt; op2, zero if op1 = op2, and a negative value if op1 &lt; op2.</para>
    /// <para>To determine if two rationals are equal, mpq_equal is faster than mpq_cmp.</para>
    /// </summary>
    public static unsafe int Compare(GmpRational op1, GmpInteger op2)
    {
        return GmpLib.__gmpq_cmp_z(op1.Raw, op2.Raw);
    }

    public static bool operator >(GmpRational op1, GmpInteger op2) => Compare(op1, op2) > 0;
    public static bool operator >=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) >= 0;
    public static bool operator <(GmpRational op1, GmpInteger op2) => Compare(op1, op2) < 0;
    public static bool operator <=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(GmpRational op1, GmpInteger op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpRational op1, GmpInteger op2) => Compare(op1, op2) != 0;

    public static bool operator >(GmpInteger op1, GmpRational op2) => Compare(op2, op1) < 0;
    public static bool operator >=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) <= 0;
    public static bool operator <(GmpInteger op1, GmpRational op2) => Compare(op2, op1) > 0;
    public static bool operator <=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) <= 0;
    public static bool operator ==(GmpInteger op1, GmpRational op2) => Compare(op2, op1) == 0;
    public static bool operator !=(GmpInteger op1, GmpRational op2) => Compare(op2, op1) != 0;

    public static unsafe int Compare(GmpRational op1, uint num, uint den)
    {
        return GmpLib.__gmpq_cmp_ui(op1.Raw, num, den);
    }

    public static unsafe int Compare(GmpRational op1, int num, uint den)
    {
        return GmpLib.__gmpq_cmp_si(op1.Raw, num, den);
    }

    public static unsafe bool Equals(GmpRational op1, GmpRational op2)
    {
        return GmpLib.__gmpq_equal(op1.Raw, op2.Raw) != 0;
    }

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

    public override int GetHashCode() => Raw.GetHashCode();
    #endregion

    #region Applying Integer Functions to Rationals
    public unsafe GmpInteger Num
    {
        get => new((IntPtr)(&((Mpq_t*)Raw)->Num), isOwner: false);
        set
        {
            GmpLib.__gmpq_set_num(Raw, value.Raw);
        }
    }

    public unsafe GmpInteger Den
    {
        get => new((IntPtr)(&((Mpq_t*)Raw)->Den), isOwner: false);
        set
        {
            GmpLib.__gmpq_set_den(Raw, value.Raw);
        }
    }
    #endregion
}

[StructLayout(LayoutKind.Sequential)]
public struct Mpq_t
{
    public Mpz_t Num, Den;

    public static unsafe int RawSize => sizeof(Mpq_t);

    public static unsafe IntPtr Alloc() => Marshal.AllocHGlobal(sizeof(Mpq_t));

    public override int GetHashCode() => HashCode.Combine(Num.GetHashCode(), Den.GetHashCode());
}