namespace Sdcb.Arithmetic.Mpfr
{
    /// <summary>
    /// <para>
    /// Definition of rounding modes (DON'T USE MPFR_RNDNA!).
    /// </para>
    /// </summary>
    public enum MpfrRounding
    {
        /// <summary>
        /// round to nearest, with ties to even
        /// </summary>
        Nearest = 0,

        /// <summary>
        /// round toward zero
        /// </summary>
        Zero,

        /// <summary>
        /// round toward +Inf
        /// </summary>
        Up,

        /// <summary>
        /// round toward -Inf
        /// </summary>
        Down,

        /// <summary>
        /// round away from zero
        /// </summary>
        AwayFromZero,

        /// <summary>
        /// faithful rounding
        /// </summary>
        Faithful,

        /// <summary>
        /// round to nearest, with ties away from zero (mpfr_round)
        /// </summary>
        NA = -1, 
    }
}
