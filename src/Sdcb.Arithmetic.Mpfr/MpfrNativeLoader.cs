using Sdcb.Arithmetic.Gmp;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpfr;

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
        static bool IsAndroid()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix && Environment.GetEnvironmentVariable("ANDROID_ROOT") != null;
        }

        if (libraryName == MpfrLib.Dll)
        {
            GmpNativeLoader.Load(assembly, searchPath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string[] trys =
                [
                    "libmpfr-6.dll",
                    "mpfr-6.dll", // for compatibility with older versions
                ];
                foreach (string tryName in trys)
                {
                    if (NativeLibrary.TryLoad(tryName, assembly, searchPath, out IntPtr handle))
                    {
                        return handle;
                    }
                }
                throw new DllNotFoundException(trys[0]);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return NativeLibrary.Load("mpfr.so.6", assembly, searchPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return NativeLibrary.Load("libmpfr.6.dylib", assembly, searchPath);
            }
            else if (IsAndroid())
            {
                return NativeLibrary.Load("libmpfr.so", assembly, searchPath);
            }
            else
            {
                return NativeLibrary.Load("mpfr.6", assembly, searchPath);
            }
        }
        return IntPtr.Zero;
    }
}
