using System;
using System.Collections.Generic;
using System.Text;

namespace Sdcb.Arithmetic.Mpfr;

[Flags]
public enum MpfrErrorFlags : uint
{
    Underflow = 0b_0000_0001, 
    Overflow = 0b_0000_0010,
    NaN = 0b_0000_0100, 
    Inexact = 0b_0000_1000, 
    ERange = 0b_0001_0000, 
    DivideByZero = 0b_0010_0000, 
}

internal static class MpfrKnownErrorFlags
{
    public static bool Underflow
    {
        get => MpfrLib.mpfr_underflow_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_underflow(); else MpfrLib.mpfr_clear_underflow();
        }
    }

    public static bool Overflow
    {
        get => MpfrLib.mpfr_overflow_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_overflow(); else MpfrLib.mpfr_clear_overflow();
        }
    }

    public static bool DivideByZero
    {
        get => MpfrLib.mpfr_divby0_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_divby0(); else MpfrLib.mpfr_clear_divby0();
        }
    }

    public static bool Invalid
    {
        get => MpfrLib.mpfr_nanflag_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_nanflag(); else MpfrLib.mpfr_clear_nanflag();
        }
    }

    public static bool Inexact
    {
        get => MpfrLib.mpfr_inexflag_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_inexflag(); else MpfrLib.mpfr_clear_inexflag();
        }
    }

    public static bool ERange
    {
        get => MpfrLib.mpfr_erangeflag_p() != 0;
        set
        {
            if (value) MpfrLib.mpfr_set_erangeflag(); else MpfrLib.mpfr_clear_erangeflag();
        }
    }
}
