using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// A struct that represents a rational number in GMP library.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Mpq_t
{
    /// <summary>
    /// The numerator of the rational number.
    /// </summary>
    public Mpz_t Num;

    /// <summary>
    /// The denominator of the rational number.
    /// </summary>
    public Mpz_t Den;

    /// <summary>
    /// Returns the size in bytes of the raw structure representation in memory.
    /// </summary>
    public static int RawSize => Marshal.SizeOf<Mpq_t>();

    /// <summary>
    /// Returns a hash code for the current Mpq_t object.
    /// </summary>
    /// <returns>An integer that represents the hash code for the current object.</returns>
    public override readonly int GetHashCode() => HashCode.Combine(Num.GetHashCode(), Den.GetHashCode());
}
