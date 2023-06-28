using Sdcb.Arithmetic.Gmp;
using System;

namespace Sdcb.Arithmetic.Mpfr;

/// <summary>
/// Provides MPFR related extension methods for the <see cref="GmpRandom"/> class.
/// </summary>
public unsafe static class GmpRandomExtensions
{
    /// <summary>
    /// Generate a random <see cref="MpfrFloat"/> instance in-place using the specified <paramref name="random"/> generator.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the generated value.</param>
    /// <returns>An integer indicating the success or failure of the operation.</returns>
    /// <remarks>
    /// The <paramref name="rop"/> instance will be modified to store the generated value.
    /// </remarks>
    public static int NextMpfrFloatInplace(this GmpRandom random, MpfrFloat rop)
    {
        fixed (GmpRandomState* prandom = &random.Raw)
        fixed (Mpfr_t* pop = &rop.Raw)
        {
            return MpfrLib.mpfr_urandomb((IntPtr)pop, (IntPtr)prandom);
        }
    }

    /// <summary>
    /// Generates a new random <see cref="MpfrFloat"/> instance using the specified <paramref name="r"/> random number generator and the specified <paramref name="precision"/> in bit. If no precision is specified, the default precision (<see cref="MpfrFloat.DefaultPrecision"/>) is used.
    /// </summary>
    /// <param name="r">The random number generator to use.</param>
    /// <param name="precision">The precision in bit of the generated <see cref="MpfrFloat"/> instance. If not specified, the default precision is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the generated random value.</returns>
    public static MpfrFloat NextMpfrFloat(this GmpRandom r, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
        NextMpfrFloatInplace(r, rop);
        return rop;
    }

    /// <summary>
    /// Generate a random <see cref="MpfrFloat"/> number using the specified <paramref name="random"/> generator and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="MpfrFloat.DefaultRounding"/>.</param>
    /// <returns>The status code of the operation.</returns>
    public static int NextMpfrFloatRoundInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (GmpRandomState* prandom = &random.Raw)
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_urandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
        }
    }

    /// <summary>
    /// Generates a new random <see cref="MpfrFloat"/> instance with optional <paramref name="precision"/> and <paramref name="rounding"/>.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> instance to use for generating the random number.</param>
    /// <param name="precision">The precision of the generated number in bits. If not specified, the default precision of <see cref="MpfrFloat"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use for the generated number. If not specified, the default rounding mode of <see cref="MpfrFloat"/> will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the generated random number.</returns>
    public static MpfrFloat NextMpfrFloatRound(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
        NextMpfrFloatRoundInplace(random, rop, rounding);
        return rop;
    }

    /// <summary>
    /// Generates a random <see cref="MpfrFloat"/> instance in place using the specified <paramref name="random"/> generator and <paramref name="rop"/> as output.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the generated value.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="MpfrFloat.DefaultRounding"/>.</param>
    /// <returns>The status code of the generation operation.</returns>
    public static int NextNMpfrFloatInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (GmpRandomState* prandom = &random.Raw)
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_nrandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
        }
    }

    /// <summary>
    /// Generates a new random <see cref="MpfrFloat"/> instance using the specified <paramref name="random"/> generator.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="precision">The precision of the generated <see cref="MpfrFloat"/> instance in bits. If null, <see cref="MpfrFloat.DefaultPrecision"/> is used.</param>
    /// <param name="rounding">The rounding mode to use when generating the <see cref="MpfrFloat"/> instance. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the generated random value.</returns>
    public static MpfrFloat NextNMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
        NextNMpfrFloatInplace(random, rop, rounding);
        return rop;
    }

    /// <summary>
    /// Generate two random <see cref="MpfrFloat"/> instances in the range [0, 1) using the specified <paramref name="random"/> generator and store them in-place in <paramref name="rop1"/> and <paramref name="rop2"/>.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="rop1">The first <see cref="MpfrFloat"/> instance to store the generated value.</param>
    /// <param name="rop2">The second <see cref="MpfrFloat"/> instance to store the generated value.</param>
    /// <param name="rounding">The optional rounding mode to use, default to <see cref="MpfrFloat.DefaultRounding"/>.</param>
    /// <returns>The return value of <see cref="MpfrLib.mpfr_grandom(IntPtr, IntPtr, IntPtr, MpfrRounding)"/>.</returns>
    [Obsolete("Use NextNMpfrFloatInplace instead: mpfr_nrandom is much more efficient than mpfr_grandom, especially for large precision. Thus mpfr_grandom is marked as deprecated and will be removed in a future release.")]
    public static int NextGMpfrFloatInplace(this GmpRandom random, MpfrFloat rop1, MpfrFloat rop2, MpfrRounding? rounding = null)
    {
        fixed (GmpRandomState* prandom = &random.Raw)
        fixed (Mpfr_t* pr1 = &rop1.Raw)
        fixed (Mpfr_t* pr2 = &rop2.Raw)
        {
            return MpfrLib.mpfr_grandom((IntPtr)pr1, (IntPtr)pr2, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
        }
    }

    /// <summary>
    /// Generate two random <see cref="MpfrFloat"/> instances using the specified <paramref name="random"/> generator.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> generator to use.</param>
    /// <param name="precision">The precision in bits of the generated <see cref="MpfrFloat"/> instances. If null, <see cref="MpfrFloat.DefaultPrecision"/> is used.</param>
    /// <param name="rounding">The rounding mode to use when generating the <see cref="MpfrFloat"/> instances. If null, the default rounding mode is used.</param>
    /// <returns>A tuple containing two new instances of <see cref="MpfrFloat"/> representing the generated values.</returns>
    [Obsolete("Use NextNMpfrFloat instead: mpfr_nrandom is much more efficient than mpfr_grandom, especially for large precision. Thus mpfr_grandom is marked as deprecated and will be removed in a future release.")]
    public static (MpfrFloat op1, MpfrFloat op2) Next2GMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop1 = new(precision ?? MpfrFloat.DefaultPrecision);
        MpfrFloat rop2 = new(precision ?? MpfrFloat.DefaultPrecision);
        NextGMpfrFloatInplace(random, rop1, rop2, rounding);
        return (rop1, rop2);
    }

    /// <summary>
    /// Generate a random <see cref="MpfrFloat"/> instance in place using the specified <paramref name="random"/> generator and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> instance to use as the random number generator.</param>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the generated random number.</param>
    /// <param name="rounding">The optional rounding mode to use, default to <see cref="MpfrFloat.DefaultRounding"/>.</param>
    /// <returns>The status code of the generation operation.</returns>
    public static int NextEMpfrFloatInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (GmpRandomState* prandom = &random.Raw)
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_erandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
        }
    }

    /// <summary>
    /// Generates a new <see cref="MpfrFloat"/> instance with the next random value from <paramref name="random"/> using the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="random">The <see cref="GmpRandom"/> instance to generate the random value from.</param>
    /// <param name="precision">The precision of the generated <see cref="MpfrFloat"/> instance in bits. If null, the default precision of <see cref="MpfrFloat"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use when generating the random value. If null, the default rounding mode of <see cref="MpfrFloat"/> will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the next random value generated by <paramref name="random"/>.</returns>
    public static MpfrFloat NextEMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
        NextEMpfrFloatInplace(random, rop, rounding);
        return rop;
    }
}