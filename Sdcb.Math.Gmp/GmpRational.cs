using System;
using System.Runtime.InteropServices;

namespace Sdcb.Math.Gmp;

public class GmpRational : IDisposable
{
    public Mpq_t Raw = new();
    private bool _disposed = false;

    public unsafe GmpRational()
    {
        fixed (Mpq_t* ptr = &Raw)
        {
            GmpLib.__gmpq_init((IntPtr)ptr);
        }
    }

    public static unsafe GmpRational From(int num, uint den)
    {
        GmpRational r = new();
        fixed (Mpq_t* pr = &r.Raw)
        {
            GmpLib.__gmpq_set_si((IntPtr)pr, num, den);
        }
        return r;
    }

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
}

public struct Mpq_t
{
    public Mpz_t Num, Den;

    public static int RawSize => Marshal.SizeOf<Mpq_t>();

    public override int GetHashCode() => HashCode.Combine(Num.GetHashCode(), Den.GetHashCode());
}