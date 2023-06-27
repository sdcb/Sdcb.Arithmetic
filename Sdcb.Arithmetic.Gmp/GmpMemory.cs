using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

public static unsafe class GmpMemory
{
    [DllImport(GmpLib.Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void __gmp_get_memory_functions(
        out delegate* unmanaged[Cdecl]<nint, IntPtr> malloc,
        out delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> realloc,
        out delegate* unmanaged[Cdecl]<IntPtr, nint, void> free);

    [DllImport(GmpLib.Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void __gmp_set_memory_functions(
        delegate* unmanaged[Cdecl]<nint, IntPtr> malloc,
        delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> realloc,
        delegate* unmanaged[Cdecl]<IntPtr, nint, void> free);

    static GmpMemory()
    {
        GmpNativeLoader.Init();
        __gmp_get_memory_functions(out _nativeMallocFp, out _nativeReallocFp, out _nativeFreeFp);
        _mallocFp = _nativeMallocFp;
        _reallocFp = _nativeReallocFp;
        _freeFp= _nativeFreeFp;
    }

    public static void SetAllocator(
        GmpMalloc malloc,
        GmpRealloc realloc,
        GmpFree free)
    {
        IntPtr mallocPtr = Marshal.GetFunctionPointerForDelegate(malloc);
        IntPtr reallocPtr = Marshal.GetFunctionPointerForDelegate(realloc);
        IntPtr freePtr = Marshal.GetFunctionPointerForDelegate(free);
        __gmp_set_memory_functions(
            (delegate* unmanaged[Cdecl]<nint, IntPtr>)mallocPtr, 
            (delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr>)reallocPtr,
            (delegate* unmanaged[Cdecl]<IntPtr, nint, void>) freePtr);
        __gmp_get_memory_functions(out _mallocFp, out _reallocFp, out _freeFp);
    }

    public static void ResetAllocator()
    {
        __gmp_set_memory_functions(_nativeMallocFp, _nativeReallocFp, _nativeFreeFp);
        __gmp_get_memory_functions(out _mallocFp, out _reallocFp, out _freeFp);
    }

    private static delegate* unmanaged[Cdecl]<nint, IntPtr> _nativeMallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> _nativeReallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, void> _nativeFreeFp;

    private static delegate* unmanaged[Cdecl]<nint, IntPtr> _mallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> _reallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, void> _freeFp;

    public static IntPtr Malloc(nint size) => _mallocFp(size);
    public static IntPtr Realloc(IntPtr ptr, nint oldSize, nint newSize) => _reallocFp(ptr, oldSize, newSize);
    public static void Free(IntPtr ptr, nint size = 0) => _freeFp(ptr, size);
}

public delegate IntPtr GmpMalloc(nint size);
public delegate IntPtr GmpRealloc(IntPtr ptr, IntPtr newSize);
public delegate void GmpFree(IntPtr ptr, nint size);
