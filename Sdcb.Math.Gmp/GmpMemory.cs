using System;
using System.Runtime.InteropServices;

namespace Sdcb.Math.Gmp
{
    public static unsafe class GmpMemory
    {
        [DllImport(GmpNative.Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void __gmp_get_memory_functions(out delegate*<nint, IntPtr> malloc, out delegate*<IntPtr, nint, nint, IntPtr> realloc, out delegate*<IntPtr, nint, void> free);

        [DllImport(GmpNative.Dll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void __gmp_set_memory_functions(out delegate*<nint, IntPtr> malloc, out delegate*<IntPtr, nint, nint, IntPtr> realloc, out delegate*<IntPtr, nint, void> free);

        static GmpMemory()
        {
            __gmp_get_memory_functions(out MallocFp, out ReallocFp, out FreeFp);
        }

        private static delegate*<nint, IntPtr> MallocFp;
        private static delegate*<IntPtr, nint, nint, IntPtr> ReallocFp;
        private static delegate*<IntPtr, nint, void> FreeFp;

        public static IntPtr Malloc(nint size) => MallocFp(size);
        public static IntPtr Realloc(IntPtr ptr, nint oldSize, nint newSize) => ReallocFp(ptr, oldSize, newSize);
        public static void Free(IntPtr ptr, nint size = 0) => FreeFp(ptr, size);
    }
}
