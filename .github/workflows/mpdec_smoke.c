#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mpdecimal.h>

static int check(const char *name, const char *actual, const char *expected)
{
    if (strcmp(actual, expected) != 0) {
        fprintf(stderr, "FAIL %s: expected %s, got %s\n", name, expected, actual);
        return 1;
    }
    printf("OK   %s = %s\n", name, actual);
    return 0;
}

int main(void)
{
    mpd_context_t ctx;
    uint32_t status = 0;
    int failed = 0;
    char *s;

    printf("libmpdec version: %s\n", mpd_version());
    failed |= strcmp(mpd_version(), MPD_VERSION) != 0;

    mpd_maxcontext(&ctx);
    ctx.prec = 50;
    ctx.round = MPD_ROUND_HALF_EVEN;

    mpd_t *a = mpd_qnew();
    mpd_t *b = mpd_qnew();
    mpd_t *r = mpd_qnew();

    mpd_qset_string(a, "1", &ctx, &status);
    mpd_qset_string(b, "7", &ctx, &status);
    mpd_qdiv(r, a, b, &ctx, &status);
    s = mpd_to_sci(r, 0);
    failed |= check("1/7 (prec=50)", s, "0.14285714285714285714285714285714285714285714285714");
    mpd_free(s);

    mpd_qset_string(a, "0.1", &ctx, &status);
    mpd_qset_string(b, "0.2", &ctx, &status);
    mpd_qadd(r, a, b, &ctx, &status);
    s = mpd_to_sci(r, 0);
    failed |= check("0.1+0.2", s, "0.3");
    mpd_free(s);

    status = 0;
    mpd_qset_string(a, "2", &ctx, &status);
    mpd_qsqrt(r, a, &ctx, &status);
    s = mpd_to_sci(r, 0);
    failed |= check("sqrt(2) (prec=50)", s, "1.4142135623730950488016887242096980785696718753769");
    mpd_free(s);
    failed |= !(status & MPD_Inexact);

    mpd_del(a);
    mpd_del(b);
    mpd_del(r);

    if (failed) {
        fprintf(stderr, "libmpdec smoke test FAILED\n");
        return 1;
    }
    printf("libmpdec smoke test passed\n");
    return 0;
}
