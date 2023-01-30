using System;
using System.Runtime.InteropServices;

namespace Sdcb.Arithmetic.Gmp
{
    public static unsafe class GmpMemory
    {
        [DllImport(GmpLib.Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void __gmp_get_memory_functions(
            out delegate* unmanaged[Cdecl]<nint, IntPtr> malloc, 
            out delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> realloc, 
            out delegate* unmanaged[Cdecl]<IntPtr, nint, void> free);

        [DllImport(GmpLib.Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void __gmp_set_memory_functions(
            out delegate* unmanaged[Cdecl]<nint, IntPtr> malloc, 
            out delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> realloc, 
            out delegate* unmanaged[Cdecl]<IntPtr, nint, void> free);

        static GmpMemory()
        {
            GmpNativeLoader.Init();
            __gmp_get_memory_functions(out MallocFp, out ReallocFp, out FreeFp);
        }

        private static delegate* unmanaged[Cdecl]<nint, IntPtr> MallocFp;
        private static delegate* unmanaged[Cdecl]<IntPtr, nint, nint, IntPtr> ReallocFp;
        private static delegate* unmanaged[Cdecl]<IntPtr, nint, void> FreeFp;

        public static IntPtr Malloc(nint size) => MallocFp(size);
        public static IntPtr Realloc(IntPtr ptr, nint oldSize, nint newSize) => ReallocFp(ptr, oldSize, newSize);
        public static void Free(IntPtr ptr, nint size = 0) => FreeFp(ptr, size);
    }
}
