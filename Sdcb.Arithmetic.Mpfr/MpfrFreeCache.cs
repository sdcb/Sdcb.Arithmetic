using System;

namespace Sdcb.Arithmetic.Mpfr
{
    /// <summary>
    /// Free cache policy
    /// </summary>
    [Flags]
    public enum MpfrFreeCache
    {
        /// <summary>
        /// 1 << 0
        /// </summary>
        Local = 1,
        
        /// <summary>
        /// 1 << 1
        /// </summary>
        Global = 2, 
    }
}
