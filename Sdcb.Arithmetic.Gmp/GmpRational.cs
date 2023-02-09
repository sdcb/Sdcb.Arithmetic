using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

public class GmpRational : IDisposable
{
    public readonly Mpq_t Raw = new();
    private readonly bool _isOwner;

    #region Initialization and Assignment Functions
    public unsafe GmpRational(bool isOwner = true)
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            GmpLib.__gmpq_init((IntPtr)ptr);
        }
        _isOwner = isOwner;
    }

    public GmpRational(Mpq_t raw, bool isOwner = true)
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
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_canonicalize((IntPtr)pthis);
        }
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
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpq_t* r = &op.Raw)
        {
            GmpLib.__gmpq_set((IntPtr)pthis, (IntPtr)r);
        }
    }

    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpz_t* r = &op.Raw)
        {
            GmpLib.__gmpq_set_z((IntPtr)pthis, (IntPtr)r);
        }
    }

    public unsafe void Assign(uint num, uint den)
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_ui((IntPtr)pthis, num, den);
        }
    }

    public unsafe void Assign(int num, uint den)
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_si((IntPtr)pthis, num, den);
        }
    }

    /// <summary>
    /// <para>Set rop from a null-terminated string str in the given base.</para>
    /// <para>The string can be an integer like “41” or a fraction like “41/152”. The fraction must be in canonical form (see Rational Number Functions), or if not then mpq_canonicalize must be called.</para>
    /// <para>The numerator and optional denominator are parsed the same as in mpz_set_str (see Assigning Integers). White space is allowed in the string, and is simply ignored. The base can vary from 2 to 62, or if base is 0 then the leading characters are used: 0x or 0X for hex, 0b or 0B for binary, 0 for octal, or decimal otherwise. Note that this is done separately for the numerator and denominator, so for instance 0xEF/100 is 239/100, whereas 0xEF/0x100 is 239/256.</para>
    /// </summary>
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
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public unsafe void Assign(double val)
    {
        fixed (Mpq_t* pthis = &Raw)
        {
            GmpLib.__gmpq_set_d((IntPtr)pthis, val);
        }
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public unsafe void Assign(GmpFloat val)
    {
        fixed (Mpq_t* pthis = &Raw)
        fixed (Mpf_t* pval = &val.Raw)
        {
            GmpLib.__gmpq_set_f((IntPtr)pthis, (IntPtr)pval);
        }
    }

    public static unsafe void Swap(GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_swap((IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* pthis = &Raw)
        {
            return GmpLib.__gmpq_get_d((IntPtr)pthis);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_add((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_mul((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpq_mul_2exp((IntPtr)pr, (IntPtr)p1, bitCount);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpq_div((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpq_div_2exp((IntPtr)pr, (IntPtr)p1, bitCount);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_neg((IntPtr)pr, (IntPtr)pop);
        }
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
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_abs((IntPtr)pr, (IntPtr)pop);
        }
    }

    public static GmpRational Abs(GmpRational op)
    {
        GmpRational rop = new();
        AbsInplace(rop, op);
        return rop;
    }

    public static unsafe void InvertInplace(GmpRational rop, GmpRational op)
    {
        fixed (Mpq_t* pr = &rop.Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpq_inv((IntPtr)pr, (IntPtr)pop);
        }
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
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_cmp((IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_cmp_z((IntPtr)p1, (IntPtr)p2);
        }
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
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpq_cmp_ui((IntPtr)p1, num, den);
        }
    }

    public static unsafe int Compare(GmpRational op1, int num, uint den)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpq_cmp_si((IntPtr)p1, num, den);
        }
    }

    public static unsafe bool Equals(GmpRational op1, GmpRational op2)
    {
        fixed (Mpq_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpq_equal((IntPtr)p1, (IntPtr)p2) != 0;
        }
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
        get => new GmpInteger(Raw.Num, isOwner: false);
        set
        {
            fixed (Mpq_t* pthis = &Raw)
            fixed (Mpz_t* pop = &value.Raw)
            {
                GmpLib.__gmpq_set_num((IntPtr)pthis, (IntPtr)pop);
            }
        }
    }

    public unsafe GmpInteger Den
    {
        get => new GmpInteger(Raw.Den, isOwner: false);
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

[StructLayout(LayoutKind.Sequential)]
public struct Mpq_t
{
    public Mpz_t Num, Den;

    public static int RawSize => Marshal.SizeOf<Mpq_t>();

    public override int GetHashCode() => HashCode.Combine(Num.GetHashCode(), Den.GetHashCode());
}