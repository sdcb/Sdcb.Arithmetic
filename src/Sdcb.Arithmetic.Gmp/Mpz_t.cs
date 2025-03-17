using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// Represents a multiple precision integer using the GMP library.
/// </summary>
public record struct Mpz_t
{
    /// <summary>
    /// The number of limbs allocated for this multiple precision integer. 
    /// </summary>
    public int Allocated;

    /// <summary>
    /// The number of limbs currently used to represent this multiple precision integer.
    /// </summary>
    public int Size;

    /// <summary>
    /// A pointer to an array of nint values representing the limbs of this multiple precision integer.
    /// </summary>
    public IntPtr Limbs;

    /// <summary>
    /// Gets the size of this struct in bytes.
    /// </summary>
    public unsafe static int RawSize => sizeof(Mpz_t);

    /// <summary>
    /// Returns a read-only span of the limbs of this multiple precision integer.
    /// </summary>
    /// <remarks>
    /// This method is unsafe because it returns a pointer to unmanaged memory.
    /// </remarks>
    private readonly unsafe Span<CULong> GetLimbData() => new((void*)Limbs, Allocated);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        HashCode c = new();
        c.Add(Allocated);
        c.Add(Size);
        foreach (CULong i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}