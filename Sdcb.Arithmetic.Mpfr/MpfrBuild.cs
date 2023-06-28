using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

/// <summary>
/// Utility class for retrieving build information about the MPFR library.
/// </summary>
public static class MpfrBuild
{
    /// <summary>
    /// Retrieves the version of the MPFR library in use.
    /// </summary>
    public static string Version => Marshal.PtrToStringUTF8(MpfrLib.mpfr_get_version())!;

    /// <summary>
    /// Retrieves a string with information about any patches applied to the MPFR library.
    /// </summary>
    public static string Patches => Marshal.PtrToStringUTF8(MpfrLib.mpfr_get_patches())!;

    /// <summary>
    /// Indicates whether the MPFR library was built with thread-local storage support.
    /// </summary>
    public static bool HasThreadLocalStorage => MpfrLib.mpfr_buildopt_tls_p() != 0;

    /// <summary>
    /// Indicates whether the MPFR library was built with support for 128-bit floating-point data types.
    /// </summary>
    public static bool HasFloat128 => MpfrLib.mpfr_buildopt_float128_p() != 0;

    /// <summary>
    /// Indicates whether the MPFR library was built with support for decimal floating-point data types.
    /// </summary>
    public static bool HasDecimal => MpfrLib.mpfr_buildopt_decimal_p() != 0;

    /// <summary>
    /// Indicates whether the MPFR library was built with support for GMP internals.
    /// </summary>
    public static bool HasGmpInternals => MpfrLib.mpfr_buildopt_gmpinternals_p() != 0;

    /// <summary>
    /// Indicates whether the MPFR library was built with shared cache support.
    /// </summary>
    public static bool HasSharedCache => MpfrLib.mpfr_buildopt_sharedcache_p() != 0;

    /// <summary>
    /// Retrieves a string with the name of the CPU tuning used in the MPFR library build.
    /// </summary>
    public static string TuneCase => Marshal.PtrToStringUTF8(MpfrLib.mpfr_buildopt_tune_case())!;
}
