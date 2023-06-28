using System;
using System.Security.Cryptography;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// Represents a random number generator that uses the GNU Multiple Precision Arithmetic Library.
/// </summary>
public class GmpRandom : IDisposable
{
    internal readonly GmpRandomState Raw;

    #region Random State Initialization

    private GmpRandom(GmpRandomState raw)
    {
        Raw = raw;
        SetRandomSeed();
    }

    private GmpRandom(GmpRandomState raw, uint seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    private GmpRandom(GmpRandomState raw, GmpInteger seed)
    {
        Raw = raw;
        SetSeed(seed);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpRandom"/> class with default random state and sets the seed to a random value.
    /// </summary>
    /// <remarks>
    /// The default random state is initialized using <see cref="GmpLib.__gmp_randinit_default(IntPtr)"/>.
    /// </remarks>
    public unsafe GmpRandom()
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
        SetRandomSeed();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpRandom"/> class with the specified unsigned integer <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random state.</param>
    /// <remarks>
    /// The random state is initialized using the default algorithm and the specified <paramref name="seed"/>.
    /// </remarks>
    public unsafe GmpRandom(uint seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
        SetSeed(seed);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpRandom"/> class with the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random number generator.</param>
    /// <remarks>
    /// The <see cref="GmpRandom"/> instance is initialized with the default algorithm and default state size.
    /// </remarks>
    public unsafe GmpRandom(GmpInteger seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randinit_default((IntPtr)ptr);
        }
        SetSeed(seed);
    }

    /// <summary>
    /// Sets a random seed for the <see cref="GmpRandom"/> instance.
    /// </summary>
    /// <returns>The generated random seed.</returns>
    /// <remarks>
    /// The seed is generated using <see cref="RandomNumberGenerator.Fill(Span{byte})"/> method, which fills a span of bytes with a cryptographically strong random sequence of values.
    /// </remarks>
    public unsafe uint SetRandomSeed()
    {
        uint seed = default;
        RandomNumberGenerator.Fill(new Span<byte>(&seed, 4));
        SetSeed(seed);
        return seed;
    }

    /// <summary>
    /// Set the seed of the random number generator to the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to set.</param>
    public unsafe void SetSeed(uint seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            GmpLib.__gmp_randseed_ui((IntPtr)ptr, seed);
        }
    }

    /// <summary>
    /// Sets the seed of the current <see cref="GmpRandom"/> instance to the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed to set.</param>
    /// <remarks>
    /// The seed is used to initialize the state of the random number generator.
    /// </remarks>
    public unsafe void SetSeed(GmpInteger seed)
    {
        fixed (GmpRandomState* ptr = &Raw)
        fixed (Mpz_t* seedPtr = &seed.Raw)
        {
            GmpLib.__gmp_randseed((IntPtr)ptr, (IntPtr)seedPtr);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpRandom"/> with the same state as the current instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the same state as the current instance.</returns>
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
    /// Creates a new instance of <see cref="GmpRandom"/> with default state.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRandom"/> with default state.</returns>
    public static unsafe GmpRandom CreateDefault()
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> with default state and specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random state.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with default state and specified <paramref name="seed"/>.</returns>
    public static unsafe GmpRandom CreateDefault(uint seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> with default state and specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random state.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with default state and specified <paramref name="seed"/>.</returns>
    public static unsafe GmpRandom CreateDefault(GmpInteger seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_default((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm.</returns>
    public static unsafe GmpRandom CreateMersenneTwister()
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm with the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random number generator.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm.</returns>
    public static unsafe GmpRandom CreateMersenneTwister(uint seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm with the specified <paramref name="seed"/>.
    /// </summary>
    /// <param name="seed">The seed value to use for the random number generator.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> using the Mersenne Twister algorithm.</returns>
    public static unsafe GmpRandom CreateMersenneTwister(GmpInteger seed)
    {
        GmpRandomState state = new();
        GmpLib.__gmp_randinit_mt((IntPtr)(&state));
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> using the LC2Exp algorithm.
    /// </summary>
    /// <param name="a">The seed value as a <see cref="GmpInteger"/>.</param>
    /// <param name="c">The number of bits to discard from each value generated by the algorithm.</param>
    /// <param name="m2exp">The number of bits to generate before re-seeding the generator.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> initialized with the LC2Exp algorithm.</returns>
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
    /// Create a new instance of <see cref="GmpRandom"/> with a state initialized by the LCG algorithm with a modulus of 2^<paramref name="m2exp"/> and a multiplier of <paramref name="c"/>.
    /// </summary>
    /// <param name="a">The seed value as a <see cref="GmpInteger"/>.</param>
    /// <param name="c">The multiplier value.</param>
    /// <param name="m2exp">The exponent of the modulus value.</param>
    /// <param name="seed">The seed value.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the LCG state initialized.</returns>
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
    /// Create a new instance of <see cref="GmpRandom"/> with the state initialized by the LC 2-exponential algorithm.
    /// </summary>
    /// <param name="a">The multiplier value.</param>
    /// <param name="c">The increment value.</param>
    /// <param name="m2exp">The modulus value.</param>
    /// <param name="seed">The seed value.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the state initialized by the LC 2-exponential algorithm.</returns>
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
    /// Creates a new instance of <see cref="GmpRandom"/> with the state initialized using the LC2 algorithm with a size of <paramref name="size"/> bits.
    /// </summary>
    /// <param name="size">The size of the state in bits. Default is 128 bits.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the state initialized using the LC2 algorithm.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than or equal to zero.</exception>
    public static unsafe GmpRandom CreateLC2ExpSize(uint size = 128)
    {
        GmpRandomState state = new();
        if (GmpLib.__gmp_randinit_lc_2exp_size((IntPtr)(&state), size) == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }
        return new GmpRandom(state);
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpRandom"/> with a linear congruential generator using a power of two modulus of size <paramref name="size"/> and a seed value of <paramref name="seed"/>.
    /// </summary>
    /// <param name="size">The size of the modulus as a power of two.</param>
    /// <param name="seed">The seed value for the generator.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the specified generator and seed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than or equal to zero.</exception>
    public static unsafe GmpRandom CreateLC2ExpSize(uint size, uint seed)
    {
        GmpRandomState state = new();
        if (GmpLib.__gmp_randinit_lc_2exp_size((IntPtr)(&state), size) == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }
        return new GmpRandom(state, seed);
    }

    /// <summary>
    /// Create a new instance of <see cref="GmpRandom"/> with a state initialized by the LCG algorithm with a modulus of 2^<paramref name="size"/> and a seed of <paramref name="seed"/>.
    /// </summary>
    /// <param name="size">The size of the modulus in bits.</param>
    /// <param name="seed">The seed value to initialize the state.</param>
    /// <returns>A new instance of <see cref="GmpRandom"/> with the LCG state initialized.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="size"/> is less than or equal to zero.</exception>
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

    /// <summary>
    /// Generate a random integer value using the current state of the <see cref="GmpRandom"/> instance.
    /// </summary>
    /// <returns>A random integer value.</returns>
    public unsafe int Next()
    {
        fixed (GmpRandomState* ptr = &Raw)
        {
            const int bitCount = 32;
            return (int)GmpLib.__gmp_urandomb_ui((IntPtr)ptr, bitCount);
        }
    }

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum value.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
    /// <returns>A 32-bit signed integer greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
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

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
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
    /// Generate a random <see cref="GmpInteger"/> with <paramref name="bitCount"/> bits and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="bitCount">The number of bits to generate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitCount"/> is zero.</exception>
    /// <remarks>
    /// The random number is generated using the current state of the <see cref="GmpRandom"/> instance.
    /// </remarks>
    public unsafe void NextGmpIntegerInplace(GmpInteger rop, uint bitCount)
    {
        if (bitCount == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount), $"'{nameof(bitCount)}' must not be zero.");
        }

        fixed (Mpz_t* pr = &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        {
            GmpLib.__gmpz_urandomb((IntPtr)pr, (IntPtr)prandom, bitCount);
        }
    }

    /// <summary>
    /// Generate a random <see cref="GmpInteger"/> with <paramref name="bitCount"/> bits and return it.
    /// </summary>
    /// <param name="bitCount">The number of bits of the generated <see cref="GmpInteger"/>.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the generated random number.</returns>
    public unsafe GmpInteger NextGmpInteger(uint bitCount)
    {
        GmpInteger rop = new();
        NextGmpIntegerInplace(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Generate a random <see cref="GmpInteger"/> with <paramref name="bitCount"/> bits and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="bitCount">The number of bits to generate.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitCount"/> is zero.</exception>
    /// <remarks>
    /// The generated integer will be uniformly distributed over the range [0, 2^<paramref name="bitCount"/> - 1].
    /// </remarks>
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
    /// Generate a new <see cref="GmpInteger"/> instance with <paramref name="bitCount"/> random bits.
    /// </summary>
    /// <param name="bitCount">The number of bits to generate.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> with <paramref name="bitCount"/> random bits.</returns>
    public unsafe GmpInteger NextCornerGmpInteger(uint bitCount)
    {
        GmpInteger rop = new();
        NextCornerGmpIntegerInplace(rop, bitCount);
        return rop;
    }

    /// <summary>
    /// Generates a random <see cref="GmpInteger"/> less than <paramref name="maxValue"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number to generate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxValue"/> is negative.</exception>
    /// <remarks>
    /// This method uses the current instance of <see cref="GmpRandom"/> to generate the random number.
    /// </remarks>
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
    /// Generates a random <see cref="GmpInteger"/> value less than <paramref name="maxValue"/> and returns it.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the generated random number.</returns>
    public unsafe GmpInteger NextGmpInteger(GmpInteger maxValue)
    {
        GmpInteger rop = new();
        NextGmpIntegerInplace(rop, maxValue);
        return rop;
    }

    /// <summary>
    /// Generate a random <see cref="GmpFloat"/> with <paramref name="bitCount"/> bits of precision and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpFloat"/> instance to store the result in.</param>
    /// <param name="bitCount">The number of bits of precision for the generated <see cref="GmpFloat"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> is null.</exception>
    /// <remarks>
    /// The random number generator used is the one associated with this <see cref="GmpRandom"/> instance.
    /// </remarks>
    public unsafe void NextGmpFloatInplace(GmpFloat rop, uint bitCount)
    {
        fixed (Mpf_t* pr = &rop.Raw)
        fixed (GmpRandomState* prandom = &Raw)
        {
            GmpLib.__gmpf_urandomb((IntPtr)pr, (IntPtr)prandom, bitCount);
        }
    }

    /// <summary>
    /// Generate a new random <see cref="GmpFloat"/> instance with the specified <paramref name="precision"/> and <paramref name="bitCount"/>.
    /// </summary>
    /// <param name="precision">The precision in bits of the generated <see cref="GmpFloat"/> instance.</param>
    /// <param name="bitCount">The number of bits to generate for the mantissa of the <see cref="GmpFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> with the specified <paramref name="precision"/> and <paramref name="bitCount"/>.</returns>
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

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="GmpRandom"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

    /// <summary>
    /// Finalizer for <see cref="GmpRandom"/> class.
    /// </summary>
    ~GmpRandom()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}