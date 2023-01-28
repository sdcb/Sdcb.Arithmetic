using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Math.Gmp;

public class GmpRational : IDisposable
{
    public Mpq_t Raw = new();
    private bool _disposed = false;

    #region Initialization and Assignment Functions
    public unsafe GmpRational()
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            GmpLib.__gmpq_init((IntPtr)ptr);
        }
    }

    public GmpRational(Mpq_t raw)
    {
        Raw = raw;
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

            Clear();
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
    // https://gmplib.org/manual/Rational-Arithmetic
    #endregion
}

public struct Mpq_t
{
    public Mpz_t Num, Den;

    public static int RawSize => Marshal.SizeOf<Mpq_t>();

    public override int GetHashCode() => HashCode.Combine(Num.GetHashCode(), Den.GetHashCode());
}