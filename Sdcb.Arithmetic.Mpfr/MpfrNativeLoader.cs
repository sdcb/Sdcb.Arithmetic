using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr
{
    internal static class MpfrNativeLoader
    {
        static MpfrNativeLoader()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), MpfrImportResolver);
        }

        public static void Init()
        {
            // stub to ensure static constructor executed at least once.
        }

        private static IntPtr MpfrImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == MpfrLib.Dll)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    NativeLibrary.Load("gmp-10.dll", assembly, searchPath);
                    return NativeLibrary.Load("mpfr-6.dll", assembly, searchPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    NativeLibrary.Load("gmp.so.10", assembly, searchPath);
                    return NativeLibrary.Load("mpfr.so.6", assembly, searchPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    NativeLibrary.Load("libgmp.10.dylib", assembly, searchPath);
                    return NativeLibrary.Load("libmpfr.6.dylib", assembly, searchPath);
                }
                else
                {
                    NativeLibrary.Load("gmp.10", assembly, searchPath);
                    return NativeLibrary.Load("mpfr.6", assembly, searchPath);
                }
            }
            return IntPtr.Zero;
        }
    }
}
