namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// Represents the state of possibility of a number being prime.
/// </summary>
public enum PrimePossibility
{
    /// <summary>
    /// Definitely non-prime.
    /// </summary>
    No = 0,

    /// <summary>
    /// Probably prime (without being certain).
    /// </summary>
    Probably = 1,

    /// <summary>
    /// Definitely prime.
    /// </summary>
    Yes = 2,
}
