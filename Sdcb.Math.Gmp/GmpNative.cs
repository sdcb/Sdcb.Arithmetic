using System;
using System.Runtime.InteropServices;

namespace Sdcb.Math.Gmp;

public static class GmpNative
{
    public static uint LimbBitSize => (uint)IntPtr.Size * 8;

    internal const string Dll = "gmp";

    // NOTE: code is all auto generated below

    /// <param name="p0">(<see cref="void*(size_t)" />) </param>
    /// <param name="p1">(<see cref="void*(void*,size_t,size_t)" />) </param>
    /// <param name="p2">(<see cref="void(void*,size_t)" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_set_memory_functions(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="void*(size_t)*" />) </param>
    /// <param name="p1">(<see cref="void*(void*,size_t,size_t)*" />) </param>
    /// <param name="p2">(<see cref="void(void*,size_t)*" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_get_memory_functions(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1">(<see cref="gmp_randalg_t" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randinit(IntPtr p0, gmp_randalg_t p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randinit_default(IntPtr p0);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmp_randinit_lc_2exp(IntPtr p0, IntPtr p1, uint p2, uint p3);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmp_randinit_lc_2exp_size(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randinit_mt(IntPtr p0);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1">(<see cref="__gmp_randstate_struct*" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randinit_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randseed(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmp_randseed_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    [DllImport(Dll)]
    public static extern void __gmp_randclear(IntPtr p0);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmp_urandomb_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmp_urandomm_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="byte**" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_asprintf(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_printf(IntPtr p0);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_snprintf(IntPtr p0, nint p1, IntPtr p2);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_sprintf(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_scanf(IntPtr p0);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    [DllImport(Dll)]
    public static extern int __gmp_sscanf(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_realloc(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_add(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_add_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_addmul(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_addmul_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_and(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_array_init(IntPtr p0, int p1, int p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_bin_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_bin_uiui(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_cdiv_q(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_cdiv_q_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_cdiv_q_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_cdiv_qr(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern uint __gmpz_cdiv_qr_ui(IntPtr p0, IntPtr p1, IntPtr p2, uint p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_cdiv_r(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_cdiv_r_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_cdiv_r_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpz_cdiv_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_clear(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_clears(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_clrbit(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_cmp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_cmp_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_cmp_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_cmp_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_cmpabs(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_cmpabs_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_cmpabs_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_com(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_combit(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_congruent_p(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpz_congruent_2exp_p(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpz_congruent_ui_p(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_divexact(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_divexact_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_divisible_p(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_divisible_ui_p(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_divisible_2exp_p(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_dump(IntPtr p0);

    /// <param name="p0">(<see cref="void*" />) </param>
    /// <param name="p1">(<see cref="size_t*" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4" />
    /// <param name="p5" />
    /// <param name="p6">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_export(IntPtr p0, IntPtr p1, int p2, nint p3, int p4, nint p5, IntPtr p6);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_fac_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_2fac_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_mfac_uiui(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_primorial_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_fdiv_q(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_fdiv_q_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_fdiv_q_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_fdiv_qr(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern uint __gmpz_fdiv_qr_ui(IntPtr p0, IntPtr p1, IntPtr p2, uint p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_fdiv_r(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_fdiv_r_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_fdiv_r_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpz_fdiv_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_fib_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_fib2_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_sint_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_slong_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_sshort_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_gcd(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_gcd_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_ptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p4">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_gcdext(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, IntPtr p4);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double __gmpz_get_d(IntPtr p0);

    /// <param name="p0">(<see cref="int*" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double __gmpz_get_d_2exp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_get_si(IntPtr p0);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_get_str(IntPtr p0, int p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpz_hamdist(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4" />
    /// <param name="p5" />
    /// <param name="p6">(<see cref="void*" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_import(IntPtr p0, nint p1, int p2, nint p3, int p4, nint p5, IntPtr p6);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_init(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_init2(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_inits(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_init_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_init_set_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_init_set_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpz_init_set_str(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_init_set_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_invert(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_ior(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_jacobi(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_kronecker_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_kronecker_ui(IntPtr p0, uint p1);

    /// <param name="p0" />
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_si_kronecker(int p0, IntPtr p1);

    /// <param name="p0" />
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_ui_kronecker(uint p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_lcm(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_lcm_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_lucnum_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_lucnum2_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_millerrabin(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_mod(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_mul(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_mul_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_mul_si(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_mul_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_nextprime(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_perfect_power_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_pow_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_powm(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_powm_sec(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_powm_ui(IntPtr p0, IntPtr p1, uint p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_probab_prime_p(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_random(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_random2(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_realloc2(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpz_remove(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpz_root(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpz_rootrem(IntPtr p0, IntPtr p1, IntPtr p2, uint p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_rrandomb(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpz_scan0(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpz_scan1(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_set_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_set_f(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_set_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpz_set_str(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_set_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_setbit(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern nint __gmpz_sizeinbase(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_sqrt(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_sqrtrem(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_sub(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_sub_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_ui_sub(IntPtr p0, uint p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_submul(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_submul_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_swap(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpz_tdiv_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_tdiv_q(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_tdiv_q_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_tdiv_q_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_tdiv_qr(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_ptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern uint __gmpz_tdiv_qr_ui(IntPtr p0, IntPtr p1, IntPtr p2, uint p3);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_tdiv_r(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_tdiv_r_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpz_tdiv_r_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpz_tstbit(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_ui_pow_ui(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpz_urandomb(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_urandomm(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_xor(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_limbs_read(IntPtr p0);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_limbs_write(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_limbs_modify(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpz_limbs_finish(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern IntPtr __gmpz_roinit_n(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_add(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_canonicalize(IntPtr p0);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_clear(IntPtr p0);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_clears(IntPtr p0);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpq_cmp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpq_cmp_si(IntPtr p0, int p1, uint p2);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpq_cmp_ui(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpq_cmp_z(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_div(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpq_div_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpq_equal(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_get_num(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpz_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_get_den(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double __gmpq_get_d(IntPtr p0);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr __gmpq_get_str(IntPtr p0, int p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_init(IntPtr p0);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_inits(IntPtr p0);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_inv(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_mul(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpq_mul_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpq_set_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_set_den(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_set_f(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_set_num(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpq_set_si(IntPtr p0, int p1, uint p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpq_set_str(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpq_set_ui(IntPtr p0, uint p1, uint p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_set_z(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_sub(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpq_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_swap(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_abs(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_add(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_add_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_ceil(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_clear(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_clears(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_cmp(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_cmp_z(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpf_cmp_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpf_cmp_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpf_cmp_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_div(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_div_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_div_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_dump(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpf_eq(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_sint_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_slong_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_sshort_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_uint_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_ulong_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_fits_ushort_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_floor(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double __gmpf_get_d(IntPtr p0);

    /// <param name="p0">(<see cref="int*" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern double __gmpf_get_d_2exp(IntPtr p0, IntPtr p1);


    [DllImport(Dll)]
    public static extern uint __gmpf_get_default_prec();

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpf_get_prec(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_get_si(IntPtr p0);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1">(<see cref="mp_exp_t*" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern IntPtr __gmpf_get_str(IntPtr p0, IntPtr p1, int p2, nint p3, IntPtr p4);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpf_get_ui(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_init(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_init2(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_inits(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_init_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_init_set_d(IntPtr p0, double p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_init_set_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpf_init_set_str(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_init_set_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpf_integer_p(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_mul(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_mul_2exp(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_mul_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_neg(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_pow_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_random2(IntPtr p0, int p1, int p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_reldiff(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_set(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_d(IntPtr p0, double p1);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_default_prec(uint p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_prec(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_prec_raw(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_set_q(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_si(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpf_set_str(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_set_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_set_z(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern nint __gmpf_size(IntPtr p0);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_sqrt(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpf_sqrt_ui(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_sub(IntPtr p0, IntPtr p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_sub_ui(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_swap(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_trunc(IntPtr p0, IntPtr p1);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_ui_div(IntPtr p0, uint p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mpf_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpf_ui_sub(IntPtr p0, uint p1, IntPtr p2);

    /// <param name="p0">(<see cref="mpf_t" />) </param>
    /// <param name="p1">(<see cref="gmp_randstate_t" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpf_urandomb(IntPtr p0, IntPtr p1, uint p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_add_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_addmul_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern void __gmpn_divexact_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_divexact_by3c(IntPtr p0, IntPtr p1, int p2, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mp_ptr" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mp_srcptr" />) </param>
    /// <param name="p5" />
    [DllImport(Dll)]
    public static extern int __gmpn_divrem(IntPtr p0, int p1, IntPtr p2, int p3, IntPtr p4, int p5);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_divrem_1(IntPtr p0, int p1, IntPtr p2, int p3, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mp_ptr" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mp_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_divrem_2(IntPtr p0, int p1, IntPtr p2, int p3, IntPtr p4);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_size_t*" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_div_qr_1(IntPtr p0, IntPtr p1, IntPtr p2, int p3, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mp_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_div_qr_2(IntPtr p0, IntPtr p1, IntPtr p2, int p3, IntPtr p4);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_ptr" />) </param>
    /// <param name="p4" />
    [DllImport(Dll)]
    public static extern int __gmpn_gcd(IntPtr p0, IntPtr p1, int p2, IntPtr p3, int p4);


    [DllImport(Dll)]
    public static extern int __gmpn_gcd_11();

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_gcd_1(IntPtr p0, int p1, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_limb_signed_t*" />) </param>
    /// <param name="p1">(<see cref="mp_limb_signed_t*" />) </param>
    /// <param name="mp_limb_t" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_gcdext_1(IntPtr p0, IntPtr p1, int mp_limb_t, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2">(<see cref="mp_size_t*" />) </param>
    /// <param name="p3">(<see cref="mp_ptr" />) </param>
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_ptr" />) </param>
    /// <param name="p6" />
    [DllImport(Dll)]
    public static extern int __gmpn_gcdext(IntPtr p0, IntPtr p1, IntPtr p2, IntPtr p3, int p4, IntPtr p5, int p6);

    /// <param name="p0">(<see cref="byte*" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mp_ptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern nint __gmpn_get_str(IntPtr p0, int p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern uint __gmpn_hamdist(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_lshift(IntPtr p0, IntPtr p1, int p2, uint p3);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_mod_1(IntPtr p0, int p1, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_srcptr" />) </param>
    /// <param name="p4" />
    [DllImport(Dll)]
    public static extern int __gmpn_mul(IntPtr p0, IntPtr p1, int p2, IntPtr p3, int p4);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_mul_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_mul_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpn_sqr(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpn_com(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpn_perfect_square_p(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpn_perfect_power_p(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpn_popcount(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    /// <param name="p4">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_pow_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t, IntPtr p4);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="mp_limb_t" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_preinv_mod_1(IntPtr p0, int p1, int mp_limb_t, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpn_random(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpn_random2(IntPtr p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_rshift(IntPtr p0, IntPtr p1, int p2, uint p3);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpn_scan0(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern uint __gmpn_scan1(IntPtr p0, uint p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="byte*" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_set_str(IntPtr p0, IntPtr p1, nint p2, int p3);

    /// <param name="p0">(<see cref="mp_srcptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern nint __gmpn_sizeinbase(IntPtr p0, int p1, int p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_sqrtrem(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern int __gmpn_sub_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    [DllImport(Dll)]
    public static extern int __gmpn_submul_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_srcptr" />) </param>
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_srcptr" />) </param>
    /// <param name="p6" />
    [DllImport(Dll)]
    public static extern void __gmpn_tdiv_qr(IntPtr p0, IntPtr p1, int p2, IntPtr p3, int p4, IntPtr p5, int p6);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_and_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_andn_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_nand_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_ior_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_iorn_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_nior_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_xor_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    [DllImport(Dll)]
    public static extern void __gmpn_xnor_n(IntPtr p0, IntPtr p1, IntPtr p2, int p3);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpn_copyi(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern void __gmpn_copyd(IntPtr p0, IntPtr p1, int p2);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern void __gmpn_zero(IntPtr p0, int p1);


    [DllImport(Dll)]
    public static extern int __gmpn_cnd_add_n();


    [DllImport(Dll)]
    public static extern int __gmpn_cnd_sub_n();

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    /// <param name="p4">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_sec_add_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t, IntPtr p4);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_add_1_itch(int p0);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="mp_limb_t" />
    /// <param name="p4">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_sec_sub_1(IntPtr p0, IntPtr p1, int p2, int mp_limb_t, IntPtr p4);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_sub_1_itch(int p0);


    [DllImport(Dll)]
    public static extern void __gmpn_cnd_swap();

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_srcptr" />) </param>
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpn_sec_mul(IntPtr p0, IntPtr p1, int p2, IntPtr p3, int p4, IntPtr p5);

    /// <param name="p0" />
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_mul_itch(int p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpn_sec_sqr(IntPtr p0, IntPtr p1, int p2, IntPtr p3);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_sqr_itch(int p0);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_srcptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_srcptr" />) </param>
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_srcptr" />) </param>
    /// <param name="p6" />
    /// <param name="p7">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpn_sec_powm(IntPtr p0, IntPtr p1, int p2, IntPtr p3, uint p4, IntPtr p5, int p6, IntPtr p7);

    /// <param name="p0" />
    /// <param name="p1" />
    /// <param name="p2" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_powm_itch(int p0, uint p1, int p2);

    /// <param name="p0">(<see cref="mp_size_t*" />) </param>
    /// <param name="p1">(<see cref="mp_size_t*" />) </param>
    /// <param name="p2" />
    /// <param name="p3" />
    /// <param name="p4" />
    [DllImport(Dll)]
    public static extern void __gmpn_sec_tabselect(IntPtr p0, IntPtr p1, int p2, int p3, int p4);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2" />
    /// <param name="p3">(<see cref="mp_srcptr" />) </param>
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_sec_div_qr(IntPtr p0, IntPtr p1, int p2, IntPtr p3, int p4, IntPtr p5);

    /// <param name="p0" />
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_div_qr_itch(int p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1" />
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    /// <param name="p4">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpn_sec_div_r(IntPtr p0, int p1, IntPtr p2, int p3, IntPtr p4);

    /// <param name="p0" />
    /// <param name="p1" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_div_r_itch(int p0, int p1);

    /// <param name="p0">(<see cref="mp_ptr" />) </param>
    /// <param name="p1">(<see cref="mp_ptr" />) </param>
    /// <param name="p2">(<see cref="mp_srcptr" />) </param>
    /// <param name="p3" />
    /// <param name="p4" />
    /// <param name="p5">(<see cref="mp_ptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpn_sec_invert(IntPtr p0, IntPtr p1, IntPtr p2, int p3, uint p4, IntPtr p5);

    /// <param name="p0" />
    [DllImport(Dll)]
    public static extern int __gmpn_sec_invert_itch(int p0);

    /// <param name="__gmp_w">(<see cref="mpz_ptr" />) </param>
    /// <param name="__gmp_u">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_abs(IntPtr __gmp_w, IntPtr __gmp_u);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_uint_p(IntPtr __gmp_z);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_ulong_p(IntPtr __gmp_z);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_fits_ushort_p(IntPtr __gmp_z);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpz_get_ui(IntPtr __gmp_z);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    /// <param name="__gmp_n" />
    [DllImport(Dll)]
    public static extern int __gmpz_getlimbn(IntPtr __gmp_z, int __gmp_n);

    /// <param name="__gmp_w">(<see cref="mpz_ptr" />) </param>
    /// <param name="__gmp_u">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_neg(IntPtr __gmp_w, IntPtr __gmp_u);

    /// <param name="__gmp_a">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern int __gmpz_perfect_square_p(IntPtr __gmp_a);

    /// <param name="__gmp_u">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern uint __gmpz_popcount(IntPtr __gmp_u);

    /// <param name="__gmp_w">(<see cref="mpz_ptr" />) </param>
    /// <param name="__gmp_u">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpz_set_q(IntPtr __gmp_w, IntPtr __gmp_u);

    /// <param name="__gmp_z">(<see cref="mpz_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern nint __gmpz_size(IntPtr __gmp_z);

    /// <param name="__gmp_w">(<see cref="mpq_ptr" />) </param>
    /// <param name="__gmp_u">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_abs(IntPtr __gmp_w, IntPtr __gmp_u);

    /// <param name="__gmp_w">(<see cref="mpq_ptr" />) </param>
    /// <param name="__gmp_u">(<see cref="mpq_srcptr" />) </param>
    [DllImport(Dll)]
    public static extern void __gmpq_neg(IntPtr __gmp_w, IntPtr __gmp_u);

    /// <param name="__gmp_wp">(<see cref="mp_ptr" />) </param>
    /// <param name="__gmp_xp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_xsize" />
    /// <param name="__gmp_yp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_ysize" />
    [DllImport(Dll)]
    public static extern int __gmpn_add(IntPtr __gmp_wp, IntPtr __gmp_xp, int __gmp_xsize, IntPtr __gmp_yp, int __gmp_ysize);

    /// <param name="__gmp_dst">(<see cref="mp_ptr" />) </param>
    /// <param name="__gmp_src">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_size" />
    /// <param name="__gmp_n" />
    [DllImport(Dll)]
    public static extern int __gmpn_add_1(IntPtr __gmp_dst, IntPtr __gmp_src, int __gmp_size, int __gmp_n);

    /// <param name="__gmp_xp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_yp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_size" />
    [DllImport(Dll)]
    public static extern int __gmpn_cmp(IntPtr __gmp_xp, IntPtr __gmp_yp, int __gmp_size);

    /// <param name="__gmp_p">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_n" />
    [DllImport(Dll)]
    public static extern int __gmpn_zero_p(IntPtr __gmp_p, int __gmp_n);

    /// <param name="__gmp_wp">(<see cref="mp_ptr" />) </param>
    /// <param name="__gmp_xp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_xsize" />
    /// <param name="__gmp_yp">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_ysize" />
    [DllImport(Dll)]
    public static extern int __gmpn_sub(IntPtr __gmp_wp, IntPtr __gmp_xp, int __gmp_xsize, IntPtr __gmp_yp, int __gmp_ysize);

    /// <param name="__gmp_dst">(<see cref="mp_ptr" />) </param>
    /// <param name="__gmp_src">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_size" />
    /// <param name="__gmp_n" />
    [DllImport(Dll)]
    public static extern int __gmpn_sub_1(IntPtr __gmp_dst, IntPtr __gmp_src, int __gmp_size, int __gmp_n);

    /// <param name="__gmp_rp">(<see cref="mp_ptr" />) </param>
    /// <param name="__gmp_up">(<see cref="mp_srcptr" />) </param>
    /// <param name="__gmp_n" />
    [DllImport(Dll)]
    public static extern int __gmpn_neg(IntPtr __gmp_rp, IntPtr __gmp_up, int __gmp_n);
}