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

            _console.WriteLine(version); // 4.1.1-p1
        }

        [Fact]
        public void RawPatch()
        {
            IntPtr ver = MpfrLib.mpfr_get_patches();
            Assert.NotEqual(IntPtr.Zero, ver);

            string? patches = Marshal.PtrToStringAnsi(ver);
            Assert.NotNull(patches);

            _console.WriteLine(patches); // mpfr_custom_get_kind
        }
    }
}