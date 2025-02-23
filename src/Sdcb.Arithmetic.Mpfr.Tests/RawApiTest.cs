using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class RawApiTest
{
    private readonly ITestOutputHelper _console;

    public RawApiTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public unsafe void InitClose()
    {
        byte[] t = new byte[MpfrFloat.RawSize];
        fixed (byte* pt = &t[0])
        {
            MpfrLib.mpfr_init2((IntPtr)pt, new CLong(128));
            MpfrLib.mpfr_clear((IntPtr)pt);
            _console.WriteLine("Good!");
        }
    }
}
