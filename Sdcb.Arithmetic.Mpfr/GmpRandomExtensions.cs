using Sdcb.Arithmetic.Gmp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sdcb.Arithmetic.Mpfr
{
    public unsafe static class GmpRandomExtensions
    {
        public static int NextMpfrFloatInplace(this GmpRandom random, MpfrFloat rop)
        {
            fixed (GmpRandomState* prandom = &random.Raw)
            fixed (byte* pop = &rop.Raw[0])
            {
                return MpfrLib.mpfr_urandomb((IntPtr)pop, (IntPtr)prandom);
            }
        }

        public static MpfrFloat NextMpfrFloat(this GmpRandom r, int? precision = null)
        {
            MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
            NextMpfrFloatInplace(r, rop);
            return rop;
        }

        public static int NextMpfrFloatRoundInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
        {
            fixed (GmpRandomState* prandom = &random.Raw)
            fixed (byte* pr = &rop.Raw[0])
            {
                return MpfrLib.mpfr_urandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
            }
        }

        public static MpfrFloat NextMpfrFloatRound(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
        {
            MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
            NextMpfrFloatRoundInplace(random, rop, rounding);
            return rop;
        }

        /// <summary>Generate the MpfrFloat according to a standard normal Gaussian distribution(with mean zero and variance one)</summary>
        public static int NextNMpfrFloatInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
        {
            fixed (GmpRandomState* prandom = &random.Raw)
            fixed (byte* pr = &rop.Raw[0])
            {
                return MpfrLib.mpfr_nrandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
            }
        }

        /// <summary>Generate the MpfrFloat according to a standard normal Gaussian distribution(with mean zero and variance one)</summary>
        public static MpfrFloat NextNMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
        {
            MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
            NextNMpfrFloatInplace(random, rop, rounding);
            return rop;
        }

        /// <summary>Generate 2 MpfrFloat according to a standard normal Gaussian distribution(with mean zero and variance one)</summary>
        public static int Next2NMpfrFloatInplace(this GmpRandom random, MpfrFloat rop1, MpfrFloat rop2, MpfrRounding? rounding = null)
        {
            fixed (GmpRandomState* prandom = &random.Raw)
            fixed (byte* pr1 = &rop1.Raw[0])
            fixed (byte* pr2 = &rop2.Raw[0])
            {
                return MpfrLib.mpfr_grandom((IntPtr)pr1, (IntPtr)pr2, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
            }
        }

        /// <returns>2 MpfrFloat according to a standard normal Gaussian distribution(with mean zero and variance one)</returns>
        public static (MpfrFloat op1, MpfrFloat op2) Next2NMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
        {
            MpfrFloat rop1 = new(precision ?? MpfrFloat.DefaultPrecision);
            MpfrFloat rop2 = new(precision ?? MpfrFloat.DefaultPrecision);
            Next2NMpfrFloatInplace(random, rop1, rop2, rounding);
            return (rop1, rop2);
        }

        /// <summary>Generate one random floating-point number according to an exponential distribution, with mean one.</summary>
        public static int NextEMpfrFloatInplace(this GmpRandom random, MpfrFloat rop, MpfrRounding? rounding = null)
        {
            fixed (GmpRandomState* prandom = &random.Raw)
            fixed (byte* pr = &rop.Raw[0])
            {
                return MpfrLib.mpfr_erandom((IntPtr)pr, (IntPtr)prandom, rounding ?? MpfrFloat.DefaultRounding);
            }
        }

        /// <summary>Generate one random floating-point number according to an exponential distribution, with mean one.</summary>
        public static MpfrFloat NextEMpfrFloat(this GmpRandom random, int? precision = null, MpfrRounding? rounding = null)
        {
            MpfrFloat rop = new(precision ?? MpfrFloat.DefaultPrecision);
            NextEMpfrFloatInplace(random, rop, rounding);
            return rop;
        }
    }
}
