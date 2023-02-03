namespace Sdcb.Arithmetic.Mpfr
{
    /// <summary>
    /// Definition of MPFR rounding modes
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
    }
}
