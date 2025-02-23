using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

[StructLayout(LayoutKind.Sequential)]
internal record struct Mpfr_t
{
    public CLong Precision;
    public int Sign;
    public CLong Exponent;
    public IntPtr Limbs;

    public static unsafe int RawSize => sizeof(Mpfr_t);

    private readonly int LimbCount => (int)((Precision.Value - 1) / (IntPtr.Size * 8) + 1);

    private readonly unsafe Span<ulong> GetLimbData() => new((ulong*)Limbs, LimbCount);

    public override readonly int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Sign);
        c.Add(Exponent);
        foreach (ulong i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}