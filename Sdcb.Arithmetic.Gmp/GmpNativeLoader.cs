using System;
using System.Reflection;
using System.Runtime.InteropServices;

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return NativeLibrary.Load("gmp-10.dll", assembly, searchPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return NativeLibrary.Load("libgmp.so.10", assembly, searchPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return NativeLibrary.Load("libgmp.10.dylib", assembly, searchPath);
            }
            else
            {
                return NativeLibrary.Load("gmp.10", assembly, searchPath);
            }
        }
        return IntPtr.Zero;
    }
}
