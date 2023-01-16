using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Math.Gmp
{
    public class BigFloat : IDisposable
    {
        public static int RawStructSize => Marshal.SizeOf<Mpf_t>();

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

        public unsafe BigFloat(uint precision)
        {
            fixed (Mpf_t* ptr = &Raw)
            {
                GmpNative.__gmpf_init2((IntPtr)ptr, precision);
            }
        }

        public unsafe static BigFloat From(int val)
        {
            BigFloat r = new();
            fixed (Mpf_t* ptr = &r.Raw)
            {
                GmpNative.__gmpf_init_set_si((IntPtr)ptr, val);
            }
            return r;
        }

        public unsafe static BigFloat From(uint val)
        {
            BigFloat r = new();
            fixed (Mpf_t* ptr = &r.Raw)
            {
                GmpNative.__gmpf_init_set_ui((IntPtr)ptr, val);
            }
            return r;
        }

        public unsafe static BigFloat From(double val)
        {
            BigFloat r = new();
            fixed (Mpf_t* ptr = &r.Raw)
            {
                GmpNative.__gmpf_init_set_d((IntPtr)ptr, val);
            }
            return r;
        }

        public unsafe static BigFloat Parse(string val, int valBase = 10)
        {
            BigFloat r = new();
            fixed (Mpf_t* ptr = &r.Raw)
            {
                byte[] valBytes = Encoding.UTF8.GetBytes(val);
                fixed (byte* pval = valBytes)
                {
                    int ret = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
                    if (ret != 0)
                    {
                        r.Clear();
                        throw new FormatException($"Failed to parse {val}, base={valBase} to BigFloat, __gmpf_init_set_str returns {ret}");
                    }
                }
            }
            return r;
        }

        public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out BigFloat result, int valBase = 10)
        {
            BigFloat r = new();
            fixed (Mpf_t* ptr = &r.Raw)
            {
                byte[] valBytes = Encoding.UTF8.GetBytes(val);
                fixed (byte* pval = valBytes)
                {
                    int rt = GmpNative.__gmpf_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
                    if (rt != 0)
                    {
                        r.Clear();
                        result = null;
                        return false;
                    }
                    else
                    {
                        result = r;
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
            fixed(Mpf_t* pthis = &Raw)
            fixed(Mpf_t* pthat = &op.Raw)
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
            {
                // TODO: GmpNative.__gmpf_set_z((IntPtr)pthis, op);
                throw new NotImplementedException();
            }
        }

        public unsafe void Assign(BigRational op)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                // TODO: GmpNative.__gmpf_set_q((IntPtr)pthis, op);
                throw new NotImplementedException();
            }
        }

        public unsafe void Assign(string op, int opBase = 10)
        {
            fixed (Mpf_t* pthis = &Raw)
            {
                byte[] opBytes = Encoding.UTF8.GetBytes(op);
                fixed(byte* opBytesPtr = opBytes)
                {
                    int ret = GmpNative.__gmpf_set_str((IntPtr)pthis, (IntPtr)opBytesPtr, opBase);
                    if (ret != 0)
                    {
                        throw new FormatException($"Failed to parse {op}, base={opBase} to BigFloat, __gmpf_set_str returns {ret}");
                    }
                }
            }
        }

        public unsafe static void Swap(BigFloat op1, BigFloat op2)
        {
            fixed (Mpf_t* pthis = &op1.Raw)
            fixed (Mpf_t* pthat = &op2.Raw)
            {
                GmpNative.__gmpf_swap((IntPtr)pthis, (IntPtr)pthat);
            }
        }
        #endregion

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
    }

    public struct Mpf_t
    {
        public int Precision;
        public int Size;
        public int Exponent;
        public IntPtr Limbs;
    }
}
