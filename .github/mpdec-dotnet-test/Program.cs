using System.Reflection;
using System.Runtime.InteropServices;

NativeLibrary.SetDllImportResolver(typeof(Mpdec).Assembly, Mpdec.Resolve);

string version = Marshal.PtrToStringUTF8(Mpdec.mpd_version())!;
Console.WriteLine($"libmpdec version: {version}");

MpdContext ctx = default;
Mpdec.mpd_maxcontext(ref ctx);
ctx.prec = 50;

uint status = 0;
IntPtr a = Mpdec.mpd_qnew(), b = Mpdec.mpd_qnew(), r = Mpdec.mpd_qnew();
try
{
    Mpdec.mpd_qset_string(a, "1", ref ctx, ref status);
    Mpdec.mpd_qset_string(b, "7", ref ctx, ref status);
    Mpdec.mpd_qdiv(r, a, b, ref ctx, ref status);

    IntPtr s = Mpdec.mpd_to_sci(r, 0);
    string result = Marshal.PtrToStringUTF8(s)!;
    Mpdec.Free(s);

    const string expected = "0.14285714285714285714285714285714285714285714285714";
    Console.WriteLine($"1/7 (prec=50) = {result}");
    if (result != expected)
    {
        Console.Error.WriteLine($"FAIL: expected {expected}");
        return 1;
    }
}
finally
{
    Mpdec.mpd_del(a);
    Mpdec.mpd_del(b);
    Mpdec.mpd_del(r);
}

Console.WriteLine(".NET P/Invoke smoke test passed");
return 0;

[StructLayout(LayoutKind.Sequential)]
struct MpdContext
{
    public nint prec, emax, emin;
    public uint traps, status, newtrap;
    public int round, clamp, allcr;
}

static unsafe class Mpdec
{
    const string Lib = "libmpdec";
    static readonly string NativeName = OperatingSystem.IsWindows() ? "libmpdec-4.0.1" : "libmpdec.so.4";
    static IntPtr _handle;

    public static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? path)
    {
        if (name != Lib) return IntPtr.Zero;
        return _handle = NativeLibrary.Load(NativeName, assembly, path);
    }

    [DllImport(Lib)] public static extern IntPtr mpd_version();
    [DllImport(Lib)] public static extern void mpd_maxcontext(ref MpdContext ctx);
    [DllImport(Lib)] public static extern IntPtr mpd_qnew();
    [DllImport(Lib)] public static extern void mpd_del(IntPtr dec);
    [DllImport(Lib)] public static extern void mpd_qset_string(IntPtr dec, [MarshalAs(UnmanagedType.LPUTF8Str)] string s, ref MpdContext ctx, ref uint status);
    [DllImport(Lib)] public static extern void mpd_qdiv(IntPtr q, IntPtr a, IntPtr b, ref MpdContext ctx, ref uint status);
    [DllImport(Lib)] public static extern IntPtr mpd_to_sci(IntPtr dec, int fmt);

    // mpd_free is an exported function-pointer variable, not a function.
    public static void Free(IntPtr p)
    {
        var free = *(delegate* unmanaged[Cdecl]<IntPtr, void>*)NativeLibrary.GetExport(_handle, "mpd_free");
        free(p);
    }
}
