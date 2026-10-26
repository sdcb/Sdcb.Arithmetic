using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpdec.Tests;

public unsafe class ConversionTests
{
    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    [InlineData(0L)]
    [InlineData(-1234567890123456789L)]
    public void Int64RoundTrip(long value)
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* d = mpd_qnew();
        uint status = 0;
        mpd_qset_i64(d, value, &ctx, &status);
        Assert.Equal(value.ToString(), H.Sci(d));
        Assert.Equal(value, mpd_qget_i64(d, &status));
        Assert.Equal(0u, status);
        mpd_del(d);
    }

    [Fact]
    public void UInt64Overflow()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* d = H.New("18446744073709551616", &ctx);
        uint status = 0;
        mpd_qget_u64(d, &status);
        Assert.Equal(MPD_Invalid_operation, status);
        mpd_del(d);
    }

    [Fact]
    public void ScientificAndEngineeringStrings()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* d = H.New("1.23E+7", &ctx);
        Assert.Equal("1.23E+7", H.Sci(d));
        byte* eng = mpd_to_eng(d, 1);
        Assert.Equal("12.3E+6", H.Str(eng));
        mpd_free(eng);

        byte* lower = null;
        long len = mpd_to_sci_size(&lower, d, 0);
        Assert.Equal("1.23e+7", H.Str(lower));
        Assert.Equal(7, len);
        mpd_free(lower);
        mpd_del(d);
    }

    [Fact]
    public void Format()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* d = H.New("1234567.891", &ctx);
        uint status = 0;
        fixed (byte* fmt = H.Utf8(",.2f"))
        {
            byte* s = mpd_qformat(d, fmt, &ctx, &status);
            Assert.Equal("1,234,567.89", H.Str(s));
            mpd_free(s);
        }
        mpd_del(d);
    }

    [Fact]
    public void Uint128TripleRoundTrip()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* d = H.New("-340282366920938463463374607431768211455E-10", &ctx);
        mpd_uint128_triple_t t = mpd_as_uint128_triple(d);
        Assert.Equal(MPD_TRIPLE_NORMAL, t.tag);
        Assert.Equal(1, t.sign);
        Assert.Equal(ulong.MaxValue, t.hi);
        Assert.Equal(ulong.MaxValue, t.lo);
        Assert.Equal(-10, t.exp);

        mpd_t* back = mpd_qnew();
        uint status = 0;
        Assert.Equal(0, mpd_from_uint128_triple(back, &t, &status));
        Assert.Equal(H.Sci(d), H.Sci(back));
        mpd_del(d); mpd_del(back);
    }

    [Fact]
    public void ImportExportBase2Pow32()
    {
        mpd_context_t ctx = H.Context(MPD_MAX_PREC);
        uint[] words = [0xDEADBEEF, 0x01234567, 0x89ABCDEF, 0x7];
        mpd_t* d = mpd_qnew();
        uint status = 0;
        fixed (uint* p = words)
        {
            mpd_qimport_u32(d, p, (nuint)words.Length, MPD_POS, 1U << 31, &ctx, &status);
        }
        // base 2^31 because base must be <= UINT32_MAX; verify with System.Numerics.BigInteger
        System.Numerics.BigInteger expected = 0;
        for (int i = words.Length - 1; i >= 0; --i) expected = expected * (1L << 31) + words[i];
        Assert.Equal(expected.ToString(), H.Sci(d));

        uint* outWords = null;
        nuint n = mpd_qexport_u32(&outWords, 0, 1U << 31, d, &status);
        uint[] exported = new ReadOnlySpan<uint>(outWords, (int)n).ToArray();
        mpd_free(outWords);

        System.Numerics.BigInteger roundTrip = 0;
        for (int i = exported.Length - 1; i >= 0; --i) roundTrip = roundTrip * (1L << 31) + exported[i];
        Assert.Equal(expected, roundTrip);
        Assert.Equal(0u, status);
        mpd_del(d);
    }

    [Fact]
    public void StackAllocatedStaticDecimal()
    {
        // mpd_t with static storage and static data (no malloc), like MPD_NEW_STATIC in C
        ulong* data = stackalloc ulong[4];
        mpd_t d = new() { flags = MPD_STATIC | MPD_STATIC_DATA, alloc = 4, data = data };
        mpd_context_t ctx = H.Context(50);
        uint status = 0;
        mpd_qset_u64(&d, ulong.MaxValue, &ctx, &status);
        Assert.Equal(ulong.MaxValue.ToString(), H.Sci(&d));
        Assert.NotEqual(0, mpd_isstatic(&d));
        Assert.NotEqual(0, mpd_isstatic_data(&d));
        Assert.Equal(20, d.digits);
        mpd_del(&d); // no-op for static data
    }

    [Fact]
    public void SignallingFunctionWithCustomTrapHandler()
    {
        delegate* unmanaged[Cdecl]<mpd_context_t*, void> old = mpd_traphandler;
        try
        {
            mpd_traphandler = &IgnoreTrap;
            mpd_context_t ctx = H.Context(28);
            ctx.traps = MPD_Traps;
            mpd_t* a = H.New("1", &ctx), z = H.New("0", &ctx), r = mpd_qnew();
            mpd_div(r, a, z, &ctx);
            Assert.Equal(MPD_Division_by_zero, ctx.status & MPD_Division_by_zero);
            Assert.Equal(MPD_Division_by_zero, ctx.newtrap);
            mpd_del(a); mpd_del(z); mpd_del(r);
        }
        finally
        {
            mpd_traphandler = old;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static void IgnoreTrap(mpd_context_t* ctx) { }
}
