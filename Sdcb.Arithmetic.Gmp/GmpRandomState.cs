using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

[StructLayout(LayoutKind.Sequential)]
internal struct GmpRandomState
{
    public Mpz_t Seed;
    public GmpRandomAlgorithm Algorithm;
    public IntPtr Data;
}
