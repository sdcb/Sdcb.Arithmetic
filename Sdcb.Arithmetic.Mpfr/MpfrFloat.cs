using Sdcb.Arithmetic.Gmp;
using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

public unsafe class MpfrFloat : IDisposable
{
    // https://www.mpfr.org/mpfr-current/mpfr.html

    public readonly Mpfr_t Raw;

    #region Initialization Functions
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

    #region Compatibility With MPF
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