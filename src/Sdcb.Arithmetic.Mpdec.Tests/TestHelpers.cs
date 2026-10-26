using System.Runtime.InteropServices;
using System.Text;
using Sdcb.Arithmetic.Mpdec;

namespace Sdcb.Arithmetic.Mpdec.Tests;

internal static unsafe class H
{
    public static mpd_context_t Context(long prec, int round = MPD_ROUND_HALF_EVEN)
    {
        mpd_context_t ctx;
        mpd_maxcontext(&ctx);
        ctx.prec = prec;
        ctx.round = round;
        return ctx;
    }

    public static mpd_t* New(string value, mpd_context_t* ctx)
    {
        mpd_t* d = mpd_qnew();
        Assert.True(d != null);
        uint status = 0;
        fixed (byte* s = Utf8(value))
        {
            mpd_qset_string(d, s, ctx, &status);
        }
        Assert.Equal(0u, status & MPD_Conversion_syntax);
        return d;
    }

    public static string Sci(mpd_t* d)
    {
        byte* s = mpd_to_sci(d, fmt: 1); // 1: uppercase exponent character
        try
        {
            return Marshal.PtrToStringUTF8((IntPtr)s)!;
        }
        finally
        {
            mpd_free(s);
        }
    }

    public static string Str(byte* s) => Marshal.PtrToStringUTF8((IntPtr)s)!;

    public static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s + "\0");
}
