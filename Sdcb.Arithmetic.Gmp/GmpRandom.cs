using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Sdcb.Arithmetic.Gmp;

public class GmpRandom : IDisposable
{
    private IntPtr Raw;

    #region Random State Initialization

    public GmpRandom(IntPtr raw)
    {
        Raw = raw;
        SetRandomSeed();
    }

    private GmpRandom(IntPtr raw, uint seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    private GmpRandom(IntPtr raw, GmpInteger seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    public GmpRandom()
    {
        Raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(Raw);
        SetRandomSeed();
    }

    public GmpRandom(uint seed)
    {
        Raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(Raw);
        SetSeed(seed);
    }

    public GmpRandom(GmpInteger seed)
    {
        Raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(Raw);
        SetSeed(seed);
    }

    /// <returns>the random seed</returns>
    public unsafe uint SetRandomSeed()
    {
        uint seed = default;
        RandomNumberGenerator.Fill(new Span<byte>(&seed, 4));
        SetSeed(seed);
        return seed;
    }

    public void SetSeed(uint seed)
    {
        GmpLib.__gmp_randseed_ui(Raw, (uint)seed);
    }

    public void SetSeed(GmpInteger seed)
    {
        GmpLib.__gmp_randseed(Raw, seed.Raw);
    }

    public GmpRandom Clone()
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_set(raw, Raw);
        return new GmpRandom(raw);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static GmpRandom CreateDefault()
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(raw);
        return new GmpRandom(raw);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static GmpRandom CreateDefault(uint seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(raw);
        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static GmpRandom CreateDefault(GmpInteger seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_default(raw);
        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static GmpRandom CreateMersenneTwister()
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_mt(raw);
        return new GmpRandom(raw);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static GmpRandom CreateMersenneTwister(uint seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_mt(raw);
        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static GmpRandom CreateMersenneTwister(GmpInteger seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_mt(raw);
        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// <para>
    /// Initialize state with a linear congruential algorithm X = (a*X + c) mod 2^m2exp.
    /// </para>
    /// <para>
    /// The low bits of X in this algorithm are not very random. 
    /// The least significant bit will have a period no more than 2, 
    /// and the second bit no more than 4, etc. For this reason only the high half of each X is actually used.
    /// </para>
    /// <para>
    /// When a random number of more than m2exp/2 bits is to be generated,
    /// multiple iterations of the recurrence are used and the results concatenated.
    /// </para>
    /// </summary>
    public static GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_lc_2exp(raw, a.Raw, c, m2exp);

        return new GmpRandom(raw);
    }

    /// <summary>
    /// <para>
    /// Initialize state with a linear congruential algorithm X = (a*X + c) mod 2^m2exp.
    /// </para>
    /// <para>
    /// The low bits of X in this algorithm are not very random. 
    /// The least significant bit will have a period no more than 2, 
    /// and the second bit no more than 4, etc. For this reason only the high half of each X is actually used.
    /// </para>
    /// <para>
    /// When a random number of more than m2exp/2 bits is to be generated,
    /// multiple iterations of the recurrence are used and the results concatenated.
    /// </para>
    /// </summary>
    public static GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp, uint seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_lc_2exp(raw, a.Raw, c, m2exp);

        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// <para>
    /// Initialize state with a linear congruential algorithm X = (a*X + c) mod 2^m2exp.
    /// </para>
    /// <para>
    /// The low bits of X in this algorithm are not very random. 
    /// The least significant bit will have a period no more than 2, 
    /// and the second bit no more than 4, etc. For this reason only the high half of each X is actually used.
    /// </para>
    /// <para>
    /// When a random number of more than m2exp/2 bits is to be generated,
    /// multiple iterations of the recurrence are used and the results concatenated.
    /// </para>
    /// </summary>
    public static GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp, GmpInteger seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        GmpLib.__gmp_randinit_lc_2exp(raw, a.Raw, c, m2exp);

        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// Initialize state for a linear congruential algorithm as per gmp_randinit_lc_2exp. 
    /// a, c and m2exp are selected from a table, 
    /// chosen so that size bits (or more) of each X will be used, i.e. m2exp/2 >= size.
    /// </summary>
    /// <returns>
    /// If size is bigger than the table data provides then the return <see cref="ArgumentOutOfRangeException"/>. 
    /// The maximum size currently supported is 128.
    /// </returns>
    public static GmpRandom CreateLC2ExpSize(uint size = 128)
    {
        IntPtr raw = GmpRandomState.Alloc();
        if (GmpLib.__gmp_randinit_lc_2exp_size(raw, size) == 0)
        {
            throw new ArgumentOutOfRangeException("size");
        }
        return new GmpRandom(raw);
    }

    /// <summary>
    /// Initialize state for a linear congruential algorithm as per gmp_randinit_lc_2exp. 
    /// a, c and m2exp are selected from a table, 
    /// chosen so that size bits (or more) of each X will be used, i.e. m2exp/2 >= size.
    /// </summary>
    /// <returns>
    /// If size is bigger than the table data provides then the return <see cref="ArgumentOutOfRangeException"/>. 
    /// The maximum size currently supported is 128.
    /// </returns>
    public static GmpRandom CreateLC2ExpSize(uint size, uint seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        if (GmpLib.__gmp_randinit_lc_2exp_size(raw, size) == 0)
        {
            throw new ArgumentOutOfRangeException("size");
        }
        return new GmpRandom(raw, seed);
    }

    /// <summary>
    /// Initialize state for a linear congruential algorithm as per gmp_randinit_lc_2exp. 
    /// a, c and m2exp are selected from a table, 
    /// chosen so that size bits (or more) of each X will be used, i.e. m2exp/2 >= size.
    /// </summary>
    /// <returns>
    /// If size is bigger than the table data provides then the return <see cref="ArgumentOutOfRangeException"/>. 
    /// The maximum size currently supported is 128.
    /// </returns>
    public static GmpRandom CreateLC2ExpSize(uint size, GmpInteger seed)
    {
        IntPtr raw = GmpRandomState.Alloc();
        if (GmpLib.__gmp_randinit_lc_2exp_size(raw, size) == 0)
        {
            throw new ArgumentOutOfRangeException("size");
        }
        return new GmpRandom(raw, seed);
    }
    #endregion

    #region Generation functions
    /// <summary>
    /// Return a uniformly distributed random number of n bits, i.e. in the range 0 to 2^n-1 inclusive. n must be less than or equal to the number of bits in an unsigned long.
    /// </summary>
    public uint NextNBits(uint bitCount)
    {
        return GmpLib.__gmp_urandomb_ui(Raw, bitCount);
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2n-1, inclusive.
    /// </summary>
    public void NextNBits(GmpInteger rop, uint bitCount)
    {
        GmpLib.__gmpz_urandomb(rop.Raw, Raw, bitCount);
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2n-1, inclusive.
    /// </summary>
    public GmpInteger NextGmpIntegerNBits(uint bitCount)
    {
        GmpInteger rop = new();
        NextNBits(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Generate a random integer with long strings of zeros and ones in the binary representation. 
    /// Useful for testing functions and algorithms, 
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// The random number will be in the range 2n-1 to 2n-1, inclusive.
    /// </summary>
    public void RNextNBits(GmpInteger rop, uint bitCount)
    {
        GmpLib.__gmpz_rrandomb(rop.Raw, Raw, bitCount);
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2n-1, inclusive.
    /// </summary>
    /// <returns>The random number will be in the range 2n-1 to 2n-1, inclusive.</returns>
    public GmpInteger RNextNBits(uint bitCount)
    {
        GmpInteger rop = new();
        RNextNBits(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Return a uniformly distributed random number in the range 0 to n-1, inclusive.
    /// </summary>
    public uint Next(uint n)
    {
        return GmpLib.__gmp_urandomm_ui(Raw, n);
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2n-1, inclusive.
    /// </summary>
    public void Next(GmpInteger rop, GmpInteger n)
    {
        GmpLib.__gmpz_urandomm(rop.Raw, Raw, n.Raw);
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2n-1, inclusive.
    /// </summary>
    public GmpInteger NextGmpInteger(GmpInteger n)
    {
        GmpInteger rop = new();
        Next(rop, n);
        return rop;
    }

    /// <summary>
    /// Generate a uniformly distributed random float in rop, such that 0 &lt;= rop &lt; 1, 
    /// with nbits significant bits in the mantissa or less if the precision of rop is smaller.
    /// </summary>
    public void Next(GmpFloat rop, uint nbits)
    {
        GmpLib.__gmpf_urandomb(rop.Raw, Raw, nbits);
    }

    /// <summary>
    /// Generate a uniformly distributed random float in rop, such that 0 &lt;= rop &lt; 1, 
    /// with nbits significant bits in the mantissa or less if the precision of rop is smaller.
    /// </summary>
    public GmpFloat NextGmpFloat(uint precision, uint nbits)
    {
        GmpFloat rop = new(precision);
        Next(rop, nbits);
        return rop;
    }
    #endregion

    #region Dispose pattern
    private bool _disposed;

    private void Clear()
    {
        GmpLib.__gmp_randclear(Raw);
        Marshal.FreeHGlobal(Raw);
        Raw = IntPtr.Zero;
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

    ~GmpRandom()
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

public enum GmpRandomAlgorithm
{
    Default = 0,
    LC = 0,
}

[StructLayout(LayoutKind.Sequential)]
public record struct GmpRandomState
{
    public Mpz_t Seed;
    public GmpRandomAlgorithm Algorithm;
    public IntPtr Data;

    public static unsafe int RawSize => sizeof(GmpRandomState);
    public static unsafe IntPtr Alloc() => Marshal.AllocHGlobal(sizeof(GmpRandomState));
}