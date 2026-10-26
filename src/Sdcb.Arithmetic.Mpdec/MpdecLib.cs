using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sdcb.Arithmetic.Mpdec.Tests")]
#pragma warning disable IDE1006 // Naming Styles

namespace Sdcb.Arithmetic.Mpdec;

/// <summary>
/// Raw P/Invoke binding of libmpdec (mpdecimal), the arbitrary-precision decimal library behind CPython's <c>decimal</c> module.
/// Names, signatures and constants map 1:1 to <c>mpdecimal.h</c> (64-bit configuration); there is no managed high-level API.
/// </summary>
/// <remarks>
/// <para>Strings (<c>char*</c>) are passed as NUL-terminated UTF-8 <see cref="byte"/> pointers. Strings returned by
/// libmpdec (e.g. <c>mpd_to_sci</c>) must be released with <see cref="mpd_free(void*)"/>.</para>
/// <para>Prefer the quiet <c>mpd_q*</c> functions. The signalling functions call <see cref="mpd_traphandler"/> when a trapped
/// condition occurs; the default handler raises <c>SIGFPE</c>, which terminates a .NET process.</para>
/// </remarks>
public static unsafe partial class MpdecLib
{
    static MpdecLib() => MpdecNativeLoader.Init();

    internal const string Dll = "libmpdec";

    #region Version
    public const int MPD_MAJOR_VERSION = 4;
    public const int MPD_MINOR_VERSION = 0;
    public const int MPD_MICRO_VERSION = 1;
    public const string MPD_VERSION = "4.0.1";
    public const int MPD_VERSION_HEX = (MPD_MAJOR_VERSION << 24) | (MPD_MINOR_VERSION << 16) | (MPD_MICRO_VERSION << 8);
    #endregion

    #region Types for 64 bit architectures
    public const ulong MPD_UINT_MAX = ulong.MaxValue;
    public const int MPD_BITS_PER_UINT = 64;
    public const long MPD_SSIZE_MAX = long.MaxValue;
    public const long MPD_SSIZE_MIN = long.MinValue;
    public const ulong MPD_RADIX = 10000000000000000000UL;
    public const int MPD_RDIGITS = 19;
    public const int MPD_MAX_POW10 = 19;
    public const int MPD_EXPDIGITS = 19;
    public const ulong MPD_MAXTRANSFORM_2N = 4294967296UL;
    public const long MPD_MAX_PREC = 999999999999999999L;
    public const int MPD_MAX_PREC_LOG2 = 64;
    public const long MPD_ELIMIT = 1000000000000000000L;
    public const long MPD_MAX_EMAX = 999999999999999999L;
    public const long MPD_MIN_EMIN = -999999999999999999L;
    public const long MPD_MIN_ETINY = MPD_MIN_EMIN - (MPD_MAX_PREC - 1);
    public const long MPD_EXP_INF = 2000000000000000001L;
    public const long MPD_EXP_CLAMP = -4000000000000000001L;
    public const long MPD_MAXIMPORT = 105263157894736842L;
    public const int MPD_IEEE_CONTEXT_MAX_BITS = 512;
    #endregion

    #region Context
    public const int MPD_ROUND_UP = 0;
    public const int MPD_ROUND_DOWN = 1;
    public const int MPD_ROUND_CEILING = 2;
    public const int MPD_ROUND_FLOOR = 3;
    public const int MPD_ROUND_HALF_UP = 4;
    public const int MPD_ROUND_HALF_DOWN = 5;
    public const int MPD_ROUND_HALF_EVEN = 6;
    public const int MPD_ROUND_05UP = 7;
    public const int MPD_ROUND_TRUNC = 8;
    public const int MPD_ROUND_GUARD = 9;

    public const int MPD_CLAMP_DEFAULT = 0;
    public const int MPD_CLAMP_IEEE_754 = 1;
    public const int MPD_CLAMP_GUARD = 2;

    public const uint MPD_Clamped = 0x00000001U;
    public const uint MPD_Conversion_syntax = 0x00000002U;
    public const uint MPD_Division_by_zero = 0x00000004U;
    public const uint MPD_Division_impossible = 0x00000008U;
    public const uint MPD_Division_undefined = 0x00000010U;
    public const uint MPD_Fpu_error = 0x00000020U;
    public const uint MPD_Inexact = 0x00000040U;
    public const uint MPD_Invalid_context = 0x00000080U;
    public const uint MPD_Invalid_operation = 0x00000100U;
    public const uint MPD_Malloc_error = 0x00000200U;
    public const uint MPD_Not_implemented = 0x00000400U;
    public const uint MPD_Overflow = 0x00000800U;
    public const uint MPD_Rounded = 0x00001000U;
    public const uint MPD_Subnormal = 0x00002000U;
    public const uint MPD_Underflow = 0x00004000U;
    public const uint MPD_Max_status = 0x00008000U - 1U;

    /// <summary>Conditions that result in an IEEE 754 exception.</summary>
    public const uint MPD_IEEE_Invalid_operation = MPD_Conversion_syntax | MPD_Division_impossible | MPD_Division_undefined |
        MPD_Fpu_error | MPD_Invalid_context | MPD_Invalid_operation | MPD_Malloc_error;
    /// <summary>Errors that require the result of an operation to be set to NaN.</summary>
    public const uint MPD_Errors = MPD_IEEE_Invalid_operation | MPD_Division_by_zero;
    /// <summary>Default traps.</summary>
    public const uint MPD_Traps = MPD_IEEE_Invalid_operation | MPD_Division_by_zero | MPD_Overflow | MPD_Underflow;
    public const uint MPD_Insufficient_storage = MPD_Malloc_error;

    public const int MPD_DECIMAL32 = 32;
    public const int MPD_DECIMAL64 = 64;
    public const int MPD_DECIMAL128 = 128;

    public const long MPD_MINALLOC_MIN = 2;
    public const long MPD_MINALLOC_MAX = 64;

    /// <summary><c>extern mpd_ssize_t MPD_MINALLOC;</c> (set it with <c>mpd_setminalloc</c>).</summary>
    public static long MPD_MINALLOC => *(long*)MpdecNativeLoader.GetExport(nameof(MPD_MINALLOC));

    /// <summary><c>extern const char * const mpd_round_string[MPD_ROUND_GUARD];</c></summary>
    public static byte* mpd_round_string(int round)
    {
        if ((uint)round >= MPD_ROUND_GUARD) throw new ArgumentOutOfRangeException(nameof(round));
        return ((byte**)MpdecNativeLoader.GetExport(nameof(mpd_round_string)))[round];
    }

    /// <summary><c>extern const char * const mpd_clamp_string[MPD_CLAMP_GUARD];</c></summary>
    public static byte* mpd_clamp_string(int clamp)
    {
        if ((uint)clamp >= MPD_CLAMP_GUARD) throw new ArgumentOutOfRangeException(nameof(clamp));
        return ((byte**)MpdecNativeLoader.GetExport(nameof(mpd_clamp_string)))[clamp];
    }

    /// <summary><c>extern void (* mpd_traphandler)(mpd_context_t *);</c> called by the signalling functions.</summary>
    public static delegate* unmanaged[Cdecl]<mpd_context_t*, void> mpd_traphandler
    {
        get => *(delegate* unmanaged[Cdecl]<mpd_context_t*, void>*)MpdecNativeLoader.GetExport(nameof(mpd_traphandler));
        set => *(delegate* unmanaged[Cdecl]<mpd_context_t*, void>*)MpdecNativeLoader.GetExport(nameof(mpd_traphandler)) = value;
    }
    #endregion

    #region Decimal Arithmetic
    public const byte MPD_POS = 0;
    public const byte MPD_NEG = 1;
    public const byte MPD_INF = 2;
    public const byte MPD_NAN = 4;
    public const byte MPD_SNAN = 8;
    public const byte MPD_SPECIAL = MPD_INF | MPD_NAN | MPD_SNAN;
    public const byte MPD_STATIC = 16;
    public const byte MPD_STATIC_DATA = 32;
    public const byte MPD_SHARED_DATA = 64;
    public const byte MPD_CONST_DATA = 128;
    public const byte MPD_DATAFLAGS = MPD_STATIC_DATA | MPD_SHARED_DATA | MPD_CONST_DATA;
    #endregion

    #region Triple
    public const int MPD_TRIPLE_NORMAL = 0;
    public const int MPD_TRIPLE_INF = 1;
    public const int MPD_TRIPLE_QNAN = 2;
    public const int MPD_TRIPLE_SNAN = 3;
    public const int MPD_TRIPLE_ERROR = 4;
    #endregion

    #region Quiet, thread-safe functions
    public const int MPD_NUM_FLAGS = 15;
    public const int MPD_MAX_FLAG_STRING = 208;
    public const int MPD_MAX_FLAG_LIST = MPD_MAX_FLAG_STRING + 18;
    public const int MPD_MAX_SIGNAL_LIST = 121;
    #endregion

    #region Memory handling
    /// <summary><c>extern void *(* mpd_mallocfunc)(size_t size);</c></summary>
    public static delegate* unmanaged[Cdecl]<nuint, void*> mpd_mallocfunc
        => *(delegate* unmanaged[Cdecl]<nuint, void*>*)MpdecNativeLoader.GetExport(nameof(mpd_mallocfunc));

    /// <summary><c>extern void *(* mpd_callocfunc)(size_t nmemb, size_t size);</c></summary>
    public static delegate* unmanaged[Cdecl]<nuint, nuint, void*> mpd_callocfunc
        => *(delegate* unmanaged[Cdecl]<nuint, nuint, void*>*)MpdecNativeLoader.GetExport(nameof(mpd_callocfunc));

    /// <summary><c>extern void *(* mpd_reallocfunc)(void *ptr, size_t size);</c></summary>
    public static delegate* unmanaged[Cdecl]<void*, nuint, void*> mpd_reallocfunc
        => *(delegate* unmanaged[Cdecl]<void*, nuint, void*>*)MpdecNativeLoader.GetExport(nameof(mpd_reallocfunc));

    private static delegate* unmanaged[Cdecl]<void*, void> _free;

    /// <summary>
    /// Calls <c>extern void (* mpd_free)(void *ptr);</c>. Use it to release strings and buffers allocated by libmpdec
    /// (e.g. the results of <c>mpd_to_sci</c>, <c>mpd_qformat</c>, <c>mpd_qexport_u32</c>).
    /// </summary>
    public static void mpd_free(void* ptr)
    {
        if (_free == null)
        {
            _free = *(delegate* unmanaged[Cdecl]<void*, void>*)MpdecNativeLoader.GetExport(nameof(mpd_free));
        }
        _free(ptr);
    }
    #endregion
}
