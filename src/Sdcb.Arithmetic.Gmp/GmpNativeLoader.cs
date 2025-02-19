using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
[assembly: InternalsVisibleTo("Sdcb.Arithmetic.Mpfr")]

namespace Sdcb.Arithmetic.Gmp;

internal static class GmpNativeLoader
{
    static GmpNativeLoader()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), GmpImportResolver);
    }

    public static void Init()
    {
        // stub to ensure static constructor executed at least once.
    }

    private static IntPtr GmpImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == GmpLib.Dll)
        {
            return Load(assembly, searchPath);
        }
        return IntPtr.Zero;
    }

    public static IntPtr Load(Assembly assembly, DllImportSearchPath? searchPath)
    {
        static bool IsAndroid()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix && Environment.GetEnvironmentVariable("ANDROID_ROOT") != null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string[] trys =
            [
                "libgmp-10.dll",
                "gmp-10.dll", // for compatibility with older versions
            ];
            foreach (string tryName in trys)
            {
                if (NativeLibrary.TryLoad(tryName, assembly, searchPath, out IntPtr handle))
                {
                    return handle;
                }
            }
            throw new DllNotFoundException("libgmp-10.dll");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return NativeLibrary.Load("libgmp.so.10", assembly, searchPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return NativeLibrary.Load("libgmp.10.dylib", assembly, searchPath);
        }
        else if (IsAndroid())
        {
            return NativeLibrary.Load("libgmp.so", assembly, searchPath);
        }
        else
        {
            return NativeLibrary.Load("gmp.10", assembly, searchPath);
        }
    }
}
