using Sdcb.Arithmetic.Gmp;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sdcb.Arithmetic.Mpfr;

public unsafe class MpfrFloat : IDisposable
{
    // https://www.mpfr.org/mpfr-current/mpfr.html

    public readonly Mpfr_t Raw;

    #region 1. Initialization Functions
    /// <summary>
    /// Initialize, set its precision to be exactly prec bits and its value to NaN.
    /// (Warning: the corresponding MPF function initializes to zero instead.)
    /// </summary>
    /// <param name="precision"></param>
    public MpfrFloat(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init2((IntPtr)ptr, precision);
        }
    }

    /// <summary>
    /// Initialize, set its precision to the default precision, and set its value to NaN.
    /// The default precision can be changed by a call to mpfr_set_default_prec.
    /// </summary>
    public MpfrFloat()
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init((IntPtr)ptr);
        }
    }

    /// <summary>
    /// The current default MPFR precision in bits.
    /// </summary>
    public static int DefaultPrecision
    {
        get => MpfrLib.mpfr_get_default_prec();
        set => MpfrLib.mpfr_set_default_prec(value);
    }

    private static readonly ThreadLocal<MpfrRounding> _defaultRounding = new(GetRawRounding); /* should be MpfrRounding.Nearest */
    public static MpfrRounding DefaultRounding
    {
        get => _defaultRounding.Value;
        set
        {
            _defaultRounding.Value = value;
            MpfrLib.mpfr_set_default_rounding_mode(value);
        }
    }

    private static MpfrRounding GetRawRounding() => MpfrLib.mpfr_get_default_rounding_mode();

    /// <summary>
    /// The number of bits used to store its significand.
    /// </summary>
    public int Precision
    {
        get
        {
            fixed (Mpfr_t* ptr = &Raw)
            {
                return MpfrLib.mpfr_get_prec((IntPtr)ptr);
            }
        }
        set
        {
            fixed (Mpfr_t* ptr = &Raw)
            {
                MpfrLib.mpfr_set_prec((IntPtr)ptr, value);
            }
        }
    }
    #endregion

    #region 2. Assignment Functions
    public void Assign(MpfrFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* pval = &val.Raw)
        {
            MpfrLib.mpfr_set((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public void Assign(uint val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_ui((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public void Assign(int val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_si((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public void Assign(float val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_flt((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public void Assign(double val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_d((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public void Assign(GmpInteger val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pval = &val.Raw)
        {
            MpfrLib.mpfr_set_z((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public void Assign(GmpRational val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpq_t* pval = &val.Raw)
        {
            MpfrLib.mpfr_set_q((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public void Assign(GmpFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpf_t* pval = &val.Raw)
        {
            MpfrLib.mpfr_set_f((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public void Assign2Exp(uint op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_ui_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public void Assign2Exp(int op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_si_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public void Assign2Exp(GmpInteger op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            MpfrLib.mpfr_set_z_2exp((IntPtr)pthis, (IntPtr)pop, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set rop to the value of the string s in base base, rounded in the direction rnd. 
    /// </summary>
    public void Assign(string s, int @base = 0, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(s);
            fixed (byte* opPtr = opBytes)
            {
                IntPtr endptr = default;
                int ret = MpfrLib.mpfr_strtofr((IntPtr)pthis, (IntPtr)opPtr, (IntPtr)(&endptr), @base, rounding ?? DefaultRounding);
                if (endptr != default)
                {
                    string location = Marshal.PtrToStringUTF8(endptr);
                    throw new FormatException($"Failed to parse \"{s}\", base={@base} to {nameof(MpfrFloat)}, mpfr_strtofr returns {ret} at: {location}");
                }
            }
        }
    }

    /// <summary>
    /// Set to the value of the string s in base base, rounded in the direction rnd. 
    /// </summary>
    public bool TryAssign(string s, int @base = 0, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(s);
            fixed (byte* opPtr = opBytes)
            {
                IntPtr endptr = default;
                int ret = MpfrLib.mpfr_strtofr((IntPtr)pthis, (IntPtr)opPtr, (IntPtr)(&endptr), @base, rounding ?? DefaultRounding);
                return endptr == default;
            }
        }
    }

    /// <summary>
    /// set to NaN(not-a-number)
    /// </summary>
    public void AssignNaN()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_nan((IntPtr)pthis);
        }
    }

    /// <summary>
    /// set to +inf if sign >= 0, otherwise -inf
    /// </summary>
    public void AssignInfinity(int sign)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_inf((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// set to +0 if sign >= 0, otherwise -0
    /// </summary>
    /// <param name="sign"></param>
    public void AssignZero(int sign = 0)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_zero((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// Swap the structures pointed to by x and y.
    /// In particular, the values are exchanged without rounding
    /// (this may be different from three mpfr_set calls using a third auxiliary variable).
    /// </summary>
    public static void Swap(MpfrFloat x, MpfrFloat y)
    {
        fixed (Mpfr_t* p1 = &x.Raw)
        fixed (Mpfr_t* p2 = &y.Raw)
        {
            MpfrLib.mpfr_swap((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Swap the structures pointed to by this and op.
    /// In particular, the values are exchanged without rounding
    /// (this may be different from three mpfr_set calls using a third auxiliary variable).
    /// </summary>
    public void Swap(MpfrFloat op) => Swap(this, op);
    #endregion

    #region 4. Conversion Functions
    public double ToDouble(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_d((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator double(MpfrFloat op) => op.ToDouble();

    public float ToFloat(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_flt((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator float(MpfrFloat op) => op.ToFloat();

    public override string ToString() => ToString(10);

    public string ToString(int @base = 10, MpfrRounding? rounding = null)
    {
        const nint srcptr = 0;
        const int digits = 0;
        fixed (Mpfr_t* pthis = &Raw)
        {
            int exp;
            IntPtr ret = default;
            try
            {
                ret = MpfrLib.mpfr_get_str(srcptr, (IntPtr)(&exp), @base, digits, (IntPtr)pthis, rounding ?? DefaultRounding);
                if (ret == IntPtr.Zero)
                {
                    throw new ArgumentException($"Unable to convert BigInteger to string.");
                }

                return GmpFloat.ToString(ret, Raw.Sign, exp);
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

    #region 15. Compatibility With MPF
    /// <summary>
    /// Reset the precision of x to be exactly prec bits.
    /// The only difference with mpfr_set_prec is that prec 
    /// is assumed to be small enough so that the 
    /// significand fits into the current allocated memory 
    /// space for x. 
    /// Otherwise the behavior is undefined.
    /// </summary>
    public void SetRawPrecision(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_set_prec_raw((IntPtr)ptr, precision);
        }
    }

    /// <summary>
    /// Return non-zero if op1 and op2 are both non-zero ordinary 
    /// numbers with the same exponent and the same first op3 bits,
    /// both zero, or both infinities of the same sign. Return 
    /// zero otherwise. 
    /// This function is defined for compatibility with MPF, we do 
    /// not recommend to use it otherwise. 
    /// Do not use it either if you want to know whether two numbers 
    /// are close to each other; for instance, 1.011111 and 1.100000 
    /// are regarded as different for any value of op3 larger than 1.
    /// </summary>
    public int MpfEquals(MpfrFloat op1, MpfrFloat op2, uint op3)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_eq((IntPtr)p1, (IntPtr)p2, op3);
        }
    }

    /// <summary>
    /// <para>rop = abs(op1-op2)/op1</para>
    /// <para>
    /// Compute the relative difference between op1 and op2 and store the result in rop.
    /// This function does not guarantee the correct rounding on the relative difference;
    /// it just computes |op1 - op2| / op1</para>
    /// </summary>
    public static unsafe void RelDiffInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            MpfrLib.mpfr_reldiff((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding);
        }
    }

    /// <summary>
    /// <para>
    /// Compute the relative difference between op1 and op2 and store the result in rop.
    /// This function does not guarantee the correct rounding on the relative difference;
    /// it just computes |op1 - op2| / op1</para>
    /// </summary>
    /// <param name="precision">the result precision</param>
    /// <returns>abs(op1-op2)/op1</returns>
    public static MpfrFloat RelDiff(MpfrFloat op1, MpfrFloat op2, MpfrRounding rounding, int precision = 0)
    {
        MpfrFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static void Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_mul_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static void Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_div_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static MpfrFloat Multiply2Exp(MpfrFloat op1, uint op2, MpfrRounding rounding, int precision)
    {
        MpfrFloat rop = new (precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static MpfrFloat Divide2Exp(MpfrFloat op1, uint op2, MpfrRounding rounding, int precision)
    {
        MpfrFloat rop = new(precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }
    #endregion

    #region Clear & Dispose
    private bool _disposed;

    private void Clear()
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_clear((IntPtr)ptr);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            Clear();
            _disposed = true;
        }
    }

    ~MpfrFloat()
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

[StructLayout(LayoutKind.Sequential)]
public record struct Mpfr_t
{
    public int Precision;
    public int Sign;
    public int Exponent;
    public IntPtr Limbs;

    public static int RawSize => Marshal.SizeOf<Mpf_t>();

    private unsafe Span<int> GetLimbData() => new((void*)Limbs, Precision - 1);

    public override int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Sign);
        c.Add(Exponent);
        foreach (int i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}