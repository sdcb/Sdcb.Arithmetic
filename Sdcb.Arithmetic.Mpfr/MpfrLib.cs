using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Sdcb.Arithmetic.Mpfr.Tests")]

namespace Sdcb.Arithmetic.Mpfr;

using mpfr_rnd_t = MpfrRounding;
using mpfr_free_cache_t = MpfrFreeCache;

public static class MpfrLib
{
    static MpfrLib() => MpfrNativeLoader.Init();

    public const string Dll = "mpfr-6.dll";



    [DllImport(Dll)]
    public static extern IntPtr mpfr_get_version();


    [DllImport(Dll)]
    public static extern IntPtr mpfr_get_patches();


    [DllImport(Dll)]
    public static extern int mpfr_buildopt_tls_p();


    [DllImport(Dll)]
    public static extern int mpfr_buildopt_float128_p();


    [DllImport(Dll)]
    public static extern int mpfr_buildopt_decimal_p();


    [DllImport(Dll)]
    public static extern int mpfr_buildopt_gmpinternals_p();


    [DllImport(Dll)]
    public static extern int mpfr_buildopt_sharedcache_p();


    [DllImport(Dll)]
    public static extern IntPtr mpfr_buildopt_tune_case();


    [DllImport(Dll)]
    public static extern int mpfr_get_emin();

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int mpfr_set_emin(int p0);


    [DllImport(Dll)]
    public static extern int mpfr_get_emin_min();


    [DllImport(Dll)]
    public static extern int mpfr_get_emin_max();


    [DllImport(Dll)]
    public static extern int mpfr_get_emax();

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int mpfr_set_emax(int p0);


    [DllImport(Dll)]
    public static extern int mpfr_get_emax_min();


    [DllImport(Dll)]
    public static extern int mpfr_get_emax_max();

    /// <param name="p0">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_set_default_rounding_mode(mpfr_rnd_t p0);


    [DllImport(Dll)]
    public static extern mpfr_rnd_t mpfr_get_default_rounding_mode();

    /// <param name="p0">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr mpfr_print_rnd_mode(mpfr_rnd_t p0);


    [DllImport(Dll)]
    public static extern void mpfr_clear_flags();


    [DllImport(Dll)]
    public static extern void mpfr_clear_underflow();


    [DllImport(Dll)]
    public static extern void mpfr_clear_overflow();


    [DllImport(Dll)]
    public static extern void mpfr_clear_divby0();


    [DllImport(Dll)]
    public static extern void mpfr_clear_nanflag();


    [DllImport(Dll)]
    public static extern void mpfr_clear_inexflag();


    [DllImport(Dll)]
    public static extern void mpfr_clear_erangeflag();


    [DllImport(Dll)]
    public static extern void mpfr_set_underflow();


    [DllImport(Dll)]
    public static extern void mpfr_set_overflow();


    [DllImport(Dll)]
    public static extern void mpfr_set_divby0();


    [DllImport(Dll)]
    public static extern void mpfr_set_nanflag();


    [DllImport(Dll)]
    public static extern void mpfr_set_inexflag();


    [DllImport(Dll)]
    public static extern void mpfr_set_erangeflag();


    [DllImport(Dll)]
    public static extern int mpfr_underflow_p();


    [DllImport(Dll)]
    public static extern int mpfr_overflow_p();


    [DllImport(Dll)]
    public static extern int mpfr_divby0_p();


    [DllImport(Dll)]
    public static extern int mpfr_nanflag_p();


    [DllImport(Dll)]
    public static extern int mpfr_inexflag_p();


    [DllImport(Dll)]
    public static extern int mpfr_erangeflag_p();

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern void mpfr_flags_clear(uint p0);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern void mpfr_flags_set(uint p0);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern uint mpfr_flags_test(uint p0);


    [DllImport(Dll)]
    public static extern uint mpfr_flags_save();

    /// <param name="p0" />
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_flags_restore(uint p0, uint p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_check_range(IntPtr p0, int p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_init2(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_init(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_clear(IntPtr p0);

    /// <param name="p0" />
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_inits2(int p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_inits(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_clears(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_prec_round(IntPtr p0, int p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    /// <param name="p4" />
    [DllImport(Dll)]
    public static extern int mpfr_can_round(IntPtr p0, int p1, mpfr_rnd_t p2, mpfr_rnd_t p3, int p4);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_min_prec(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_exp(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_set_exp(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_prec(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_set_prec(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_set_prec_raw(IntPtr p0, int p1);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern void mpfr_set_default_prec(int p0);


    [DllImport(Dll)]
    public static extern int mpfr_get_default_prec();

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_d(IntPtr p0, double p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_flt(IntPtr p0, float p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_ld(IntPtr p0, double p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_z(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_z_2exp(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_set_nan(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_set_inf(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_set_zero(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_f(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cmp_f(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_f(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_si(IntPtr p0, int p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_ui(IntPtr p0, uint p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_si_2exp(IntPtr p0, int p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_ui_2exp(IntPtr p0, uint p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_q(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_q(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_q(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add_q(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub_q(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cmp_q(IntPtr p0, IntPtr p1);

    /// <param name="q">(<see cref="mpq_ptr" />) </param>
    /// <param name="f">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_get_q(IntPtr q, IntPtr f);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set_str(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_init_set_str(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int mpfr_set4(IntPtr p0, IntPtr p1, mpfr_rnd_t p2, int p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_abs(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_set(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_neg(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_signbit(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_setsign(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_copysign(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_z_2exp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern float mpfr_get_flt(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern double mpfr_get_d(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern double mpfr_get_ld(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double mpfr_get_d1(IntPtr p0);

    /// <param name="p0">(<see cref="int*" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern double mpfr_get_d_2exp(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="int*" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern double mpfr_get_ld_2exp(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_exp_t*" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_frexp(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_si(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern uint mpfr_get_ui(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0" />
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern nint mpfr_get_str_ndigits(int p0, int p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1">(<see cref="mpfr_exp_t*" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p5">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr mpfr_get_str(IntPtr p0, IntPtr p1, int p2, nint p3, IntPtr p4, mpfr_rnd_t p5);

    /// <param name="z">(<see cref="mpz_ptr" />) </param>
    /// <param name="f">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_get_z(IntPtr z, IntPtr f, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_free_str(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_urandom(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p2">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_grandom(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_nrandom(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_erandom(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_urandomb(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_nextabove(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_nextbelow(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_nexttoward(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_printf(IntPtr p0);

    /// <param name="p0">(<see cref="byte**" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_asprintf(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sprintf(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_snprintf(IntPtr p0, nint p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_pow(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_pow_si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_pow_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ui_pow_ui(IntPtr p0, uint p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ui_pow(IntPtr p0, uint p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_pow_z(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sqrt(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sqrt_ui(IntPtr p0, uint p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rec_sqrt(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ui_sub(IntPtr p0, uint p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ui_div(IntPtr p0, uint p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add_si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub_si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_si_sub(IntPtr p0, int p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_si_div(IntPtr p0, int p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add_d(IntPtr p0, IntPtr p1, double p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub_d(IntPtr p0, IntPtr p1, double p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_d_sub(IntPtr p0, double p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_d(IntPtr p0, IntPtr p1, double p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_d(IntPtr p0, IntPtr p1, double p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_d_div(IntPtr p0, double p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sqr(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_const_pi(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_const_log2(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_const_euler(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_const_catalan(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_agm(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_log(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_log2(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_log10(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_log1p(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_log_ui(IntPtr p0, uint p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_exp(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_exp2(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_exp10(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_expm1(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_eint(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_li2(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cmp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp3(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_ld(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_ui_2exp(IntPtr p0, uint p1, int p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int mpfr_cmp_si_2exp(IntPtr p0, int p1, int p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cmpabs(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_cmpabs_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_reldiff(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int mpfr_eq(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sgn(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_2exp(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_2exp(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_2ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_2ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_2si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_2si(IntPtr p0, IntPtr p1, int p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_roundeven(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_round(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_trunc(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ceil(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_floor(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint_roundeven(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint_round(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint_trunc(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint_ceil(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rint_floor(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_frac(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_modf(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="int*" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_remquo(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, mpfr_rnd_t p4);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_remainder(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fmod(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="int*" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fmodquo(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, mpfr_rnd_t p4);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_ulong_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_slong_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_uint_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_sint_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_ushort_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_sshort_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_uintmax_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fits_intmax_p(IntPtr p0, mpfr_rnd_t p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void mpfr_extract(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_swap(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_dump(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_nan_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_inf_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_number_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_integer_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_zero_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_regular_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_greater_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_greaterequal_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_less_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_lessequal_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_lessgreater_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_equal_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_unordered_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_atanh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_acosh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_asinh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cosh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sinh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_tanh(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sinh_cosh(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sech(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_csch(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_coth(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_acos(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_asin(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_atan(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sin(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sin_cos(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cos(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_tan(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_atan2(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sec(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_csc(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cot(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_hypot(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_erf(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_erfc(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cbrt(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_root(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_rootn_ui(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_gamma(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_gamma_inc(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_beta(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_lngamma(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="int*" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_lgamma(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_digamma(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_zeta(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_zeta_ui(IntPtr p0, uint p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fac_ui(IntPtr p0, uint p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_j0(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_j1(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_jn(IntPtr p0, int p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_y0(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_y1(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_yn(IntPtr p0, int p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_ai(IntPtr p0, IntPtr p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_min(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_max(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_dim(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_mul_z(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_div_z(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_add_z(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sub_z(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_z_sub(IntPtr p0, IntPtr p1, IntPtr p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_cmp_z(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fma(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, mpfr_rnd_t p4);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fms(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, mpfr_rnd_t p4);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p5">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fmma(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, mpfr_rnd_t p5);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p5">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_fmms(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4, mpfr_rnd_t p5);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr*" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_sum(IntPtr p0, IntPtr p1, uint p2, mpfr_rnd_t p3);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_ptr*" />) </param>
    /// <param name="p2">(<see cref="mpfr_ptr*" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_dot(IntPtr p0, IntPtr p1, IntPtr p2, uint p3, mpfr_rnd_t p4);


    [DllImport(Dll)]
    public static extern void mpfr_free_cache();

    /// <param name="p0">(<see cref="mpfr_free_cache_t" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_free_cache2(mpfr_free_cache_t p0);


    [DllImport(Dll)]
    public static extern void mpfr_free_pool();


    [DllImport(Dll)]
    public static extern int mpfr_mp_memory_cleanup();

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_subnormalize(IntPtr p0, int p1, mpfr_rnd_t p2);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2">(<see cref="byte**" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mpfr_rnd_t" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_strtofr(IntPtr p0, IntPtr p1, IntPtr p2, int p3, mpfr_rnd_t p4);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_round_nearest_away_begin(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int mpfr_round_nearest_away_end(IntPtr p0, int p1);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern nint mpfr_custom_get_size(int p0);

    /// <param name="p0">(<see cref="void*" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void mpfr_custom_init(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr mpfr_custom_get_significand(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_custom_get_exp(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1">(<see cref="void*" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_custom_move(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpfr_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4">(<see cref="void*" />) </param>
    [DllImport(Dll)]
    public static extern void mpfr_custom_init_set(IntPtr p0, int p1, int p2, int p3, IntPtr p4);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_custom_get_kind(IntPtr p0);

    /// <param name="p0">(<see cref="mpfr_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpfr_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int mpfr_total_order_p(IntPtr p0, IntPtr p1);
}
