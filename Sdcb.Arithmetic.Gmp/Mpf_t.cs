﻿using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// This struct represents a GMP floating-point number with arbitrary precision.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public record struct Mpf_t
{
    /// <summary>
    /// The number of significant bits in the number.
    /// </summary>
    public int Precision;
    /// <summary>
    /// The size of the data block in bytes.
    /// </summary>
    public int Size;
    /// <summary>
    /// The exponent of the number.
    /// </summary>
    public int Exponent;
    /// <summary>
    /// A pointer to the data block of the number.
    /// </summary>
    public IntPtr Limbs;

    /// <summary>
    /// The size of the struct in bytes.
    /// </summary>
    public static int RawSize => Marshal.SizeOf<Mpf_t>();


    private readonly unsafe Span<int> GetLimbData() => new((void*)Limbs, Precision - 1);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Size);
        c.Add(Exponent);
        foreach (int i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}
