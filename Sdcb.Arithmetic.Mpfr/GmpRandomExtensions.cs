using Sdcb.Arithmetic.Gmp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sdcb.Arithmetic.Mpfr
{
    public unsafe static class GmpRandomExtensions
    {
        public static int Next(this GmpRandom r, MpfrFloat rop)
        {
            fixed (GmpRandomState* prandom = &r.Raw)
            fixed (Mpfr_t* pop = &rop.Raw)
            {
                return MpfrLib.mpfr_urandomb((IntPtr)pop, (IntPtr)prandom);
            }
        }

        public static MpfrFloat NextMpfrFloat(this GmpRandom r, int? precision = null)
        {
            MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
            Next(r, rop);
            return rop;
        }
    }
}
