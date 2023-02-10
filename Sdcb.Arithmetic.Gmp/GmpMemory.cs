using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp
{
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
            __gmp_get_memory_functions(out MallocFp, out ReallocFp, out FreeFp);
        }

        public static void SetMemoryFunctions(
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
            __gmp_get_memory_functions(out MallocFp, out ReallocFp, out FreeFp);
        }

        private static delegate* unmanaged[Cdecl]<nint, IntPtr> MallocFp;
        private static delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> ReallocFp;
        private static delegate* unmanaged[Cdecl]<IntPtr, nint, void> FreeFp;

        public static IntPtr Malloc(nint size) => MallocFp(size);
        public static IntPtr Realloc(IntPtr ptr, nint oldSize, nint newSize) => ReallocFp(ptr, oldSize, newSize);
        public static void Free(IntPtr ptr, nint size = 0) => FreeFp(ptr, size);
    }

    public delegate IntPtr GmpMalloc(nint size);
    public delegate IntPtr GmpRealloc(IntPtr ptr, IntPtr newSize);
    public delegate void GmpFree(IntPtr ptr, nint size);
}
