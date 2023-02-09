using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Sdcb.Arithmetic.Gmp;

public class GmpRandom : IDisposable
{
    public readonly GmpRandomState Raw;

    #region Random State Initialization

    public GmpRandom(GmpRandomState raw)
    {
        Raw = raw;
        SetRandomSeed();
    }

    public GmpRandom(GmpRandomState raw, uint seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    public GmpRandom(GmpRandomState raw, GmpInteger seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    public unsafe GmpRandom()
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
        SetRandomSeed();
    }

    public unsafe GmpRandom(uint seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
        SetSeed(seed);
    }

    public unsafe GmpRandom(GmpInteger seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
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

    public unsafe void SetSeed(uint seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randseed_ui((IntPtr)ptr, (uint)seed);
        }
    }

    public unsafe void SetSeed(GmpInteger seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        fixed (Mpz_t* seedPtr = &seed.Raw)
        {
            GmpLib.__gmp_randseed((IntPtr)ptr, (IntPtr)seedPtr);
        }
    }

    public unsafe GmpRandom Clone()
    {
        GmpRandomState raw = new();
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_set((IntPtr)(&raw), (IntPtr)ptr);
        }
        return new GmpRandom(raw);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static unsafe GmpRandom CreateDefault()
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static unsafe GmpRandom CreateDefault(uint seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Initialize state with a default algorithm. 
    /// This will be a compromise between speed and randomness, 
    /// and is recommended for applications with no special requirements. 
    /// Currently this is gmp_randinit_mt.
    /// </summary>
    public static unsafe GmpRandom CreateDefault(GmpInteger seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static unsafe GmpRandom CreateMersenneTwister()
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static unsafe GmpRandom CreateMersenneTwister(uint seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Initialize state for a Mersenne Twister algorithm. 
    /// This algorithm is fast and has good randomness properties.
    /// </summary>
    public static unsafe GmpRandom CreateMersenneTwister(GmpInteger seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state, seed);
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
    public static unsafe GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp)
    {
        GmpRandomState state = new();
        fixed (Mpz_t* pa = &a.Raw)
        {
            GmpLib.__gmp_randinit_lc_2exp((IntPtr)(&state), (IntPtr)pa, c, m2exp);
        }

        return new GmpRandom(state);
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
    public static unsafe GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp, uint seed)
    {
        GmpRandomState state = new();
        fixed (Mpz_t* pa = &a.Raw)
        {
            GmpLib.__gmp_randinit_lc_2exp((IntPtr)(&state), (IntPtr)pa, c, m2exp);
        }

        return new GmpRandom(state, seed);
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
    public static unsafe GmpRandom CreateLC2Exp(GmpInteger a, uint c, uint m2exp, GmpInteger seed)
    {
        GmpRandomState state = new();
        fixed (Mpz_t* pa = &a.Raw)
        {
            GmpLib.__gmp_randinit_lc_2exp((IntPtr)(&state), (IntPtr)pa, c, m2exp);
        }

        return new GmpRandom(state, seed);
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
    public static unsafe GmpRandom CreateLC2ExpSize(uint size = 128)
    {
        GmpRandomState state = new();
        if (GmpLib.__gmp_randinit_lc_2exp_size((IntPtr)(&state), size) == 0)
        {
            throw new ArgumentOutOfRangeException("size");
        }
        return new GmpRandom(state);
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
    public static unsafe GmpRandom CreateLC2ExpSize(uint size, uint seed)
    {
        GmpRandomState state = new();
        if (GmpLib.__gmp_randinit_lc_2exp_size((IntPtr)(&state), size) == 0)
        {
            throw new ArgumentOutOfRangeException("size");
        }
        return new GmpRandom(state, seed);
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
    public static unsafe GmpRandom CreateLC2ExpSize(uint size, GmpInteger seed)
    {
        GmpRandomState state = new();
        if (GmpLib.__gmp_randinit_lc_2exp_size((IntPtr)(&state), size) == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }
        return new GmpRandom(state, seed);
    }
    #endregion

    #region Generation functions

    /// <returns>Returns a random integer, including positives, 0, and negatives</returns>
    public unsafe int Next()
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            const int bitCount = 32;
            return (int)GmpLib.__gmp_urandomb_ui((IntPtr)ptr, bitCount);
        }
    }

    public unsafe int Next(int maxValue)
    {
        if (maxValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Non-negative number required.");
        }
        fixed (GmpRandomState* ptr = &Raw)
        {
            return (int)GmpLib.__gmp_urandomm_ui((IntPtr)ptr, (uint)maxValue);
        }
    }

    public unsafe int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue), $"'{nameof(minValue)}' cannot be greater than maxValue.");
        }
        uint diff = (uint)(maxValue - minValue);
        fixed (GmpRandomState* ptr = &Raw)
        {
            return (int)(minValue + GmpLib.__gmp_urandomm_ui((IntPtr)ptr, diff));
        }
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2^(n-1), inclusive.
    /// </summary>
    public unsafe void NextGmpIntegerInplace(GmpInteger rop, uint bitCount)
    {
        if (bitCount == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), $"'{nameof(bitCount)}' must not be zero.");
        }

        fixed (Mpz_t* pr= &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        {
            GmpLib.__gmpz_urandomb((IntPtr)pr, (IntPtr)prandom, bitCount);
        }
    }

    /// <summary>
    /// Generate a uniformly distributed random integer in the range 0 to 2^(n-1), inclusive.
    /// </summary>
    public unsafe GmpInteger NextGmpInteger(uint bitCount)
    {
        GmpInteger rop = new();
        NextGmpIntegerInplace(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Generate a random integer with long strings of zeros and ones in the binary representation.
    /// Useful for testing functions and algorithms,
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs.
    /// The random number will be in the range 2^(n-1) to 2^n-1, inclusive.
    /// </summary>
    public unsafe void NextCornerGmpIntegerInplace(GmpInteger rop, uint bitCount)
    {
        if (bitCount == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), $"'{nameof(bitCount)}' must not be zero.");
        }

        fixed (Mpz_t* pr = &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        {
            GmpLib.__gmpz_rrandomb((IntPtr)pr, (IntPtr)prandom, bitCount);
        }
    }

    /// <summary>
    /// Generate a random integer with long strings of zeros and ones in the binary representation.
    /// Useful for testing functions and algorithms, 
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs.
    /// </summary>
    /// <returns>The random number will be in the range 2^(n-1) to 2^n-1, inclusive.</returns>
    public unsafe GmpInteger NextCornerGmpInteger(uint bitCount)
    {
        GmpInteger rop = new();
        NextCornerGmpIntegerInplace(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Generate a uniform random integer in the range 0 to n-1, inclusive.
    /// </summary>
    public unsafe void NextGmpIntegerInplace(GmpInteger rop, GmpInteger maxValue)
    {
        if (maxValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Non-negative number required.");
        }
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        fixed (Mpz_t* pn = &maxValue.Raw)
        {
            GmpLib.__gmpz_urandomm((IntPtr)pr, (IntPtr)prandom, (IntPtr)pn);
        }
    }

    /// <summary>
    /// Generate a uniform random integer in the range 0 to n-1, inclusive.
    /// </summary>
    public unsafe GmpInteger NextGmpInteger(GmpInteger maxValue)
    {
        GmpInteger rop = new();
        NextGmpIntegerInplace(rop, maxValue);
        return rop;
    }

    /// <summary>
    /// Generate a uniformly distributed random float in rop, such that 0 &lt;= rop &lt; 1, 
    /// with bitCount significant bits in the mantissa or less if the precision of rop is smaller.
    /// </summary>
    public unsafe void NextGmpFloatInplace(GmpFloat rop, uint bitCount)
    {
        fixed (Mpf_t* pr = &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        {
            GmpLib.__gmpf_urandomb((IntPtr)pr, (IntPtr)prandom, bitCount);
        }
    }

    /// <summary>
    /// Generate a uniformly distributed random float in rop, such that 0 &lt;= rop &lt; 1, 
    /// with bitCount significant bits in the mantissa or less if the precision of rop is smaller.
    /// </summary>
    public unsafe GmpFloat NextGmpFloat(uint precision, uint bitCount)
    {
        GmpFloat rop = new(precision);
        NextGmpFloatInplace(rop, bitCount);
        return rop;
    }
    #endregion

    #region Dispose pattern
    private bool _disposed;

    private unsafe void Clear()
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randclear((IntPtr)ptr);
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

[StructLayout(LayoutKind.Sequential)]
public struct GmpRandomState
{
    public Mpz_t Seed;
    public GmpRandomAlgorithm Algorithm;
    public IntPtr Data;
}

public enum GmpRandomAlgorithm
{
    Default = 0,
    LC = 0,
}
