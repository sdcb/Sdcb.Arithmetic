namespace Sdcb.Arithmetic.Mpfr;

/// <summary>
/// Definition of MPFR rounding modes
/// </summary>
public enum MpfrRounding
{
    /// <summary>
    /// MPFR_RNDN, round to nearest, with ties to even
    /// </summary>
    ToEven = 0,

    /// <summary>
    /// MPFR_RNDZ, round toward zero
    /// </summary>
    ToZero,

    /// <summary>
    /// MPFR_RNDU, round toward +Inf
    /// </summary>
    ToPositiveInfinity,

    /// <summary>
    /// MPFR_RNDD, round toward -Inf
    /// </summary>
    ToNegativeInfinity,

    /// <summary>
    /// MPFR_RNDA, round away from zero
    /// </summary>
    AwayFromZero,

    /// <summary>
    /// MPFR_RNDF, faithful rounding
    /// </summary>
    Faithful,
}
