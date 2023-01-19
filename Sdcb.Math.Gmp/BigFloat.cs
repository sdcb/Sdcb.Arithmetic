using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using SysMath = System.Math;

namespace Sdcb.Math.Gmp;

public class BigFloat : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpNative.__gmpf_get_default_prec();
        set => GmpNative.__gmpf_set_default_prec(value);
    }

    public Mpf_t Raw = new();
    private bool _disposed = false;

    #region Initialization functions

    public unsafe BigFloat()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpNative.__gmpf_init((IntPtr)ptr);
        }
    }

    public BigFloat(Mpf_t raw)
    {
        Raw = raw;
    }

    public unsafe BigFloat(uint precision)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            if (precision == 0)
            {
                GmpNative.__gmpf_init((IntPtr)ptr);
            }
            else
            {
                GmpNative.__gmpf_init2((IntPtr)ptr, precision);
            }
        }
    }
    #endregion

    #region Combined Initialization and Assignment Functions

    public unsafe static BigFloat From(int val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpNative.__gmpf_init_set_si((IntPtr)ptr, val);
        return new BigFloat(raw);
    }

    public unsafe static BigFloat From(uint val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpNative.__gmpf_init_set_ui((IntPtr)ptr, val);
        return new BigFloat(raw);
    }

    public unsafe static BigFloat From(double val)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        GmpNative.__gmpf_init_set_d((IntPtr)ptr, val);
        return new BigFloat(raw);
    }

    public unsafe static BigFloat Parse(string val, int valBase = 10)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
            if (ret != 0)
            {
                GmpNative.__gmpf_clear((IntPtr)ptr);
                throw new FormatException($"Failed to parse {val}, base={valBase} to BigFloat, __gmpf_init_set_str returns {ret}");
            }
        }
        return new BigFloat(raw);
    }

    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out BigFloat result, int valBase = 10)
    {
        Mpf_t raw = new();
        Mpf_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
            if (rt != 0)
            {
                GmpNative.__gmpf_clear((IntPtr)ptr);
                result = null;
                return false;
            }
            else
            {
                result = new BigFloat(raw);
                return true;
            }
        }
    }

    public unsafe uint Precision
    {
        get
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpNative.__gmpf_get_prec((IntPtr)ptr);
            }
        }
        set
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpNative.__gmpf_set_prec((IntPtr)ptr, value);
            }
        }
    }

    [Obsolete("use Precision")]
    public unsafe void SetRawPrecision(uint value)
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpNative.__gmpf_set_prec_raw((IntPtr)ptr, value);
        }
    }
    #endregion

    #region Assignment functions
    public unsafe void Assign(BigFloat op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpf_t* pthat = &op.Raw)
        {
            GmpNative.__gmpf_set((IntPtr)pthis, (IntPtr)pthat);
        }
    }

    public unsafe void Assign(uint op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpNative.__gmpf_set_ui((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(int op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpNative.__gmpf_set_si((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(double op)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            GmpNative.__gmpf_set_d((IntPtr)pthis, op);
        }
    }

    public unsafe void Assign(BigInteger op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_set_z((IntPtr)pthis, (IntPtr)pop);
        }
    }

    public unsafe void Assign(BigRational op)
    {
        fixed (Mpf_t* pthis = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_set_q((IntPtr)pthis, (IntPtr)pop);
        }
    }

    public unsafe void Assign(string op, int opBase = 10)
    {
        fixed (Mpf_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(op);
            fixed (byte* opBytesPtr = opBytes)
            {
                int ret = GmpNative.__gmpf_set_str((IntPtr)pthis, (IntPtr)opBytesPtr, opBase);
                if (ret != 0)
                {
                    throw new FormatException($"Failed to parse \"{op}\", base={opBase} to BigFloat, __gmpf_set_str returns {ret}");
                }
            }
        }
    }

    public unsafe static void Swap(BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_swap((IntPtr)pop1, (IntPtr)pop2);
        }
    }
    #endregion

    #region Conversion Functions
    public unsafe double ToDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_get_d((IntPtr)ptr);
        }
    }

    public static explicit operator double(BigFloat op) => op.ToDouble();

    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            int exp;
            double val = GmpNative.__gmpf_get_d_2exp((IntPtr)ptr, (IntPtr)(&exp));
            return new ExpDouble(exp, val);
        }
    }

    public unsafe int ToInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_get_si((IntPtr)ptr);
        }
    }

    public static explicit operator int(BigFloat op) => op.ToInt32();

    public unsafe uint ToUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_get_ui((IntPtr)ptr);
        }
    }

    public static explicit operator uint(BigFloat op) => op.ToUInt32();

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
                ret = GmpNative.__gmpf_get_str(srcptr, (IntPtr)(&exp), @base, digits, (IntPtr)ptr);
                if (ret == IntPtr.Zero)
                {
                    throw new ArgumentException($"Unable to convert BigInteger to string.");
                }

                string s = Marshal.PtrToStringUTF8(ret)!;

                int sign = Sign;
                string pre = sign == -1 ? "-" : "";
                s = sign == -1 ? s[1..] : s;

                return pre + (exp switch
                {
                    > 0 => (s.Length == exp) switch
                    {
                        false => (s + new string('0', SysMath.Max(0, exp - s.Length + 1))) switch { var ss => ss[..exp] + "." + ss[exp..] },
                        true => s
                    },
                    _ => s switch
                    {
                        "" => 0, 
                        _ => "0." + new string('0', -exp) + s
                    }
                });
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

    #endregion

    #region Arithmetic Functions
    #region Arithmetic Functions - Raw inplace functions
    public static unsafe void AddInplace(BigFloat rop, BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_add((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void AddInplace(BigFloat rop, BigFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_add_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void SubtractInplace(BigFloat rop, BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_sub((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void SubtractInplace(BigFloat rop, BigFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_sub_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void SubtractInplace(BigFloat rop, uint op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_ui_sub((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    public static unsafe void MultiplyInplace(BigFloat rop, BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_mul((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void MultiplyInplace(BigFloat rop, BigFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_mul_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void DivideInplace(BigFloat rop, BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_div((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static unsafe void DivideInplace(BigFloat rop, BigFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_div_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void DivideInplace(BigFloat rop, uint op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_ui_div((IntPtr)prop, op1, (IntPtr)pop2);
        }
    }

    public static unsafe void PowerInplace(BigFloat rop, BigFloat op1, uint op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_pow_ui((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void NegateInplace(BigFloat rop, BigFloat op1)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_neg((IntPtr)prop, (IntPtr)pop1);
        }
    }

    public static unsafe void SqrtInplace(BigFloat rop, BigFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static unsafe void SqrtInplace(BigFloat rop, uint op, uint precision = 0)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        {
            GmpNative.__gmpf_sqrt_ui((IntPtr)prop, op);
        }
    }

    public static unsafe void AbsInplace(BigFloat rop, BigFloat op, uint precision = 0)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_abs((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// op1 * Math.Pow(2, op2)
    /// </summary>
    public static unsafe void Mul2ExpInplace(BigFloat rop, BigFloat op1, uint op2, uint precision = 0)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_mul_2exp((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// op1 / Math.Pow(2, op2)
    /// </summary>
    public static unsafe void Div2ExpInplace(BigFloat rop, BigFloat op1, uint op2, uint precision = 0)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpf_div_2exp((IntPtr)prop, (IntPtr)pop1, op2);
        }
    }

    #endregion

    #region Arithmetic Functions - Easier functions

    public static unsafe BigFloat Add(BigFloat op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Add(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        AddInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Subtract(BigFloat op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Subtract(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Subtract(uint op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        SubtractInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Multiple(BigFloat op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Multiple(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        MultiplyInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Divide(BigFloat op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Divide(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Divide(uint op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        DivideInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Power(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        PowerInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe BigFloat Negate(BigFloat op1, uint precision = 0)
    {
        BigFloat rop = new(precision);
        NegateInplace(rop, op1);
        return rop;
    }

    public static unsafe BigFloat Sqrt(BigFloat op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    public static unsafe BigFloat Sqrt(uint op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        SqrtInplace(rop, op);
        return rop;
    }

    public static unsafe BigFloat Abs(BigFloat op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        AbsInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// op1 * Math.Pow(2, op2)
    /// </summary>
    public static unsafe BigFloat Mul2Exp(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        Mul2ExpInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// op1 / Math.Pow(2, op2)
    /// </summary>
    public static unsafe BigFloat Div2Exp(BigFloat op1, uint op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        Div2ExpInplace(rop, op1, op2);
        return rop;
    }
    #endregion

    #region Arithmetic Functions - Operators
    public static unsafe BigFloat operator +(BigFloat op1, BigFloat op2) => Add(op1, op2);

    public static unsafe BigFloat operator +(BigFloat op1, uint op2) => Add(op1, op2);

    public static unsafe BigFloat operator -(BigFloat op1, BigFloat op2) => Subtract(op1, op2);

    public static unsafe BigFloat operator -(BigFloat op1, uint op2) => Subtract(op1, op2);

    public static unsafe BigFloat operator -(uint op1, BigFloat op2) => Subtract(op1, op2);

    public static unsafe BigFloat operator *(BigFloat op1, BigFloat op2) => Multiple(op1, op2);

    public static unsafe BigFloat operator *(BigFloat op1, uint op2) => Multiple(op1, op2);

    public static unsafe BigFloat operator /(BigFloat op1, BigFloat op2) => Divide(op1, op2);

    public static unsafe BigFloat operator /(BigFloat op1, uint op2) => Divide(op1, op2);

    public static unsafe BigFloat operator /(uint op1, BigFloat op2) => Divide(op1, op2);

    public static unsafe BigFloat operator ^(BigFloat op1, uint op2) => Power(op1, op2);

    public static unsafe BigFloat operator -(BigFloat op1) => Negate(op1);

    #endregion
    #endregion

    #region Comparison Functions
    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            return GmpNative.__gmpf_cmp((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        BigFloat bf => Compare(this, bf) == 0,
        BigInteger bi => Compare(this, bi) == 0,
        double d => Compare(this, d) == 0,
        int i => Compare(this, i) == 0,
        uint ui => Compare(this, ui) == 0,
        _ => false
    };

    public override int GetHashCode() => Raw.GetHashCode();

    public static bool operator ==(BigFloat left, BigFloat right) => Compare(left, right) == 0;

    public static bool operator !=(BigFloat left, BigFloat right) => Compare(left, right) != 0;

    public static bool operator >(BigFloat left, BigFloat right) => Compare(left, right) > 0;

    public static bool operator <(BigFloat left, BigFloat right) => Compare(left, right) < 0;

    public static bool operator >=(BigFloat left, BigFloat right) => Compare(left, right) >= 0;

    public static bool operator <=(BigFloat left, BigFloat right) => Compare(left, right) <= 0;

    public static bool operator ==(BigFloat left, double right) => Compare(left, right) == 0;

    public static bool operator !=(BigFloat left, double right) => Compare(left, right) != 0;

    public static bool operator >(BigFloat left, double right) => Compare(left, right) > 0;

    public static bool operator <(BigFloat left, double right) => Compare(left, right) < 0;

    public static bool operator >=(BigFloat left, double right) => Compare(left, right) >= 0;

    public static bool operator <=(BigFloat left, double right) => Compare(left, right) <= 0;

    public static bool operator ==(BigFloat left, int right) => Compare(left, right) == 0;

    public static bool operator !=(BigFloat left, int right) => Compare(left, right) != 0;

    public static bool operator >(BigFloat left, int right) => Compare(left, right) > 0;

    public static bool operator <(BigFloat left, int right) => Compare(left, right) < 0;

    public static bool operator >=(BigFloat left, int right) => Compare(left, right) >= 0;

    public static bool operator <=(BigFloat left, int right) => Compare(left, right) <= 0;

    public static bool operator ==(BigFloat left, uint right) => Compare(left, right) == 0;

    public static bool operator !=(BigFloat left, uint right) => Compare(left, right) != 0;

    public static bool operator >(BigFloat left, uint right) => Compare(left, right) > 0;

    public static bool operator <(BigFloat left, uint right) => Compare(left, right) < 0;

    public static bool operator >=(BigFloat left, uint right) => Compare(left, right) >= 0;

    public static bool operator <=(BigFloat left, uint right) => Compare(left, right) <= 0;

    public static bool operator ==(BigFloat left, BigInteger right) => Compare(left, right) == 0;

    public static bool operator !=(BigFloat left, BigInteger right) => Compare(left, right) != 0;

    public static bool operator >(BigFloat left, BigInteger right) => Compare(left, right) > 0;

    public static bool operator <(BigFloat left, BigInteger right) => Compare(left, right) < 0;

    public static bool operator >=(BigFloat left, BigInteger right) => Compare(left, right) >= 0;

    public static bool operator <=(BigFloat left, BigInteger right) => Compare(left, right) <= 0;

    public static bool operator ==(double left, BigFloat right) => right == left;

    public static bool operator !=(double left, BigFloat right) => right != left;

    public static bool operator >(double left, BigFloat right) => right < left;

    public static bool operator <(double left, BigFloat right) => right > left;

    public static bool operator >=(double left, BigFloat right) => right <= left;

    public static bool operator <=(double left, BigFloat right) => right >= left;

    public static bool operator ==(int left, BigFloat right) => right == left;

    public static bool operator !=(int left, BigFloat right) => right != left;

    public static bool operator >(int left, BigFloat right) => right < left;

    public static bool operator <(int left, BigFloat right) => right > left;

    public static bool operator >=(int left, BigFloat right) => right <= left;

    public static bool operator <=(int left, BigFloat right) => right >= left;

    public static bool operator ==(uint left, BigFloat right) => right == left;

    public static bool operator !=(uint left, BigFloat right) => right != left;

    public static bool operator >(uint left, BigFloat right) => right < left;

    public static bool operator <(uint left, BigFloat right) => right > left;

    public static bool operator >=(uint left, BigFloat right) => right <= left;

    public static bool operator <=(uint left, BigFloat right) => right >= left;

    public static bool operator ==(BigInteger left, BigFloat right) => right == left;

    public static bool operator !=(BigInteger left, BigFloat right) => right != left;

    public static bool operator >(BigInteger left, BigFloat right) => right < left;

    public static bool operator <(BigInteger left, BigFloat right) => right > left;

    public static bool operator >=(BigInteger left, BigFloat right) => right <= left;

    public static bool operator <=(BigInteger left, BigFloat right) => right >= left;

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(BigFloat op1, BigInteger op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            return GmpNative.__gmpf_cmp_z((IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(BigFloat op1, double op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpNative.__gmpf_cmp_d((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(BigFloat op1, int op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpNative.__gmpf_cmp_si((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Compare op1 and op2. Return a positive value if op1 > op2, zero if op1 = op2, and a negative value if op1 < op2.
    /// </summary>
    public static unsafe int Compare(BigFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpNative.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// Return non-zero if the first op3 bits of op1 and op2 are equal, zero otherwise. Note that numbers like e.g., 256 (binary 100000000) and 255 (binary 11111111) will never be equal by this function’s measure, and furthermore that 0 will only be equal to itself.
    /// </summary>
    [Obsolete("This function is mathematically ill-defined and should not be used.")]
    public static unsafe int MpfEquals(BigFloat op1, uint op2)
    {
        fixed (Mpf_t* pop1 = &op1.Raw)
        {
            return GmpNative.__gmpf_cmp_ui((IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// rop = abs(op1-op2)/op1
    /// </summary>
    public static unsafe void RelDiffInplace(BigFloat rop, BigFloat op1, BigFloat op2)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop1 = &op1.Raw)
        fixed (Mpf_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpf_reldiff((IntPtr)prop, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// abs(op1-op2)/op1
    /// </summary>
    public static unsafe BigFloat RelDiff(BigFloat op1, BigFloat op2, uint precision = 0)
    {
        BigFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2);
        return rop;
    }

    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;

    #endregion

    #region Misc Functions
    public static unsafe void CeilInplace(BigFloat rop, BigFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_ceil((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static BigFloat Ceil(BigFloat op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        CeilInplace(rop, op);
        return rop;
    }

    public static unsafe void FloorInplace(BigFloat rop, BigFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_floor((IntPtr)prop, (IntPtr)pop);
        }
    }

    public static BigFloat Floor(BigFloat op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// mpf_trunc, Set rop to op rounded to an integer, to the integer towards zero.
    /// </summary>
    public static unsafe void RoundInplace(BigFloat rop, BigFloat op)
    {
        fixed (Mpf_t* prop = &rop.Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpf_trunc((IntPtr)prop, (IntPtr)pop);
        }
    }

    /// <summary>
    /// mpf_trunc, Set rop to op rounded to an integer, to the integer towards zero.
    /// </summary>
    public static BigFloat Round(BigFloat op, uint precision = 0)
    {
        BigFloat rop = new(precision);
        RoundInplace(rop, op);
        return rop;
    }

    public unsafe bool IsInteger
    {
        get
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                return GmpNative.__gmpf_integer_p((IntPtr)ptr) != 0;
            }
        }
    }

    public unsafe bool FitsInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_fits_sint_p((IntPtr)ptr) != 0;
        }
    }

    public unsafe bool FitsUInt32()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_fits_uint_p((IntPtr)ptr) != 0;
        }
    }

    public unsafe bool FitsInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_fits_sshort_p((IntPtr)ptr) != 0;
        }
    }

    public unsafe bool FitsUInt16()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            return GmpNative.__gmpf_fits_ushort_p((IntPtr)ptr) != 0;
        }
    }
    #endregion

    #region Dispose and Clear
    private unsafe void Clear()
    {
        fixed (Mpf_t* ptr = &Raw)
        {
            GmpNative.__gmpf_clear((IntPtr)ptr);
        }
    }

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

    ~BigFloat()
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
}

public record struct Mpf_t
{
    public int Precision;
    public int Size;
    public int Exponent;
    public IntPtr Limbs;

    public static int RawSize => Marshal.SizeOf<Mpf_t>();

    private unsafe Span<int> GetLimbData() => new Span<int>((void*)Limbs, Precision - 1);

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
