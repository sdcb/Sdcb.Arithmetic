using Sdcb.Arithmetic.Gmp;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests
{
    public class GlobalTest
    {
        private readonly ITestOutputHelper _console;

        public GlobalTest(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void RawVersion()
        {
            IntPtr ver = MpfrLib.mpfr_get_version();
            Assert.NotEqual(IntPtr.Zero, ver);

            string? version = Marshal.PtrToStringAnsi(ver);
            Assert.NotNull(version);

            _console.WriteLine(version); // "4.2.0"
        }

        [Fact]
        public void RawPatch()
        {
            IntPtr ver = MpfrLib.mpfr_get_patches();
            Assert.NotEqual(IntPtr.Zero, ver);

            string? patches = Marshal.PtrToStringAnsi(ver);
            Assert.NotNull(patches);

            _console.WriteLine(patches); // ""
        }

        [Fact]
        public unsafe void CalcE()
        {
#pragma warning disable CA1806 // 不要忽略方法结果
            // https://www.mpfr.org/sample.html
            byte[] s = new byte[MpfrFloat.RawSize], t = new byte[MpfrFloat.RawSize], u = new byte[MpfrFloat.RawSize];
            fixed (byte* ps = &s[0])
            fixed (byte* pt = &t[0])
            fixed (byte* pu = &u[0])
            {
                MpfrLib.mpfr_init2((IntPtr)pt, 200);
                MpfrLib.mpfr_set_d((IntPtr)pt, 1.0, MpfrRounding.ToNegativeInfinity);
                MpfrLib.mpfr_init2((IntPtr)ps, 200);
                MpfrLib.mpfr_set_d((IntPtr)ps, 1.0, MpfrRounding.ToNegativeInfinity);
                MpfrLib.mpfr_init2((IntPtr)pu, 200);

                for (uint i = 1; i <= 100; ++i)
                {
                    MpfrLib.mpfr_mul_ui((IntPtr)pt, (IntPtr)pt, i, MpfrRounding.ToPositiveInfinity);
                    MpfrLib.mpfr_set_d((IntPtr)pu, 1.0, MpfrRounding.ToNegativeInfinity);
                    MpfrLib.mpfr_div((IntPtr)pu, (IntPtr)pu, (IntPtr)pt, MpfrRounding.ToNegativeInfinity);
                    MpfrLib.mpfr_add((IntPtr)ps, (IntPtr)ps, (IntPtr)pu, MpfrRounding.ToNegativeInfinity);
                }

                int exp;
                IntPtr strptr = MpfrLib.mpfr_get_str(IntPtr.Zero, (IntPtr)(&exp), 10, 0, (IntPtr)ps, MpfrRounding.ToNegativeInfinity);
                string str = Marshal.PtrToStringAnsi(strptr)!;
                Assert.Equal("2.7182818284590452353602874713526624977572470936999595749669131", GmpFloat.ToString(str, exp));
                MpfrLib.mpfr_free_str(strptr);
                MpfrLib.mpfr_clear((IntPtr)ps);
                MpfrLib.mpfr_clear((IntPtr)pt);
                MpfrLib.mpfr_clear((IntPtr)pu);
                MpfrLib.mpfr_free_cache();
            }
#pragma warning restore CA1806 // 不要忽略方法结果
        }
    }
}