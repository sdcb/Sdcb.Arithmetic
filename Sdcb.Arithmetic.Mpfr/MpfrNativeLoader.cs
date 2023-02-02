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
                    return NativeLibrary.Load("mpfr-6.dll", assembly, searchPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return NativeLibrary.Load("libmpfr.so.6", assembly, searchPath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return NativeLibrary.Load("libmpfr.6.dylib", assembly, searchPath);
                }
                else
                {
                    return NativeLibrary.Load("mpfr.6", assembly, searchPath);
                }
            }
            return IntPtr.Zero;
        }
    }
}
