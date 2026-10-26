using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Mpdec;

/// <summary>
/// Resolves the <c>libmpdec</c> native library for <see cref="MpdecLib"/>.
/// </summary>
public static class MpdecNativeLoader
{
    private static IntPtr _handle;

    static MpdecNativeLoader()
    {
        NativeLibrary.SetDllImportResolver(typeof(MpdecNativeLoader).Assembly, Resolve);
    }

    internal static void Init()
    {
        // stub to ensure static constructor executed at least once.
    }

    /// <summary>
    /// Candidate native library names for the current OS, in probing order.
    /// </summary>
    public static string[] CandidateNames
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return
                [
                    $"libmpdec-{MpdecLib.MPD_VERSION}.dll", // MSVC build (Sdcb.Arithmetic.Mpdec.runtime.win-x64)
                    $"libmpdec-{MpdecLib.MPD_MAJOR_VERSION}.dll", // MinGW build
                    "libmpdec.dll",
                    "mpdec.dll",
                ];
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return [$"libmpdec.{MpdecLib.MPD_MAJOR_VERSION}.dylib", "libmpdec.dylib"];
            }
            else
            {
                return [$"libmpdec.so.{MpdecLib.MPD_MAJOR_VERSION}", "libmpdec.so"];
            }
        }
    }

    /// <summary>
    /// Loads (once) and returns the native <c>libmpdec</c> library handle.
    /// </summary>
    public static IntPtr Handle => _handle != IntPtr.Zero ? _handle : Load(typeof(MpdecNativeLoader).Assembly, null);

    /// <summary>
    /// Gets the address of an exported symbol (function or global variable) of <c>libmpdec</c>.
    /// </summary>
    public static IntPtr GetExport(string name) => NativeLibrary.GetExport(Handle, name);

    private static IntPtr Load(Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (_handle != IntPtr.Zero) return _handle;

        string[] names = CandidateNames;
        foreach (string name in names)
        {
            if (NativeLibrary.TryLoad(name, assembly, searchPath, out IntPtr handle))
            {
                return _handle = handle;
            }
        }
        throw new DllNotFoundException($"Unable to load libmpdec, tried: {string.Join(", ", names)}. " +
            $"Install the native runtime package, e.g. Sdcb.Arithmetic.Mpdec.runtime.win-x64 / Sdcb.Arithmetic.Mpdec.runtime.linux-x64.");
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName == MpdecLib.Dll ? Load(assembly, searchPath) : IntPtr.Zero;
    }
}
