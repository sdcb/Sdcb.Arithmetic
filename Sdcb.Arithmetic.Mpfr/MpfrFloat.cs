using Sdcb.Arithmetic.Gmp;
using System;
using System.Runtime.InteropServices;
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

    private static ThreadLocal<MpfrRounding> _defaultRounding = new ThreadLocal<MpfrRounding>(GetRawRounding); /* should be MpfrRounding.Nearest */
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
    public void Assign(MpfrFloat val, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* pval = &val.Raw)
        {
            MpfrLib.mpfr_set((IntPtr)pthis, (IntPtr)pval, rounding);
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