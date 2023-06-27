using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

public static class MpfrBuild
{
    public static string Version => Marshal.PtrToStringUTF8(MpfrLib.mpfr_get_version())!;

    public static string Patches => Marshal.PtrToStringUTF8(MpfrLib.mpfr_get_patches())!;

    public static bool HasThreadLocalStorage => MpfrLib.mpfr_buildopt_tls_p() != 0;

    public static bool HasFloat128 => MpfrLib.mpfr_buildopt_float128_p() != 0;

    public static bool HasDecimal => MpfrLib.mpfr_buildopt_decimal_p() != 0;

    public static bool HasGmpInternals => MpfrLib.mpfr_buildopt_gmpinternals_p() != 0;

    public static bool HasSharedCache => MpfrLib.mpfr_buildopt_sharedcache_p() != 0;

    public static string TuneCase => Marshal.PtrToStringUTF8(MpfrLib.mpfr_buildopt_tune_case())!;
}
