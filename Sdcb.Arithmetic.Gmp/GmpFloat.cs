using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

public class GmpFloat : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpLib.__gmpf_get_default_prec();
        set => GmpLib.__gmpf_set_default_prec(value);
    }

    public readonly Mpf_t Raw = new();
    private readonly bool _isOwner;

    #region Initialization functions

    public unsafe GmpFloat(bool isOwner = true)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_init((IntPtr)ptr);
        }
        _isOwner = isOwner;
    }

    public GmpFloat(Mpf_t raw, bool isOwner = true)
    {
        Raw = raw;
        _isOwner = isOwner;
    }

    public unsafe GmpFloat(uint precision, bool isOwner = true)
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
        _isOwner = isOwner;
    }
    #endregion

    #region Combined Initialization and Assignment Functions

    public unsafe static GmpFloat From(int val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_si((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public unsafe static GmpFloat From(int val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    public unsafe static GmpFloat From(uint val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_ui((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public unsafe static GmpFloat From(uint val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    public unsafe static GmpFloat From(double val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpLib.__gmpf_init_set_d((IntPtr)ptr, val);
        return new GmpFloat(raw);
    }

    public unsafe static GmpFloat From(double val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    public unsafe static GmpFloat From(GmpInteger val, uint precision)
    {
        GmpFloat f = new(precision);
        f.Assign(val);
        return f;
    }

    /// <summary>
    /// Convert BigInteger to BigFloat, precision default to abs(BigInteger.Raw.Size)
    /// </summary>
    public unsafe static GmpFloat From(GmpInteger val)
    {
        GmpFloat f = new(precision: (uint)Math.Abs(val.Raw.Size) * GmpLib.LimbBitSize);
        f.Assign(val);
        return f;
    }

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

    public static GmpFloat Parse(string val, uint precision, int @base = 10)
    {
        GmpFloat f = new(precision);
        f.Assign(val, @base);
        return f;
    }

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
    public unsafe void Assign(GmpFloat op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpf_t* pthat = &op.Raw)
        {
            GmpLib.__gmpf_set((IntPtr)pthis, (IntPtr)pthat);
        }
    }

    public unsafe void Assign(uint op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_ui((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(int op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_si((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(double op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpLib.__gmpf_set_d((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_z((IntPtr)pthis, (IntPtr)pop);
        }
    }

    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_set_q((IntPtr)pthis, (IntPtr)pop);
        }
    }

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
    public unsafe double ToDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_d((IntPtr)ptr);
        }
    }

    public static explicit operator double(GmpFloat op) => op.ToDouble();

    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            int exp;
            double val = GmpLib.__gmpf_get_d_2exp((IntPtr)ptr, (IntPtr)(&exp));
            return new ExpDouble(exp, val);
        }
    }

    public unsafe int ToInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_si((IntPtr)ptr);
        }
    }

    public static explicit operator int(GmpFloat op) => op.ToInt32();

    public unsafe uint ToUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_get_ui((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Set rop to the value of op. There is no rounding, this conversion is exact.
    /// </summary>
    public GmpRational ToGmpRational() => GmpRational.From(this);

    public static explicit operator uint(GmpFloat op) => op.ToUInt32();

    public unsafe override string? ToString() => ToString(@base: 10);

    public unsafe string? ToString(int @base = 10)
    {
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

                return ToString(ret, Sign, exp);
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

    internal static string ToString(IntPtr ret, int sign, int exp)
    {
        string s = Marshal.PtrToStringUTF8(ret)!.TrimEnd('0');

        string pre = sign == -1 ? "-" : "";
        s = sign == -1 ? s[1..] : s;

        return pre + (exp switch
        {
            > 0 => s.Length.CompareTo(exp) switch
            {
                > 0 => (s + new string('0', Math.Max(0, exp - s.Length + 1))) switch { var ss => ss[..exp] + "." + ss[exp..] },
                < 0 => s + new string('0', Math.Max(0, exp - s.Length)),
                0 => s
            },
            _ => s switch
            {
                "" => 0,
                _ => "0." + new string('0', -exp) + s
            }
        });
    }

    #endregion

    #region Arithmetic Functions
    #region Arithmetic Functions - Raw inplace functions
    public static unsafe void AddInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_add((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void AddInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_add_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void SubtractInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_sub((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void SubtractInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_sub_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void SubtractInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_sub((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    public static unsafe void MultiplyInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_mul((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void MultiplyInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void DivideInplace(GmpFloat rop, GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_div((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void DivideInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_div_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void DivideInplace(GmpFloat rop, uint op1, GmpFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpf_ui_div((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    public static unsafe void PowerInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_pow_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void NegateInplace(GmpFloat rop, GmpFloat op1)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_neg((IntPtr)prop, (IntPtr)pop1);
        }
    }

    public static unsafe void SqrtInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static unsafe void SqrtInplace(GmpFloat rop, uint op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        {
            GmpLib.__gmpf_sqrt_ui((IntPtr)prop, op);
        }
    }

    public static unsafe void AbsInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_abs((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// op1 * Math.Pow(2, op2)
    /// </summary>
    public static unsafe void Mul2ExpInplace(GmpFloat rop, GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpf_mul_2exp((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// op1 / Math.Pow(2, op2)
    /// </summary>
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

    public static unsafe GmpFloat Add(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Add(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Subtract(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Subtract(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Subtract(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Multiply(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Multiply(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Divide(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Divide(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Divide(uint op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Power(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        PowerInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe GmpFloat Negate(GmpFloat op1, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        NegateInplace(rop, op1);
        return rop;
    }

    public static unsafe GmpFloat Sqrt(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    public static unsafe GmpFloat Sqrt(uint op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    public static unsafe GmpFloat Abs(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        AbsInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// op1 * Math.Pow(2, op2)
    /// </summary>
    public static unsafe GmpFloat Mul2Exp(GmpFloat op1, uint op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        Mul2ExpInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// op1 / Math.Pow(2, op2)
    /// </summary>
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
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(GmpFloat op1, GmpFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            return GmpLib.__gmpf_cmp((IntPtr)pop1, (IntPtr)pop2);
        }
    }

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

    public override int GetHashCode() => Raw.GetHashCode();

    public static bool operator ==(GmpFloat left, GmpFloat right) => Compare(left, right) == 0;

    public static bool operator !=(GmpFloat left, GmpFloat right) => Compare(left, right) != 0;

    public static bool operator >(GmpFloat left, GmpFloat right) => Compare(left, right) > 0;

    public static bool operator <(GmpFloat left, GmpFloat right) => Compare(left, right) < 0;

    public static bool operator >=(GmpFloat left, GmpFloat right) => Compare(left, right) >= 0;

    public static bool operator <=(GmpFloat left, GmpFloat right) => Compare(left, right) <= 0;

    public static bool operator ==(GmpFloat left, double right) => Compare(left, right) == 0;

    public static bool operator !=(GmpFloat left, double right) => Compare(left, right) != 0;

    public static bool operator >(GmpFloat left, double right) => Compare(left, right) > 0;

    public static bool operator <(GmpFloat left, double right) => Compare(left, right) < 0;

    public static bool operator >=(GmpFloat left, double right) => Compare(left, right) >= 0;

    public static bool operator <=(GmpFloat left, double right) => Compare(left, right) <= 0;

    public static bool operator ==(GmpFloat left, int right) => Compare(left, right) == 0;

    public static bool operator !=(GmpFloat left, int right) => Compare(left, right) != 0;

    public static bool operator >(GmpFloat left, int right) => Compare(left, right) > 0;

    public static bool operator <(GmpFloat left, int right) => Compare(left, right) < 0;

    public static bool operator >=(GmpFloat left, int right) => Compare(left, right) >= 0;

    public static bool operator <=(GmpFloat left, int right) => Compare(left, right) <= 0;

    public static bool operator ==(GmpFloat left, uint right) => Compare(left, right) == 0;

    public static bool operator !=(GmpFloat left, uint right) => Compare(left, right) != 0;

    public static bool operator >(GmpFloat left, uint right) => Compare(left, right) > 0;

    public static bool operator <(GmpFloat left, uint right) => Compare(left, right) < 0;

    public static bool operator >=(GmpFloat left, uint right) => Compare(left, right) >= 0;

    public static bool operator <=(GmpFloat left, uint right) => Compare(left, right) <= 0;

    public static bool operator ==(GmpFloat left, GmpInteger right) => Compare(left, right) == 0;

    public static bool operator !=(GmpFloat left, GmpInteger right) => Compare(left, right) != 0;

    public static bool operator >(GmpFloat left, GmpInteger right) => Compare(left, right) > 0;

    public static bool operator <(GmpFloat left, GmpInteger right) => Compare(left, right) < 0;

    public static bool operator >=(GmpFloat left, GmpInteger right) => Compare(left, right) >= 0;

    public static bool operator <=(GmpFloat left, GmpInteger right) => Compare(left, right) <= 0;

    public static bool operator ==(double left, GmpFloat right) => right == left;

    public static bool operator !=(double left, GmpFloat right) => right != left;

    public static bool operator >(double left, GmpFloat right) => right < left;

    public static bool operator <(double left, GmpFloat right) => right > left;

    public static bool operator >=(double left, GmpFloat right) => right <= left;

    public static bool operator <=(double left, GmpFloat right) => right >= left;

    public static bool operator ==(int left, GmpFloat right) => right == left;

    public static bool operator !=(int left, GmpFloat right) => right != left;

    public static bool operator >(int left, GmpFloat right) => right < left;

    public static bool operator <(int left, GmpFloat right) => right > left;

    public static bool operator >=(int left, GmpFloat right) => right <= left;

    public static bool operator <=(int left, GmpFloat right) => right >= left;

    public static bool operator ==(uint left, GmpFloat right) => right == left;

    public static bool operator !=(uint left, GmpFloat right) => right != left;

    public static bool operator >(uint left, GmpFloat right) => right < left;

    public static bool operator <(uint left, GmpFloat right) => right > left;

    public static bool operator >=(uint left, GmpFloat right) => right <= left;

    public static bool operator <=(uint left, GmpFloat right) => right >= left;

    public static bool operator ==(GmpInteger left, GmpFloat right) => right == left;

    public static bool operator !=(GmpInteger left, GmpFloat right) => right != left;

    public static bool operator >(GmpInteger left, GmpFloat right) => right < left;

    public static bool operator <(GmpInteger left, GmpFloat right) => right > left;

    public static bool operator >=(GmpInteger left, GmpFloat right) => right <= left;

    public static bool operator <=(GmpInteger left, GmpFloat right) => right >= left;

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(GmpFloat op1, GmpInteger op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            return GmpLib.__gmpf_cmp_z((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(GmpFloat op1, double op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_d((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(GmpFloat op1, int op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_si((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Return non-zero if the first op3 bits of op1 and op2 are equal, zero otherwise. Note that numbers like e.g., 256 (binary 100000000) and 255 (binary 11111111) will never be equal by this function’s measure, and furthermore that 0 will only be equal to itself.
    /// </summary>
    [Obsolete("This function is mathematically ill-defined and should not be used.")]
    public static unsafe int MpfEquals(GmpFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpLib.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// rop = abs(op1-op2)/op1
    /// </summary>
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
    /// abs(op1-op2)/op1
    /// </summary>
    public static unsafe GmpFloat RelDiff(GmpFloat op1, GmpFloat op2, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2);
        return rop;
    }

    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;

    #endregion

    #region Misc Functions
    public static unsafe void CeilInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_ceil((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static GmpFloat Ceil(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        CeilInplace(rop, op);
        return rop;
    }

    public static unsafe void FloorInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_floor((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static GmpFloat Floor(GmpFloat op, uint precision = 0)
    {
        GmpFloat rop = new(precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// mpf_trunc, Set rop to op rounded to an integer, to the integer towards zero.
    /// </summary>
    public static unsafe void RoundInplace(GmpFloat rop, GmpFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpf_trunc((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// mpf_trunc, Set rop to op rounded to an integer, to the integer towards zero.
    /// </summary>
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

    public unsafe bool FitsInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sint_p((IntPtr)ptr) != 0;
        }
    }

    public unsafe bool FitsUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_uint_p((IntPtr)ptr) != 0;
        }
    }

    public unsafe bool FitsInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpLib.__gmpf_fits_sshort_p((IntPtr)ptr) != 0;
        }
    }

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
    private unsafe void Clear()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpLib.__gmpf_clear((IntPtr)ptr);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            if (_isOwner) Clear();
            _disposed = true;
        }
    }

    ~GmpFloat()
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

    #region Obsoleted Random
    /// <summary>
    /// Generate a random float of at most max_size limbs, 
    /// with long strings of zeros and ones in the binary representation. 
    /// The exponent of the number is in the interval -exp to exp (in limbs). 
    /// This function is useful for testing functions and algorithms, 
    /// since these kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static unsafe void Random2Inplace(GmpFloat rop, int maxLimbCount, int maxExp)
    {
        fixed (Mpf_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpf_random2((IntPtr)ptr, maxLimbCount, maxExp);
        }
    }

    /// <summary>
    /// Generate a random float of at most max_size limbs, 
    /// with long strings of zeros and ones in the binary representation. 
    /// The exponent of the number is in the interval -exp to exp (in limbs). 
    /// This function is useful for testing functions and algorithms, 
    /// since these kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static GmpFloat Random2(uint precision, int maxLimbCount, int maxExp)
    {
        GmpFloat rop = new(precision);
        Random2Inplace(rop, maxLimbCount, maxExp);
        return rop;
    }
    #endregion
}

[StructLayout(LayoutKind.Sequential)]
public record struct Mpf_t
{
    public int Precision;
    public int Size;
    public int Exponent;
    public IntPtr Limbs;

    public static int RawSize => Marshal.SizeOf<Mpf_t>();


    private unsafe Span<int> GetLimbData() => new((void*)Limbs, Precision - 1);

    public override int GetHashCode()
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

public record struct ExpDouble(int Exp, double Value);
