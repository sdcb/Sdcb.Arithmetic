namespace Sdcb.Arithmetic.Mpdec.Tests;

public unsafe class ArithmeticTests
{
    [Fact]
    public void DivideOneBySeven()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* a = H.New("1", &ctx), b = H.New("7", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qdiv(r, a, b, &ctx, &status);
        Assert.Equal("0.14285714285714285714285714285714285714285714285714", H.Sci(r));
        Assert.Equal(MPD_Inexact | MPD_Rounded, status);
        mpd_del(a); mpd_del(b); mpd_del(r);
    }

    [Fact]
    public void DecimalAdditionIsExact()
    {
        mpd_context_t ctx = H.Context(28);
        mpd_t* a = H.New("0.1", &ctx), b = H.New("0.2", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qadd(r, a, b, &ctx, &status);
        Assert.Equal("0.3", H.Sci(r));
        Assert.Equal(0u, status);
        mpd_del(a); mpd_del(b); mpd_del(r);
    }

    [Fact]
    public void SqrtAndExp()
    {
        mpd_context_t ctx = H.Context(50);
        mpd_t* two = H.New("2", &ctx), one = H.New("1", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qsqrt(r, two, &ctx, &status);
        Assert.Equal("1.4142135623730950488016887242096980785696718753769", H.Sci(r));
        mpd_qexp(r, one, &ctx, &status);
        Assert.Equal("2.7182818284590452353602874713526624977572470937000", H.Sci(r));
        mpd_del(two); mpd_del(one); mpd_del(r);
    }

    [Fact]
    public void LargeMultiplicationUsesFullPrecision()
    {
        mpd_context_t ctx = H.Context(MPD_MAX_PREC);
        string nines = new('9', 5000);
        mpd_t* a = H.New(nines, &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qmul(r, a, a, &ctx, &status);
        // (10^n - 1)^2 = 10^2n - 2*10^n + 1 = 99..9800..01
        Assert.Equal(new string('9', 4999) + "8" + new string('0', 4999) + "1", H.Sci(r));
        Assert.Equal(0u, status);
        Assert.Equal(10000, r->digits);
        mpd_del(a); mpd_del(r);
    }

    [Theory]
    [InlineData(MPD_ROUND_UP, "3", "-3")]
    [InlineData(MPD_ROUND_DOWN, "2", "-2")]
    [InlineData(MPD_ROUND_CEILING, "3", "-2")]
    [InlineData(MPD_ROUND_FLOOR, "2", "-3")]
    [InlineData(MPD_ROUND_HALF_UP, "3", "-3")]
    [InlineData(MPD_ROUND_HALF_DOWN, "2", "-2")]
    [InlineData(MPD_ROUND_HALF_EVEN, "2", "-2")]
    [InlineData(MPD_ROUND_05UP, "2", "-2")]
    public void RoundingModes(int round, string positive, string negative)
    {
        mpd_context_t ctx = H.Context(50, round);
        mpd_t* p = H.New("2.5", &ctx), n = H.New("-2.5", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qround_to_int(r, p, &ctx, &status);
        Assert.Equal(positive, H.Sci(r));
        mpd_qround_to_int(r, n, &ctx, &status);
        Assert.Equal(negative, H.Sci(r));
        mpd_del(p); mpd_del(n); mpd_del(r);
    }

    [Fact]
    public void QuantizeHalfUp()
    {
        mpd_context_t ctx = H.Context(50, MPD_ROUND_HALF_UP);
        mpd_t* a = H.New("123.455", &ctx), q = H.New("0.01", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qquantize(r, a, q, &ctx, &status);
        Assert.Equal("123.46", H.Sci(r));
        Assert.Equal(-2, r->exp);
        mpd_del(a); mpd_del(q); mpd_del(r);
    }

    [Fact]
    public void DivisionByZeroSetsStatusQuietly()
    {
        mpd_context_t ctx = H.Context(28);
        mpd_t* a = H.New("1", &ctx), z = H.New("0", &ctx), r = mpd_qnew();
        uint status = 0;
        mpd_qdiv(r, a, z, &ctx, &status);
        Assert.Equal(MPD_Division_by_zero, status);
        Assert.NotEqual(0, mpd_isinfinite(r));
        Assert.Equal("Infinity", H.Sci(r));
        mpd_del(a); mpd_del(z); mpd_del(r);
    }

    [Fact]
    public void Compare()
    {
        mpd_context_t ctx = H.Context(28);
        mpd_t* a = H.New("1.0", &ctx), b = H.New("1.00", &ctx), c = H.New("1.01", &ctx);
        uint status = 0;
        Assert.Equal(0, mpd_qcmp(a, b, &status));
        Assert.Equal(-1, mpd_qcmp(a, c, &status));
        Assert.Equal(1, mpd_cmp_total(a, b)); // total order distinguishes exponents
        Assert.Equal(0, mpd_same_quantum(a, b));
        mpd_del(a); mpd_del(b); mpd_del(c);
    }
}
