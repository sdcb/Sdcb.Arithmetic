using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

[StructLayout(LayoutKind.Sequential)]
internal record struct LinuxMpfr_t
{
    public long Precision;
    public long Sign;
    public long Exponent;
    public IntPtr Limbs;

    public static unsafe int RawSize => sizeof(LinuxMpfr_t);

    private readonly int LimbCount => (int)((Precision - 1) / (IntPtr.Size * 8) + 1);

    private readonly unsafe Span<nint> GetLimbData() => new((void*)Limbs, LimbCount);

    public override readonly int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Sign);
        c.Add(Exponent);
        foreach (nint i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}