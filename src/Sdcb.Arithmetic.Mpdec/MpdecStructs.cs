using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles

namespace Sdcb.Arithmetic.Mpdec;

/// <summary>
/// <c>mpd_context_t</c>: precision, exponent limits, rounding mode, traps and status flags.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct mpd_context_t
{
    /// <summary>precision</summary>
    public long prec;
    /// <summary>max positive exp</summary>
    public long emax;
    /// <summary>min negative exp</summary>
    public long emin;
    /// <summary>status events that should be trapped</summary>
    public uint traps;
    /// <summary>status flags</summary>
    public uint status;
    /// <summary>set by mpd_addstatus_raise()</summary>
    public uint newtrap;
    /// <summary>rounding mode, one of <c>MpdecLib.MPD_ROUND_*</c></summary>
    public int round;
    /// <summary>clamp mode, one of <c>MpdecLib.MPD_CLAMP_*</c></summary>
    public int clamp;
    /// <summary>all functions correctly rounded</summary>
    public int allcr;
}

/// <summary>
/// <c>mpd_t</c>: a decimal number, value = (-1)^sign * coefficient * 10^exp.
/// The coefficient is stored little-endian in <see cref="data"/> words of base 10^19 (<c>MPD_RADIX</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct mpd_t
{
    /// <summary>combination of <c>MpdecLib.MPD_POS/NEG/INF/NAN/SNAN/STATIC/...</c></summary>
    public byte flags;
    public long exp;
    /// <summary>number of decimal digits in the coefficient</summary>
    public long digits;
    /// <summary>number of words in use</summary>
    public long len;
    /// <summary>number of words allocated</summary>
    public long alloc;
    public ulong* data;
}

/// <summary>
/// <c>mpd_uint128_triple_t</c>: (sign, 128-bit coefficient, exponent) interchange representation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct mpd_uint128_triple_t
{
    /// <summary>one of <c>MpdecLib.MPD_TRIPLE_*</c></summary>
    public int tag;
    public byte sign;
    public ulong hi;
    public ulong lo;
    public long exp;
}

/// <summary>
/// <c>mpd_spec_t</c>: parsed format specification used by <c>mpd_qformat_spec</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct mpd_spec_t
{
    /// <summary>minimum field width</summary>
    public long min_width;
    /// <summary>fraction digits or significant digits</summary>
    public long prec;
    /// <summary>conversion specifier</summary>
    public byte type;
    /// <summary>alignment</summary>
    public byte align;
    /// <summary>sign printing/alignment</summary>
    public byte sign;
    /// <summary>coerce to positive zero</summary>
    public byte sign_coerce;
    /// <summary>fill character (UTF-8, NUL terminated)</summary>
    public fixed byte fill[5];
    /// <summary>decimal point</summary>
    public byte* dot;
    /// <summary>thousands separator</summary>
    public byte* sep;
    /// <summary>grouping of digits</summary>
    public byte* grouping;
}
