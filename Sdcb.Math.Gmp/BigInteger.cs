using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sdcb.Math.Gmp;

public class BigInteger : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpNative.__gmpf_get_default_prec();
        set => GmpNative.__gmpf_set_default_prec(value);
    }

    public Mpz_t Raw = new();
    private bool _disposed = false;

    #region Initializing Integers
    public unsafe BigInteger()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_init((IntPtr)ptr);
        }
    }

    public unsafe BigInteger(Mpz_t raw)
    {
        Raw = raw;
    }

    public unsafe BigInteger(uint bitCount)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_init2((IntPtr)ptr, bitCount);
        }
    }

    /// <summary>
    /// Change the space for integer to new_alloc limbs. The value in integer is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByLimbs(int limbs)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_realloc((IntPtr)ptr, limbs);
        }
    }

    /// <summary>
    /// Change the space allocated for x to n bits. The value in x is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByBits(uint bits)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_realloc2((IntPtr)ptr, bits);
        }
    }
    #endregion

    #region Assignment Functions
    public unsafe void Assign(BigInteger op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(uint op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_ui((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(int op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_si((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(double op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_d((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(BigRational op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set_q((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(BigFloat op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set_f((IntPtr)ptr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe void Assign(string op, int opBase = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(op);
            fixed (byte* opPtr = opBytes)
            {
                int ret = GmpNative.__gmpz_set_str((IntPtr)ptr, (IntPtr)opPtr, opBase);
                if (ret != 0)
                {
                    throw new FormatException($"Failed to parse \"{op}\", base={opBase} to BigInteger, __gmpz_set_str returns {ret}");
                }
            }
        }
    }

    public static unsafe void Swap(BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_swap((IntPtr)pop1, (IntPtr)pop2);
        }
    }
    #endregion

    #region Combined Initialization and Assignment Functions
    public static unsafe BigInteger From(BigInteger op)
    {
        Mpz_t raw = new();
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_init_set((IntPtr)(&raw), (IntPtr)pop);
        }
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(uint op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_ui((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(int op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_si((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(double op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_d((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static BigInteger Parse(string val, int valBase = 0)
    {
        Mpz_t raw = new();
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpNative.__gmpz_init_set_str((IntPtr)(&raw), (IntPtr)pval, valBase);
            if (ret != 0)
            {
                GmpNative.__gmpz_clear((IntPtr)(&raw));
                throw new FormatException($"Failed to parse {val}, base={valBase} to BigInteger, __gmpf_init_set_str returns {ret}");
            }
        }
        return new BigInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out BigInteger result, int valBase = 10)
    {
        Mpz_t raw = new();
        Mpz_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpNative.__gmpz_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
            if (rt != 0)
            {
                GmpNative.__gmpz_clear((IntPtr)ptr);
                result = null;
                return false;
            }
            else
            {
                result = new BigInteger(raw);
                return true;
            }
        }
    }
    #endregion

    #region Dispose and Clear

    private unsafe void Clear()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_clear((IntPtr)ptr);
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

    ~BigInteger()
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

    #region Conversion Functions
    public unsafe uint ToUInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_ui((IntPtr)ptr);
        }
    }

    public unsafe int ToInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_si((IntPtr)ptr);
        }
    }

    public unsafe double ToDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_d((IntPtr)ptr);
        }
    }

    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            int exp = default;
            double val = GmpNative.__gmpz_get_d_2exp((IntPtr)(&exp), (IntPtr)ptr);
            return new ExpDouble(exp, val);
        }
    }

    public unsafe override string ToString()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            IntPtr ret = GmpNative.__gmpz_get_str(IntPtr.Zero, 10, (IntPtr)ptr);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert BigInteger to string.");
            }

            return Marshal.PtrToStringUTF8(ret)!;
        }
    }
    #endregion
}

public record struct Mpz_t
{
    public int AllocatedCount;
    public int Size;
    public IntPtr Limbs;

    public static int RawSize => Marshal.SizeOf<Mpz_t>();

    private unsafe Span<nint> GetLimbData() => new Span<nint>((void*)Limbs, AllocatedCount);

    public override int GetHashCode()
    {
        HashCode c = new();
        c.Add(AllocatedCount);
        c.Add(Size);
        foreach (nint i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}