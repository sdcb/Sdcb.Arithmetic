using System.Reflection;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpdec.Tests;

public unsafe class RawApiTests(ITestOutputHelper console)
{
    [Fact]
    public void Version()
    {
        string version = H.Str(mpd_version());
        console.WriteLine($"libmpdec {version}, loaded from candidates: {string.Join(", ", MpdecNativeLoader.CandidateNames)}");
        Assert.Equal(MPD_VERSION, version);
    }

    [Fact]
    public void StructLayoutsMatchNative()
    {
        Assert.Equal(48, sizeof(mpd_t));
        Assert.Equal(48, sizeof(mpd_context_t));
        Assert.Equal(32, sizeof(mpd_uint128_triple_t));
        Assert.Equal(56, sizeof(mpd_spec_t));
    }

    [Fact]
    public void AllEntryPointsAreExported()
    {
        MethodInfo[] imports = typeof(MpdecLib)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<DllImportAttribute>() != null)
            .ToArray();
        Assert.True(imports.Length > 300, $"only {imports.Length} imports found");

        string[] missing = imports
            .Select(m => m.GetCustomAttribute<DllImportAttribute>()!.EntryPoint ?? m.Name)
            .Where(name => !NativeLibrary.TryGetExport(MpdecNativeLoader.Handle, name, out _))
            .ToArray();
        Assert.Empty(missing);

        foreach (string data in new[] { "MPD_MINALLOC", "mpd_round_string", "mpd_clamp_string", "mpd_traphandler",
            "mpd_mallocfunc", "mpd_callocfunc", "mpd_reallocfunc", "mpd_free" })
        {
            Assert.NotEqual(IntPtr.Zero, MpdecNativeLoader.GetExport(data));
        }
    }

    [Fact]
    public void Contexts()
    {
        mpd_context_t ctx;
        mpd_maxcontext(&ctx);
        Assert.Equal(MPD_MAX_PREC, ctx.prec);
        Assert.Equal(MPD_MAX_EMAX, ctx.emax);
        Assert.Equal(MPD_MIN_EMIN, ctx.emin);
        Assert.Equal(MPD_ROUND_HALF_EVEN, ctx.round);

        mpd_defaultcontext(&ctx);
        Assert.Equal(2 * MPD_RDIGITS, mpd_getprec(&ctx));
        Assert.Equal(MPD_ROUND_HALF_UP, mpd_getround(&ctx));
        Assert.Equal(MPD_Traps, mpd_gettraps(&ctx));

        Assert.Equal(0, mpd_ieee_context(&ctx, MPD_DECIMAL128));
        Assert.Equal(34, ctx.prec);
        Assert.Equal(6144, ctx.emax);
        Assert.Equal(-6143, ctx.emin);
        Assert.Equal(MPD_CLAMP_IEEE_754, ctx.clamp);

        Assert.Equal(1, mpd_qsetround(&ctx, MPD_ROUND_05UP));
        Assert.Equal(0, mpd_qsetround(&ctx, MPD_ROUND_GUARD));
        Assert.Equal(MPD_ROUND_05UP, ctx.round);
    }

    [Fact]
    public void GlobalVariables()
    {
        Assert.Equal("ROUND_HALF_EVEN", H.Str(mpd_round_string(MPD_ROUND_HALF_EVEN)));
        Assert.Equal("CLAMP_IEEE_754", H.Str(mpd_clamp_string(MPD_CLAMP_IEEE_754)));
        Assert.InRange(MPD_MINALLOC, MPD_MINALLOC_MIN, MPD_MINALLOC_MAX);
        Assert.True(mpd_traphandler != null);
        Assert.True(mpd_mallocfunc != null);
        Assert.Throws<ArgumentOutOfRangeException>(() => mpd_round_string(MPD_ROUND_GUARD));
    }

    [Fact]
    public void AllocatorFunctionPointers()
    {
        byte* p = (byte*)mpd_mallocfunc(16);
        Assert.True(p != null);
        p[15] = 42;
        p = (byte*)mpd_reallocfunc(p, 32);
        Assert.Equal(42, p[15]);
        mpd_free(p);
    }
}
