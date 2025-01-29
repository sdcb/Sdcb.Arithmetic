using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// A static, unsafe class designed to manage memory allocation, reallocation, and deallocation
/// in the Gmp library, allowing users to define custom memory management functions.
/// </summary>
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
        _freeFp = _nativeFreeFp;
    }

    /// <summary>
    /// Set custom allocator functions for memory management in the Gmp library.
    /// </summary>
    /// <param name="malloc">Delegate for the custom malloc function.</param>
    /// <param name="realloc">Delegate for the custom realloc function.</param>
    /// <param name="free">Delegate for the custom free function.</param>
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
            (delegate* unmanaged[Cdecl]<IntPtr, nint, void>)freePtr);
        __gmp_get_memory_functions(out _mallocFp, out _reallocFp, out _freeFp);
    }

    /// <summary>
    /// Reset memory management functions to their default implementations in the Gmp library.
    /// </summary>
    public static void ResetAllocator()
    {
        __gmp_set_memory_functions(_nativeMallocFp, _nativeReallocFp, _nativeFreeFp);
        __gmp_get_memory_functions(out _mallocFp, out _reallocFp, out _freeFp);
    }

    private static readonly delegate* unmanaged[Cdecl]<nint, IntPtr> _nativeMallocFp;
    private static readonly delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> _nativeReallocFp;
    private static readonly delegate* unmanaged[Cdecl]<IntPtr, nint, void> _nativeFreeFp;

    private static delegate* unmanaged[Cdecl]<nint, IntPtr> _mallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> _reallocFp;
    private static delegate* unmanaged[Cdecl]<IntPtr, nint, void> _freeFp;

    /// <summary>
    /// Allocates a block of memory of the specified size using the current malloc function pointer.
    /// </summary>
    /// <param name="size">The size of the memory block to allocate.</param>
    /// <returns>A pointer to the allocated memory.</returns>
    public static IntPtr Malloc(nint size) => _mallocFp(size);

    /// <summary>
    /// Reallocates a block of memory at the specified pointer with the specified new size using the current realloc function pointer.
    /// </summary>
    /// <param name="ptr">The pointer to the memory block to reallocate.</param>
    /// <param name="oldSize">The old size of the memory block.</param>
    /// <param name="newSize">The new size of the memory block.</param>
    /// <returns>A pointer to the reallocated memory.</returns>
    public static IntPtr Realloc(IntPtr ptr, nint oldSize, nint newSize) => _reallocFp(ptr, oldSize, newSize);

    /// <summary>
    /// Frees a block of memory at the specified pointer using the current free function pointer.
    /// </summary>
    /// <param name="ptr">The pointer to the memory block to free.</param>
    /// <param name="size">The size of the memory block to free (default is 0).</param>
    public static void Free(IntPtr ptr, nint size = 0) => _freeFp(ptr, size);
}

/// <summary>
/// Represents a custom delegate for GMP memory allocation.
/// </summary>
/// <remarks>
/// This delegate serves as a way to provide custom memory allocation
/// functions in your application that work with the GMP (GNU Multiple
/// Precision Arithmetic) library.
/// </remarks>
/// <param name="size">The size of the requested memory block in bytes.</param>
/// <returns>A pointer to the allocated memory block.</returns>
public delegate IntPtr GmpMalloc(nint size);

/// <summary>
/// Represents a custom delegate for GMP memory reallocation.
/// </summary>
/// <remarks>
/// This delegate serves as a way to provide custom memory reallocation
/// functions in your application that work with the GMP (GNU Multiple
/// Precision Arithmetic) library.
/// </remarks>
/// <param name="ptr">A pointer to the previously allocated memory block.</param>
/// <param name="newSize">The size of the new requested memory block in bytes.</param>
/// <returns>A pointer to the reallocated memory block.</returns>
public delegate IntPtr GmpRealloc(IntPtr ptr, IntPtr newSize);

/// <summary>
/// Represents a custom delegate for GMP memory deallocation.
/// </summary>
/// <remarks>
/// This delegate serves as a way to provide custom memory deallocation
/// functions in your application that work with the GMP (GNU Multiple
/// Precision Arithmetic) library.
/// </remarks>
/// <param name="ptr">A pointer to the memory block to deallocate.</param>
/// <param name="size">The size of the memory block in bytes.</param>
public delegate void GmpFree(IntPtr ptr, nint size);