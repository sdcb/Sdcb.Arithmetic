using System;

namespace Sdcb.Math.Gmp
{
    public static class GmpMath
    {
        public static unsafe BigFloat Sqrt(BigFloat op)
        {
            BigFloat rop = new();
            fixed (Mpf_t* prop = &rop.Raw)
            fixed (Mpf_t* pop2 = &op.Raw)
            {
                GmpNative.__gmpf_sqrt((IntPtr)prop, (IntPtr)pop2);
            }
            return rop;
        }

        public static unsafe BigFloat Sqrt(uint op)
        {
            BigFloat rop = new();
            fixed (Mpf_t* prop = &rop.Raw)
            {
                GmpNative.__gmpf_sqrt_ui((IntPtr)prop, op);
            }
            return rop;
        }
    }
}
