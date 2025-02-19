using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Gmp.Tests;

[CollectionDefinition("NonParallelCollection", DisableParallelization = true)]
public class RawTest
{
    private readonly ITestOutputHelper _console;

    public RawTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public unsafe void PiMulE()
    {
        IntPtr f1 = Marshal.AllocHGlobal(24);
        IntPtr f2 = Marshal.AllocHGlobal(24);
        IntPtr f3 = Marshal.AllocHGlobal(24);
        GmpLib.__gmpf_init_set_d(f1, 3.14);
        GmpLib.__gmpf_init_set_d(f2, 2.718);
        GmpLib.__gmpf_init(f3);
        GmpLib.__gmpf_mul(f3, f1, f2);
        double d = GmpLib.__gmpf_get_d(f3);
        _console.WriteLine(d.ToString());

        int exp = 0;
        IntPtr str = GmpLib.__gmpf_get_str(IntPtr.Zero, (IntPtr)(&exp), 10, 0, f3);
        _console.WriteLine(Marshal.PtrToStringAnsi(str));
        _console.WriteLine($"exp: {exp}");

        GmpLib.__gmpf_clear(f1);
        GmpLib.__gmpf_clear(f2);
        GmpLib.__gmpf_clear(f3);
        Marshal.FreeHGlobal(f1);
        Marshal.FreeHGlobal(f2);
        Marshal.FreeHGlobal(f3);
        GmpMemory.Free(str);
    }

    [Fact(Skip = "Dangerous")]
    public void MemoryTest()
    {
        GmpMemory.SetAllocator(
            malloc: n => Marshal.AllocHGlobal(n),
            realloc: (ptr, size) => Marshal.ReAllocHGlobal(ptr, size),
            free: (ptr, size) => Marshal.FreeHGlobal(ptr));
        {
            using GmpFloat a = new(precision: 100);
            _console.WriteLine(a.ToString());
        }
        GmpMemory.ResetAllocator();
    }
}