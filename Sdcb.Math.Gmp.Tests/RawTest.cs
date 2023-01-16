using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Math.Gmp.Tests
{
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
            GmpNative.__gmpf_init_set_d(f1, 3.14);
            GmpNative.__gmpf_init_set_d(f2, 2.718);
            GmpNative.__gmpf_init(f3);
            GmpNative.__gmpf_mul(f3, f1, f2);
            double d = GmpNative.__gmpf_get_d(f3);
            _console.WriteLine(d.ToString());

            int exp = 0;
            IntPtr str = GmpNative.__gmpf_get_str(IntPtr.Zero, (IntPtr)(&exp), 10, 0, f3);
            _console.WriteLine(Marshal.PtrToStringAnsi(str));
            _console.WriteLine($"exp: {exp}");

            GmpNative.__gmpf_clear(f1);
            GmpNative.__gmpf_clear(f2);
            GmpNative.__gmpf_clear(f3);
            Marshal.FreeHGlobal(f1);
            Marshal.FreeHGlobal(f2);
            Marshal.FreeHGlobal(f3);
            GmpMemory.Free(str);
        }
    }
}