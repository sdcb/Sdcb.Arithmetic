using Sdcb.Arithmetic.Gmp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sdcb.Arithmetic.Mpfr;

/// <summary>Represents an arbitrary precision floating point number using the MPFR library.</summary>
/// <remarks>This class is not thread-safe and should be used with caution in multi-threaded applications.</remarks>
public unsafe class MpfrFloat : IDisposable, IFormattable, IEquatable<MpfrFloat>, IComparable, IComparable<MpfrFloat>
{
    /// <summary>The size of the MPFR float in bytes, depending on the operating system.</summary>
    public static readonly int RawSize = Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => sizeof(WindowsMpfr_t),
        _ => sizeof(Mpfr_t),
    };

    internal readonly Mpfr_t Raw = new();

    #region 1. Initialization Functions
    /// <summary>
    /// Initializes a new instance of the <see cref="MpfrFloat"/> class with the specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">The precision in bits of the floating-point number.</param>
    /// <remarks>
    /// The <paramref name="precision"/> parameter determines the number of bits used to represent the floating-point number. 
    /// The higher the precision, the more accurate the number representation, but also the more memory it requires.
    /// </remarks>
    public MpfrFloat(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init2((IntPtr)ptr, precision);
            _ = MpfrLib.mpfr_set_ui((IntPtr)ptr, 0, MpfrRounding.ToEven);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MpfrFloat"/> class with default precision and value 0.
    /// </summary>
    /// <remarks>
    /// The default precision is determined by the underlying MPFR library.
    /// </remarks>
    /// <returns>A new instance of <see cref="MpfrFloat"/> with default precision and value 0.</returns>
    public MpfrFloat()
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init((IntPtr)ptr);
            _ = MpfrLib.mpfr_set_ui((IntPtr)ptr, 0, MpfrRounding.ToEven);
        }
    }

    internal static MpfrFloat CreateWithNullablePrecision(int? precision) => precision switch
    {
        null => new MpfrFloat(),
        _ => new MpfrFloat(precision.Value)
    };

    /// <summary>
    /// Gets or sets the default precision in bits used for new <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <value>The default precision in bits.</value>
    public static int DefaultPrecision
    {
        get => MpfrLib.mpfr_get_default_prec();
        set => MpfrLib.mpfr_set_default_prec(value);
    }

    /// <summary>
    /// The maximum supported precision that can be used in <see cref="MpfrFloat"/>.
    /// </summary>
    public const int MaxSupportedPrecision = int.MaxValue - 256;

    /// <summary>
    /// The minimum supported precision that can be used in <see cref="MpfrFloat"/>.
    /// </summary>
    public const int MinSupportedPrecision = 1;

    /// <summary>
    /// Gets or sets the precision of the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <remarks>
    /// The precision is the number of bits used to represent the floating-point number. 
    /// </remarks>
    /// <value>The precision of the current <see cref="MpfrFloat"/> instance.</value>
    public int Precision
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_get_prec((IntPtr)pthis);
            }
        }
        set => RoundToPrecision(Precision, DefaultRounding);
    }

    private static void CheckPrecision(int precision)
    {
        if (precision < 1 || precision > MaxSupportedPrecision)
            throw new ArgumentOutOfRangeException(nameof(precision), $"Precision should in range of [{MinSupportedPrecision}..{MaxSupportedPrecision}].");
    }

    /// <summary>
    /// Reset the precision of the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="precision">The new precision to set.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="precision"/> is less than <see cref="MinSupportedPrecision"/> or greater than <see cref="MaxSupportedPrecision"/>.</exception>
    public void ResetPrecision(int precision)
    {
        CheckPrecision(precision);
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_prec((IntPtr)pthis, precision);
        }
    }
    #endregion

    #region 2. Assignment Functions

    /// <summary>
    /// Assigns the value of <paramref name="val"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="val">The <see cref="MpfrFloat"/> instance to assign the value from.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(MpfrFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the unsigned integer <paramref name="val"/> to the current instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="val">The unsigned integer value to assign.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(uint val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_ui((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the integer value <paramref name="val"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="val">The integer value to assign.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(int val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_si((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of a single-precision floating point number to the current instance.
    /// </summary>
    /// <param name="val">The single-precision floating point number to assign.</param>
    /// <param name="rounding">The rounding mode to use, or null to use the default rounding mode.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The assigned value will be rounded according to the specified <paramref name="rounding"/> mode, or the default rounding mode if <paramref name="rounding"/> is null.
    /// </remarks>
    public int Assign(float val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_flt((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of a double-precision floating-point number to the current instance.
    /// </summary>
    /// <param name="val">The double-precision floating-point number to assign.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The assigned value will be rounded according to the specified rounding mode, or the default rounding mode if none is specified.
    /// </remarks>
    public int Assign(double val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_d((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpInteger"/> instance to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="val">The <see cref="GmpInteger"/> instance to assign.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(GmpInteger val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_z((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpRational"/> instance to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="val">The <see cref="GmpRational"/> instance to assign.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(GmpRational val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpq_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_q((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of <paramref name="val"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="val">The <see cref="GmpFloat"/> instance to assign the value from.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(GmpFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpf_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_f((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of <paramref name="op"/> times 2 to the power of <paramref name="e"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The unsigned integer value to multiply by 2 to the power of <paramref name="e"/>.</param>
    /// <param name="e">The exponent of 2 to raise <paramref name="op"/> to.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The value of this instance will be set to <paramref name="op"/> times 2 to the power of <paramref name="e"/>.
    /// </remarks>
    public int Assign2Exp(uint op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_ui_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of <paramref name="op"/> times 2 to the power of <paramref name="e"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The integer value to multiply by 2 to the power of <paramref name="e"/>.</param>
    /// <param name="e">The exponent of 2.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign2Exp(int op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_si_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns the value of <paramref name="op"/> times 2 raised to the power of <paramref name="e"/> to this <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> value to multiply.</param>
    /// <param name="e">The exponent of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// This function sets this <see cref="MpfrFloat"/> instance to the value of <paramref name="op"/> times 2 raised to the power of <paramref name="e"/>.
    /// </remarks>
    public int Assign2Exp(GmpInteger op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_set_z_2exp((IntPtr)pthis, (IntPtr)pop, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Assigns a string representation of a number to the current instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="s">The string representation of the number to assign.</param>
    /// <param name="base">The base of the number to assign. Default is 0, which means auto-detect the base from the string.</param>
    /// <param name="rounding">The rounding mode to use. Default is <see cref="DefaultRounding"/>.</param>
    /// <exception cref="FormatException">Thrown when the string representation cannot be parsed to <see cref="MpfrFloat"/>.</exception>
    /// <remarks>
    /// The string representation must be in UTF-8 encoding. If the base is not specified, the function will try to auto-detect the base from the string.
    /// </remarks>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int Assign(string s, int @base = 0, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(s);
            fixed (byte* opPtr = opBytes)
            {
                byte* endptr = default;
                int ret = MpfrLib.mpfr_strtofr((IntPtr)pthis, (IntPtr)opPtr, (IntPtr)(&endptr), @base, rounding ?? DefaultRounding);
                if (endptr[0] != 0)
                {
                    string location = Marshal.PtrToStringUTF8((IntPtr)endptr)!;
                    throw new FormatException($"Failed to parse \"{s}\", base={@base} to {nameof(MpfrFloat)}, mpfr_strtofr returns {ret} at: {location}");
                }
                return ret;
            }
        }
    }

    /// <summary>
    /// Tries to assign a value to the current <see cref="MpfrFloat"/> instance from a string representation.
    /// </summary>
    /// <param name="s">The string representation of the value to assign.</param>
    /// <param name="base">The base of the string representation, or 0 to auto-detect the base.</param>
    /// <param name="rounding">The rounding mode to use, or <c>null</c> to use the default rounding mode.</param>
    /// <returns><c>true</c> if the assignment was successful, <c>false</c> otherwise.</returns>
    public bool TryAssign(string s, int @base = 0, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(s);
            fixed (byte* opPtr = opBytes)
            {
                byte* endptr = default;
                int ret = MpfrLib.mpfr_strtofr((IntPtr)pthis, (IntPtr)opPtr, (IntPtr)(&endptr), @base, rounding ?? DefaultRounding);
                return endptr[0] == 0;
            }
        }
    }

    /// <summary>
    /// Assigns a NaN (Not-a-Number) value to the current instance.
    /// </summary>
    public void AssignNaN()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_nan((IntPtr)pthis);
        }
    }

    /// <summary>
    /// Assigns positive or negative infinity to the current instance.
    /// </summary>
    /// <param name="sign">The sign of the infinity to assign. Use 1 for positive infinity, -1 for negative infinity.</param>
    /// <remarks>
    /// After calling this method, the current instance will represent positive or negative infinity, depending on the value of <paramref name="sign"/>.
    /// </remarks>
    public void AssignInfinity(int sign)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_inf((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// Assigns zero to the current instance.
    /// </summary>
    /// <param name="sign">The sign of the zero value. Default is 0, which means the sign is positive.</param>
    public void AssignZero(int sign = 0)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_zero((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// Swaps the values of two <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <param name="x">The first <see cref="MpfrFloat"/> instance.</param>
    /// <param name="y">The second <see cref="MpfrFloat"/> instance.</param>
    /// <remarks>
    /// This method swaps the values of <paramref name="x"/> and <paramref name="y"/> by reference.
    /// </remarks>
    public static void Swap(MpfrFloat x, MpfrFloat y)
    {
        fixed (Mpfr_t* p1 = &x.Raw)
        fixed (Mpfr_t* p2 = &y.Raw)
        {
            MpfrLib.mpfr_swap((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Swaps the value of this <see cref="MpfrFloat"/> instance with another <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to swap with.</param>
    /// <remarks>
    /// This method swaps the value of this <see cref="MpfrFloat"/> instance with another <see cref="MpfrFloat"/> instance.
    /// </remarks>
    public void Swap(MpfrFloat op) => Swap(this, op);
    #endregion

    #region 3. Combined Initialization and Assignment Functions

    /// <summary>
    /// Create a new <see cref="MpfrFloat"/> instance from an existing <paramref name="op"/> instance, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to create a new instance from.</param>
    /// <param name="precision">The precision in bits of the new instance, or null to use the same precision as <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use for the new instance, or null to use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> with the specified precision and rounding mode, initialized with the value of <paramref name="op"/>.</returns>
    public static MpfrFloat From(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Creates a new instance of <see cref="MpfrFloat"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new instance of <see cref="MpfrFloat"/> that is a copy of this instance.</returns>
    public MpfrFloat Clone() => From(this, Precision);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from an unsigned integer <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The unsigned integer value to convert.</param>
    /// <param name="precision">The precision in bits of the new instance. If null, use default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Implicitly converts an unsigned integer <paramref name="op"/> to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The unsigned integer value to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static implicit operator MpfrFloat(uint op) => From(op);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from an integer <paramref name="op"/> with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op">The integer value to convert.</param>
    /// <param name="precision">The precision in bits, or null to use default precision.</param>
    /// <param name="rounding">The rounding mode to use, or null to use default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(int op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Implicitly converts an integer value to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static implicit operator MpfrFloat(int op) => From(op);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from a double value <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The double value to convert.</param>
    /// <param name="precision">The precision in bits of the new instance, or null to use default precision.</param>
    /// <param name="rounding">The rounding mode to use, or null to use default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(double op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Implicitly converts a <see cref="double"/> value to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="double"/> value to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static implicit operator MpfrFloat(double op) => From(op);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to convert.</param>
    /// <param name="precision">The precision in bit of the new <see cref="MpfrFloat"/> instance, default to the absolute value of <paramref name="op"/> size times <see cref="GmpLib.LimbBitSize"/>.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(GmpInteger op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? (int)(Math.Abs(op.Raw.Size) * GmpLib.LimbBitSize));
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Explicitly converts a <see cref="GmpInteger"/> to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static explicit operator MpfrFloat(GmpInteger op) => From(op);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from a <see cref="GmpRational"/> value.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> value to convert.</param>
    /// <param name="precision">The precision in bits of the new <see cref="MpfrFloat"/> instance. If null, use default precision.</param>
    /// <param name="rounding">The rounding mode to use when converting the value. If null, use default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(GmpRational op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Explicitly converts a <see cref="GmpRational"/> instance to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static explicit operator MpfrFloat(GmpRational op) => From(op);

    /// <summary>
    /// Create a new instance of <see cref="MpfrFloat"/> from a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <param name="precision">The precision of the new <see cref="MpfrFloat"/> instance, default to null.</param>
    /// <param name="rounding">The rounding mode to use, default to null.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the converted value.</returns>
    public static MpfrFloat From(GmpFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <summary>
    /// Implicitly converts a <see cref="GmpFloat"/> instance to a <see cref="MpfrFloat"/> instance with the same precision.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the same value as <paramref name="op"/>.</returns>
    public static implicit operator MpfrFloat(GmpFloat op) => From(op, (int)op.Precision);

    /// <summary>
    /// Parses the string representation of a number and returns a new instance of <see cref="MpfrFloat"/> with the parsed value.
    /// </summary>
    /// <param name="s">The string representation of the number to parse.</param>
    /// <param name="base">The base of the number to parse. Default is 0, which means the base is determined by the prefix of the string (e.g. "0x" for hexadecimal).</param>
    /// <param name="precision">The precision of the new instance of <see cref="MpfrFloat"/>. If null, the default precision is used.</param>
    /// <param name="rounding">The rounding mode to use when assigning the parsed value to the new instance of <see cref="MpfrFloat"/>. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> with the parsed value.</returns>
    /// <exception cref="FormatException">Thrown when the string representation of the number is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when the specified base is not supported.</exception>
    /// <exception cref="OverflowException">Thrown when the parsed value is outside the range of the target type.</exception>
    /// <remarks>
    /// This method creates a new instance of <see cref="MpfrFloat"/> with the specified precision, then assigns the parsed value to it using the specified rounding mode.
    /// If the parsing fails, the created instance is cleared and an exception is thrown.
    /// </remarks>
    public static MpfrFloat Parse(string s, int @base = 0, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        try
        {
            rop.Assign(s, @base, rounding);
        }
        catch
        {
            rop.Clear();
            throw;
        }
        return rop;
    }

    /// <summary>
    /// Tries to parse the string representation of a <see cref="MpfrFloat"/> number.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="rop">When this method returns, contains the <see cref="MpfrFloat"/> equivalent of the numeric value if the conversion succeeded, or <see langword="null"/> if the conversion failed. The conversion fails if the <paramref name="s"/> parameter is <see langword="null"/>, is not a number in a valid format. This parameter is passed uninitialized; any value originally supplied in <paramref name="rop"/> will be overwritten.</param>
    /// <param name="base">The base of the number in <paramref name="s"/>. Default is 0, which means the base is determined by the format of <paramref name="s"/>.</param>
    /// <param name="precision">The precision of the <see cref="MpfrFloat"/> to create. If <see langword="null"/>, the default precision will be used.</param>
    /// <param name="rounding">The rounding mode to use. If <see langword="null"/>, the default rounding mode will be used.</param>
    /// <returns><see langword="true"/> if the conversion succeeded; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method returns <see langword="false"/> if the <paramref name="s"/> parameter is <see langword="null"/>, is not a number in a valid format. The <paramref name="rop"/> parameter is passed uninitialized; any value originally supplied in <paramref name="rop"/> will be overwritten.
    /// </remarks>
    public static bool TryParse(string s, [MaybeNullWhen(returnValue: false)] out MpfrFloat rop, int @base = 0, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat r = CreateWithNullablePrecision(precision);
        if (r.TryAssign(s, @base, rounding))
        {
            rop = r;
            return true;
        }
        else
        {
            r.Clear();
            rop = null;
            return false;
        }
    }
    #endregion

    #region 4. Conversion Functions

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a <see cref="double"/> value.
    /// </summary>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>The <see cref="double"/> value representation of the current <see cref="MpfrFloat"/> instance.</returns>
    public double ToDouble(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_d((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="MpfrFloat"/> instance to a <see cref="double"/> value.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>The <see cref="double"/> value converted from <paramref name="op"/>.</returns>
    public static explicit operator double(MpfrFloat op) => op.ToDouble();

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a single-precision floating-point number.
    /// </summary>
    /// <param name="rounding">The rounding mode to use, or <see langword="null"/> to use the default rounding mode.</param>
    /// <returns>The single-precision floating-point number equivalent to the current <see cref="MpfrFloat"/> instance.</returns>
    public float ToFloat(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_flt((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="MpfrFloat"/> instance to a single-precision floating-point number.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>The converted single-precision floating-point number.</returns>
    public static explicit operator float(MpfrFloat op) => op.ToFloat();

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a 32-bit signed integer using the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. Default is <see cref="MpfrRounding.ToZero"/>.</param>
    /// <returns>A 32-bit signed integer representation of the current <see cref="MpfrFloat"/> instance.</returns>
    public int ToInt32(MpfrRounding rounding = MpfrRounding.ToZero)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_si((IntPtr)pthis, rounding);
        }
    }

    /// <summary>
    /// Explicitly converts a <see cref="MpfrFloat"/> instance to a 32-bit signed integer.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>The 32-bit signed integer value equivalent of the <paramref name="op"/> parameter.</returns>
    public static explicit operator int(MpfrFloat op) => op.ToInt32();

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="rounding">The rounding mode to use for the conversion. Default is <see cref="MpfrRounding.ToZero"/>.</param>
    /// <returns>An unsigned 32-bit integer representation of the current <see cref="MpfrFloat"/> instance.</returns>
    public uint ToUInt32(MpfrRounding rounding = MpfrRounding.ToZero)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_ui((IntPtr)pthis, rounding);
        }
    }

    /// <summary>
    /// Explicitly converts the specified <see cref="MpfrFloat"/> object to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> object to convert.</param>
    /// <returns>An unsigned 32-bit integer that represents the converted <see cref="MpfrFloat"/> object.</returns>
    public static explicit operator uint(MpfrFloat op) => op.ToUInt32();

    /// <summary>
    /// Converts the current instance to an <see cref="ExpDouble"/> instance with the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>An <see cref="ExpDouble"/> instance representing the converted value.</returns>
    public ExpDouble ToExpDouble(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            int exp;
            double d = MpfrLib.mpfr_get_d_2exp((IntPtr)(&exp), (IntPtr)pthis, rounding ?? DefaultRounding);
            return new ExpDouble(exp, d);
        }
    }

    /// <summary>
    /// Compute the normalized fraction and exponent of this <see cref="MpfrFloat"/> instance, and store the fraction in this instance.
    /// </summary>
    /// <param name="y">The <see cref="MpfrFloat"/> instance to store the exponent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>A tuple containing the exponent and a boolean indicating whether overflow occurred.</returns>
    /// <remarks>
    /// This method computes the normalized fraction and exponent of this <see cref="MpfrFloat"/> instance, and stores the fraction in this instance.
    /// The exponent is stored in <paramref name="y"/>.
    /// </remarks>
    public (int exp, bool overflowed) FrexpInplace(MpfrFloat y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            int exp;
            bool overflowed = MpfrLib.mpfr_frexp((IntPtr)(&exp), (IntPtr)py, (IntPtr)pthis, rounding ?? DefaultRounding) != 0;
            return (exp, overflowed);
        }
    }

    /// <summary>
    /// Computes the mantissa and exponent of this <see cref="MpfrFloat"/> instance and returns a tuple containing the mantissa as a new <see cref="MpfrFloat"/> instance, the exponent as an integer and a boolean indicating whether the operation caused an overflow.
    /// </summary>
    /// <param name="precision">The precision in bits of the new <see cref="MpfrFloat"/> instance to create for the mantissa. If null, the precision of this instance is used.</param>
    /// <param name="rounding">The rounding mode to use for the operation. If null, the default rounding mode is used.</param>
    /// <returns>A tuple containing the mantissa as a new <see cref="MpfrFloat"/> instance, the exponent as an integer and a boolean indicating whether the operation caused an overflow.</returns>
    /// <remarks>
    /// The mantissa is returned as a new <see cref="MpfrFloat"/> instance with the specified precision. The exponent is returned as an integer. The boolean indicates whether the operation caused an overflow, which occurs when the mantissa is too large to be represented with the specified precision.
    /// </remarks>
    public (MpfrFloat y, int exp, bool overflowed) Frexp(int? precision = 0, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int exp, bool overflowed) = FrexpInplace(rop, rounding);
        return (rop, exp, overflowed);
    }

    /// <summary>
    /// Gets the integer value and the exponent of 2 of this <see cref="MpfrFloat"/> instance and stores the integer value in the provided <paramref name="z"/>.
    /// </summary>
    /// <param name="z">The <see cref="GmpInteger"/> instance to store the integer value.</param>
    /// <returns>The exponent of 2.</returns>
    /// <remarks>
    /// This method modifies the state of this <see cref="MpfrFloat"/> instance.
    /// </remarks>
    public int GetIntegerAnd2ExpInplace(GmpInteger z)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pz = &z.Raw)
        {
            return MpfrLib.mpfr_get_z_2exp((IntPtr)pz, (IntPtr)pthis);
        }
    }

    /// <summary>
    /// Get the integer part and 2^exp of this <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <returns>A tuple containing the integer part as a <see cref="GmpInteger"/> and the exponent as an <see cref="int"/>.</returns>
    public (GmpInteger z, int exp) GetIntegerAnd2Exp()
    {
        GmpInteger z = new();
        int exp = GetIntegerAnd2ExpInplace(z);
        return (z, exp);
    }

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a <see cref="GmpInteger"/> instance and stores the result in-place.
    /// </summary>
    /// <param name="z">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The current <see cref="MpfrFloat"/> instance will be rounded to an integer before conversion.
    /// </remarks>
    public int ToGmpIntegerInplace(GmpInteger z, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pz = &z.Raw)
        {
            return MpfrLib.mpfr_get_z((IntPtr)pz, (IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a <see cref="GmpInteger"/> instance with the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rounding">The rounding mode to use during the conversion. Default is <see cref="MpfrRounding.ToZero"/>.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public GmpInteger ToGmpInteger(MpfrRounding rounding = MpfrRounding.ToZero)
    {
        GmpInteger rop = new();
        ToGmpIntegerInplace(rop, rounding);
        return rop;
    }

    /// <summary>
    /// Explicitly converts a <see cref="MpfrFloat"/> instance to a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="r">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public static explicit operator GmpInteger(MpfrFloat r) => r.ToGmpInteger();

    /// <summary>
    /// Converts the current instance to a <see cref="GmpRational"/> instance and stores the result in-place.
    /// </summary>
    /// <param name="q">The <see cref="GmpRational"/> instance to store the result.</param>
    /// <param name="rounding">The rounding mode to use, or <see langword="null"/> to use the default rounding mode.</param>
    /// <remarks>
    /// This method converts the current instance to a <see cref="GmpRational"/> instance and stores the result in the <paramref name="q"/> parameter.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="q"/> is <see langword="null"/>.</exception>
    public void ToGmpRationalInplace(GmpRational q, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpq_t* pq = &q.Raw)
        {
            MpfrLib.mpfr_get_q((IntPtr)pq, (IntPtr)pthis);
        }
    }

    /// <summary>
    /// Converts the current instance of <see cref="GmpFloat"/> to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public GmpRational ToGmpRational()
    {
        GmpRational rop = new();
        ToGmpRationalInplace(rop);
        return rop;
    }

    /// <summary>
    /// Explicitly converts a <see cref="MpfrFloat"/> instance to a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="r">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpRational"/> representing the converted value.</returns>
    public static explicit operator GmpRational(MpfrFloat r) => r.ToGmpRational();

    /// <summary>
    /// Convert the current instance of <see cref="MpfrFloat"/> to a <see cref="GmpFloat"/> instance, and store the result in-place in <paramref name="f"/>.
    /// </summary>
    /// <param name="f">The <see cref="GmpFloat"/> instance to store the result.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int ToGmpFloatInplace(GmpFloat f, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpf_t* pf = &f.Raw)
        {
            return MpfrLib.mpfr_get_f((IntPtr)pf, (IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Converts the current instance to a <see cref="GmpFloat"/> instance with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="precision">The precision in bits of the resulting <see cref="GmpFloat"/> instance. If null, the precision of the resulting instance will be the same as the current instance.</param>
    /// <param name="rounding">The rounding mode to use for the conversion. If null, the rounding mode of the resulting instance will be the same as the current instance.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
    public GmpFloat ToGmpFloat(uint? precision = null, MpfrRounding? rounding = null)
    {
        GmpFloat rop = GmpFloat.CreateWithNullablePrecision(precision);
        ToGmpFloatInplace(rop, rounding);
        return rop;
    }

    /// <summary>
    /// Explicitly convert a <see cref="MpfrFloat"/> instance to a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="r">The <see cref="MpfrFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpFloat"/> representing the same value as <paramref name="r"/>.</returns>
    public static explicit operator GmpFloat(MpfrFloat r) => r.ToGmpFloat();

    /// <summary>
    /// Gets the maximum string length required to represent a <see cref="MpfrFloat"/> instance with the specified <paramref name="precision"/> and <paramref name="base"/>.
    /// </summary>
    /// <param name="precision">The precision of the <see cref="MpfrFloat"/> instance.</param>
    /// <param name="base">The base to use for the string representation. Must be between 2 and 62 (inclusive).</param>
    /// <returns>The maximum string length required to represent the <see cref="MpfrFloat"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="base"/> is less than 2 or greater than 62.</exception>
    public static nint GetMaxStringLength(int precision, int @base = 10)
    {
        if (@base >= 2 && @base <= 62)
        {
            return MpfrLib.mpfr_get_str_ndigits(@base, precision);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(@base));
        }
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="MpfrFloat"/> object using default format and culture-specific formatting information.
    /// </summary>
    /// <returns>A string representation of the current <see cref="MpfrFloat"/> object.</returns>
    public override string ToString() => ToString(format: null);

    /// <summary>
    /// Converts the numeric value of this instance to its equivalent string representation using the specified format and the formatting conventions of the current culture.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <returns>The string representation of the value of this instance as specified by <paramref name="format"/> and the formatting conventions of the current culture.</returns>
    public string ToString(string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Converts the value of the current <see cref="MpfrFloat"/> object to its equivalent string representation using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A standard or custom numeric format string.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current <see cref="MpfrFloat"/> object as specified by the <paramref name="format"/> and <paramref name="formatProvider"/> parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="format"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not one of the supported format strings: N, F, E, G.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        NumberFormatInfo numberFormat = (NumberFormatInfo)(formatProvider ?? Thread.CurrentThread.CurrentCulture).GetFormat(typeof(NumberFormatInfo))!;

#pragma warning disable CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
        return format switch
        {
            null or "" => Prepare(10).SplitNumberString().Format0(numberFormat),
            { Length: > 0 } x => x switch
            {
                [char c, .. var rest] => (type: char.ToUpperInvariant(c), len: int.TryParse(rest, out int r) ? new int?(r) : null)
            } switch
            {
                ('N', var len) => Prepare(10).SplitNumberString().FormatN(len ?? 2, numberFormat),
                ('F', var len) => Prepare(10).SplitNumberString().FormatF(len ?? 2, numberFormat),
                ('E', var len) c => Prepare(10).SplitNumberString().ToExpParts().FormatE(c.type, 3, len ?? 6, numberFormat),
                ('G', var len) => this switch
                {
                    var _ when CompareAbs(this, 1e-5) < 0 || CompareAbs(this, 1e16) > 0
                        => Prepare(10).SplitNumberString().ToExpParts().FormatE('e', 2, len ?? 6, numberFormat),
                    _ => Prepare(10).SplitNumberString().FormatF(len ?? 2, numberFormat),
                },
                //('C', var len) => NumberFormatter.SplitNumberString(Prepare(10)).FormatC(len ?? 2, numberFormat),
                //('D', var rest) => ToStringBase10(format, numberFormat),
                //('P', var rest) => ToStringBase10(format, numberFormat),
                //('R', var rest) => ToStringBase10(format, numberFormat),
                //('X', var rest) => ToStringBase10(format, numberFormat),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Supported: N, F, E, G"),
        };
#pragma warning restore CS8509 // switch 表达式不会处理属于其输入类型的所有可能值(它并非详尽无遗)。
    }

    /// <summary>
    /// Converts the current <see cref="MpfrFloat"/> instance to a string representation with the specified base and rounding mode.
    /// </summary>
    /// <param name="base">The base to use for the string representation. Default is 10.</param>
    /// <param name="rounding">The rounding mode to use for the conversion. Default is <see cref="DefaultRounding"/>.</param>
    /// <returns>A string representation of the current <see cref="MpfrFloat"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when unable to convert the current <see cref="MpfrFloat"/> instance to a string.</exception>
    public string ToString(int @base = 10, MpfrRounding? rounding = null)
    {
        byte[] resbuf = new byte[GetMaxStringLength(Precision, @base)];
        fixed (Mpfr_t* pthis = &Raw)
        fixed (byte* srcptr = &resbuf[0])
        {
            int exp;
            IntPtr ret = MpfrLib.mpfr_get_str((IntPtr)srcptr, (IntPtr)(&exp), @base, resbuf.Length, (IntPtr)pthis, rounding ?? DefaultRounding);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert {nameof(MpfrFloat)} to string.");
            }

            string s = Marshal.PtrToStringAnsi(ret)!;
            return GmpFloat.ToString(s, exp);
        }
    }

    private unsafe DecimalNumberString Prepare(int @base = 10, MpfrRounding? rounding = null)
    {
        byte[] resbuf = new byte[GetMaxStringLength(Precision, @base)];
        fixed (Mpfr_t* pthis = &Raw)
        fixed (byte* srcptr = &resbuf[0])
        {
            int exp;
            IntPtr ret = MpfrLib.mpfr_get_str((IntPtr)srcptr, (IntPtr)(&exp), @base, resbuf.Length, (IntPtr)pthis, rounding ?? DefaultRounding);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert {nameof(MpfrFloat)} to string.");
            }

            return new(Marshal.PtrToStringAnsi(ret)!, exp);
        }
    }

    /// <summary>
    /// Check if the current <see cref="MpfrFloat"/> instance can be safely converted to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>True if the current instance can be safely converted to an unsigned 32-bit integer, false otherwise.</returns>
    public bool FitsUInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_ulong_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> instance can be represented as a 32-bit signed integer.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>true if the current instance can be represented as a 32-bit signed integer; otherwise, false.</returns>
    public bool FitsInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_slong_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> instance can be converted to an unsigned 16-bit integer without overflow.
    /// </summary>
    /// <param name="rounding">The rounding mode to use for the conversion. If null, the default rounding mode is used.</param>
    /// <returns>true if the current instance can be converted to an unsigned 16-bit integer without overflow; otherwise, false.</returns>
    public bool FitsUInt16(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_ushort_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    /// <summary>
    /// Check if the current <see cref="MpfrFloat"/> instance can be safely converted to a 16-bit integer without losing precision.
    /// </summary>
    /// <param name="rounding">The rounding mode to use for the conversion. If null, the default rounding mode will be used.</param>
    /// <returns>True if the current instance can be safely converted to a 16-bit integer without losing precision, false otherwise.</returns>
    public bool FitsInt16(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_sshort_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current instance of <see cref="MpfrFloat"/> can be represented as an unsigned 64-bit integer.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>True if the current instance can be represented as an unsigned 64-bit integer, otherwise false.</returns>
    public bool FitsUInt64(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_uintmax_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> instance can be represented as a 64-bit integer.
    /// </summary>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>True if the current instance can be represented as a 64-bit integer, otherwise false.</returns>
    public bool FitsInt64(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_intmax_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }
    #endregion

    #region 5. Arithmetic Functions
    #region Add

    /// <summary>
    /// Adds two <see cref="MpfrFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds two <see cref="MpfrFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <see cref="DefaultPrecision"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Add(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds two <see cref="MpfrFloat"/> instances and returns the result.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, MpfrFloat op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The unsigned integer to add.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to <paramref name="op1"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to add to.</param>
    /// <param name="op2">The unsigned integer value to add.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the addition.</returns>
    public static MpfrFloat Add(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to a <see cref="MpfrFloat"/> <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to add to.</param>
    /// <param name="op2">The unsigned integer to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the addition.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, uint op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds an unsigned integer <paramref name="op1"/> to a <see cref="MpfrFloat"/> <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The unsigned integer value to add.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(uint op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    /// <summary>
    /// Adds an integer <paramref name="op2"/> to <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The integer value to add.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds an integer <paramref name="op2"/> to a <paramref name="op1"/> <see cref="MpfrFloat"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to add to.</param>
    /// <param name="op2">The integer value to add.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Add(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds an integer value to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to add to.</param>
    /// <param name="op2">The integer value to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, int op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds an integer value to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The integer value to add.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to add to.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the addition.</returns>
    public static MpfrFloat operator +(int op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    /// <summary>
    /// Adds a double-precision floating-point number <paramref name="op2"/> to <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The double-precision floating-point number to add.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds a double-precision floating-point number <paramref name="op2"/> to an <paramref name="op1"/> <see cref="MpfrFloat"/> instance and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The double-precision floating-point number to add to <paramref name="op1"/>.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Add(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds a double-precision floating-point number to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to add to.</param>
    /// <param name="op2">The double-precision floating-point number to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, double op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds a double-precision floating-point number to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The double-precision floating-point number to add.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to add to.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(double op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    /// <summary>
    /// Adds a <see cref="GmpInteger"/> <paramref name="op2"/> to a <see cref="MpfrFloat"/> <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> to store the result in.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds a <see cref="GmpInteger"/> <paramref name="op2"/> to a <see cref="MpfrFloat"/> <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to add to.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> to add.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Add(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds a <see cref="MpfrFloat"/> instance and a <see cref="GmpInteger"/> instance together.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, GmpInteger op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds a <see cref="GmpInteger"/> and a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to add.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(GmpInteger op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    /// <summary>
    /// Adds a <see cref="GmpRational"/> <paramref name="op2"/> to a <see cref="MpfrFloat"/> <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Adds a <see cref="GmpRational"/> <paramref name="op2"/> to a <see cref="MpfrFloat"/> <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Add(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Adds a <see cref="MpfrFloat"/> instance and a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to add.</param>
    /// <param name="op2">The <see cref="GmpRational"/> instance to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(MpfrFloat op1, GmpRational op2) => Add(op1, op2, op1.Precision);

    /// <summary>
    /// Adds a <see cref="GmpRational"/> and a <see cref="MpfrFloat"/> instance and returns a new <see cref="MpfrFloat"/> instance with the result.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to add.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to add.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat operator +(GmpRational op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);
    #endregion

    #region Subtract

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtracts two <see cref="MpfrFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and returns a new instance of <see cref="MpfrFloat"/> with the result.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of the result will be the maximum of the precisions of <paramref name="op1"/> and <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts two <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(MpfrFloat op1, MpfrFloat op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op1"/> from a <see cref="MpfrFloat"/> <paramref name="op2"/> and returns a new <see cref="MpfrFloat"/> instance representing the result.
    /// </summary>
    /// <param name="op1">The unsigned integer value to subtract.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to subtract from.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(uint op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    /// <summary>
    /// Subtract an unsigned integer <paramref name="op2"/> from <paramref name="op1"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The unsigned integer value to subtract.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract an unsigned integer <paramref name="op2"/> from <paramref name="op1"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The unsigned integer value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <see cref="MpfrFloat"/> <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to subtract from.</param>
    /// <param name="op2">The unsigned integer to subtract.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(MpfrFloat op1, uint op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts an integer <paramref name="op1"/> from <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The integer value to subtract from <paramref name="op2"/>.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract <paramref name="op1"/> from.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>The value of <paramref name="rop"/> will be modified to store the result of the operation.</remarks>
    public static int SubtractInplace(MpfrFloat rop, int op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_si_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract an integer <paramref name="op1"/> from a <see cref="MpfrFloat"/> <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The integer value to subtract from <paramref name="op2"/>.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to subtract <paramref name="op1"/> from.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultPrecision"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    /// <remarks>The operation is performed with the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.</remarks>
    public static MpfrFloat Subtract(int op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts an integer value from a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The integer value to subtract.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(int op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    /// <summary>
    /// Subtracts an integer value <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The integer value to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract an integer <paramref name="op2"/> from a <paramref name="op1"/> <see cref="MpfrFloat"/> instance and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The integer value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts an integer value from a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The integer value to subtract.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(MpfrFloat op1, int op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtract <paramref name="op2"/> from <paramref name="op1"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The double value to subtract from.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, double op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_d_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtracts a <paramref name="op1"/> double value from a <paramref name="op2"/> <see cref="MpfrFloat"/> value and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The double value to subtract from <paramref name="op2"/>.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to subtract <paramref name="op1"/> from.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op2"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(double op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts a <see cref="double"/> value from a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The <see cref="double"/> value to subtract.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to subtract from.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(double op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    /// <summary>
    /// Subtracts a double-precision floating-point number <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The destination <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The source <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The double-precision floating-point number to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract a double-precision floating-point number <paramref name="op2"/> from a <see cref="MpfrFloat"/> <paramref name="op1"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to subtract from.</param>
    /// <param name="op2">The double-precision floating-point number to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> is null.</exception>
    public static MpfrFloat Subtract(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts a double-precision floating-point number from a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The double-precision floating-point number to subtract.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(MpfrFloat op1, double op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtracts a <see cref="GmpInteger"/> value <paramref name="op2"/> from a <see cref="MpfrFloat"/> value <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to subtract.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtracts a <see cref="GmpInteger"/> value <paramref name="op2"/> from a <see cref="MpfrFloat"/> value <paramref name="op1"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts a <see cref="GmpInteger"/> from a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to subtract.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op1"/> and <paramref name="op2"/> have different precisions.</exception>
    /// <remarks>The precision of the result is the same as the precision of <paramref name="op1"/>.</remarks>
    public static MpfrFloat operator -(MpfrFloat op1, GmpInteger op2) => Subtract(op1, op2, op1.Precision);

    /// <summary>
    /// Subtract <paramref name="op2"/> from <paramref name="op1"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result is rounded according to the specified <paramref name="rounding"/> mode.
    /// </remarks>
    public static int SubtractInplace(MpfrFloat rop, GmpInteger op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_z_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract an <see cref="GmpInteger"/> <paramref name="op1"/> from an <see cref="MpfrFloat"/> <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to subtract from.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(GmpInteger op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts a <see cref="GmpInteger"/> from a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to subtract.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat operator -(GmpInteger op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    /// <summary>
    /// Subtract a <see cref="GmpRational"/> value <paramref name="op2"/> from a <see cref="MpfrFloat"/> value <paramref name="op1"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to subtract from.</param>
    /// <param name="op2">The <see cref="GmpRational"/> value to subtract.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Subtract a <see cref="GmpRational"/> value <paramref name="op2"/> from a <see cref="MpfrFloat"/> value <paramref name="op1"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpRational"/> value to subtract.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    public static MpfrFloat Subtract(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Subtracts a <see cref="GmpRational"/> value from a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpRational"/> value to subtract.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the subtraction.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op1"/> and <paramref name="op2"/> have different precisions.</exception>
    /// <remarks>The precision of the result is the same as the precision of <paramref name="op1"/>.</remarks>
    public static MpfrFloat operator -(MpfrFloat op1, GmpRational op2) => Subtract(op1, op2, op1.Precision);
    #endregion

    #region Multiply

    /// <summary>
    /// Multiply two <see cref="MpfrFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies two <see cref="MpfrFloat"/> instances <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    /// <remarks>The result is rounded according to the specified <paramref name="rounding"/> mode.</remarks>
    /// <seealso cref="MultiplyInplace(MpfrFloat, MpfrFloat, MpfrFloat, MpfrRounding?)"/>
    public static MpfrFloat Multiply(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiplies two <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, MpfrFloat op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiplies an <see cref="MpfrFloat"/> instance <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The unsigned integer to multiply with.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies an <see cref="MpfrFloat"/> instance <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The unsigned integer to multiply by.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiply an <see cref="MpfrFloat"/> instance by an unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The unsigned integer to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, uint op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiply an unsigned integer <paramref name="op1"/> with a <see cref="MpfrFloat"/> <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer to multiply.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(uint op1, MpfrFloat op2) => Multiply(op2, op1, op2.Precision);

    /// <summary>
    /// Multiplies an <paramref name="op1"/> <see cref="MpfrFloat"/> instance by an integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The integer value to multiply.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies an <see cref="MpfrFloat"/> instance <paramref name="op1"/> by an integer <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The integer value to multiply by.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance by an integer value.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The integer value to multiply by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, int op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiply an integer value <paramref name="op1"/> with a <see cref="MpfrFloat"/> value <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The integer value to multiply.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(int op1, MpfrFloat op2) => Multiply(op2, op1, op2.Precision);

    /// <summary>
    /// Multiplies an <paramref name="op1"/> <see cref="MpfrFloat"/> instance with a double <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The double value to multiply with.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance <paramref name="op1"/> with a double <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The double value to multiply with.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance by a double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The double-precision floating-point number to multiply by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, double op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiply a <see cref="MpfrFloat"/> instance by a double value.
    /// </summary>
    /// <param name="op1">The double value to multiply.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(double op1, MpfrFloat op2) => Multiply(op2, op1, op2.Precision);

    /// <summary>
    /// Multiplies <paramref name="op1"/> by <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to multiply.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies <paramref name="op1"/> by <paramref name="op2"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiplies two <see cref="MpfrFloat"/> instances, where the second operand is a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, GmpInteger op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiplies a <see cref="GmpInteger"/> instance with a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to multiply.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(GmpInteger op1, MpfrFloat op2) => Multiply(op2, op1, op2.Precision);

    /// <summary>
    /// Multiply <paramref name="op1"/> by <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The <see cref="GmpRational"/> instance to multiply.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies <paramref name="op1"/> with <paramref name="op2"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    /// <remarks>The result is rounded according to the specified <paramref name="rounding"/> mode.</remarks>
    /// <seealso cref="MultiplyInplace(MpfrFloat, MpfrFloat, GmpRational, MpfrRounding?)"/>
    public static MpfrFloat Multiply(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance by a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The <see cref="GmpRational"/> instance to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(MpfrFloat op1, GmpRational op2) => Multiply(op1, op2, op1.Precision);

    /// <summary>
    /// Multiplies a <see cref="GmpRational"/> instance with a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpRational"/> instance to multiply.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat operator *(GmpRational op1, MpfrFloat op2) => Multiply(op2, op1, op2.Precision);
    #endregion

    /// <summary>
    /// Computes the square of <paramref name="op1"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be squared.</param>
    /// <param name="rounding">The rounding mode to be used, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SquareInplace(MpfrFloat rop, MpfrFloat op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sqr((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the square of the given <paramref name="op1"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to square.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new <see cref="MpfrFloat"/> instance representing the square of <paramref name="op1"/>.</returns>
    public static MpfrFloat Square(MpfrFloat op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        SquareInplace(rop, op1, rounding);
        return rop;
    }

    #region Divide

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The dividend <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The divisor <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/> or <paramref name="op2"/> (whichever is greater).</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divides two <see cref="MpfrFloat"/> values.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <returns>The quotient of the division.</returns>
    public static MpfrFloat operator /(MpfrFloat op1, MpfrFloat op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divide an unsigned integer <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> to store the result.</param>
    /// <param name="op1">The unsigned integer to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide an unsigned integer <paramref name="op1"/> by a <paramref name="op2"/> <see cref="MpfrFloat"/> instance and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The unsigned integer to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to divide by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divides an unsigned integer <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat operator /(uint op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    /// <summary>
    /// Divide <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The destination <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The unsigned integer to divide by.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The unsigned integer to divide <paramref name="op1"/> by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat operator /(MpfrFloat op1, uint op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divide an integer <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The integer value to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, int op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_si_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide an integer <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The integer value to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to divide by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(int op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divides an integer value by a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The integer value to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat operator /(int op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The integer value to divide <paramref name="op1"/> by.</param>
    /// <param name="rounding">The rounding mode to be used, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by integer <paramref name="op2"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The integer value to divide <paramref name="op1"/> by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by an integer <paramref name="op2"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The integer value to divide <paramref name="op1"/> by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat operator /(MpfrFloat op1, int op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divides a double <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> to store the result in.</param>
    /// <param name="op1">The double value to be divided.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to divide by.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, double op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_d_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide a double <paramref name="op1"/> by a <see cref="MpfrFloat"/> <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The double value to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to divide by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(double op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide a double value <paramref name="op1"/> by a <see cref="MpfrFloat"/> instance <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The double value to divide.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat operator /(double op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    /// <summary>
    /// Divide <paramref name="op1"/> by a double <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The double value to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by a double <paramref name="op2"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The double value to divide by.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide a <see cref="MpfrFloat"/> instance by a double value.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The double value to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="op2"/> is zero.</exception>
    public static MpfrFloat operator /(MpfrFloat op1, double op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The dividend.</param>
    /// <param name="op2">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divides a <see cref="MpfrFloat"/> by a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to be divided.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op1"/> or <paramref name="op2"/> is not initialized.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="op1"/> precision is less than <paramref name="op2"/> precision.</exception>
    /// <remarks>The precision of the result is determined by the precision of <paramref name="op1"/>.</remarks>
    public static MpfrFloat operator /(MpfrFloat op1, GmpInteger op2) => Divide(op1, op2, op1.Precision);

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The <see cref="GmpRational"/> instance to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide <paramref name="op1"/> by <paramref name="op2"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> dividend.</param>
    /// <param name="op2">The <see cref="GmpRational"/> divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="op2"/> is zero.</exception>
    /// <remarks>The result is rounded according to the specified <paramref name="rounding"/> mode.</remarks>
    /// <seealso cref="DivideInplace(MpfrFloat, MpfrFloat, GmpRational, MpfrRounding?)"/>
    public static MpfrFloat Divide(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divides a <see cref="MpfrFloat"/> by a <see cref="GmpRational"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> to be divided.</param>
    /// <param name="op2">The <see cref="GmpRational"/> to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="op1"/> or <paramref name="op2"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="op1"/> and <paramref name="op2"/> have different precision.</exception>
    public static MpfrFloat operator /(MpfrFloat op1, GmpRational op2) => Divide(op1, op2, op1.Precision);
    #endregion

    /// <summary>
    /// Compute the square root of <paramref name="op1"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compute the square root.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SqrtInplace(MpfrFloat rop, MpfrFloat op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sqrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the square root of the specified <paramref name="op1"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compute the square root of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the square root of <paramref name="op1"/>.</returns>
    public static MpfrFloat Sqrt(MpfrFloat op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        SqrtInplace(rop, op1, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the square root of an unsigned integer <paramref name="op1"/> and stores the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op1">The unsigned integer value to calculate the square root of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SqrtInplace(MpfrFloat rop, uint op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_sqrt_ui((IntPtr)pr, op1, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the square root of a non-negative integer <paramref name="op1"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The non-negative integer to compute the square root of.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the square root of <paramref name="op1"/>.</returns>
    public static MpfrFloat Sqrt(uint op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SqrtInplace(rop, op1, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the reciprocal square of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the reciprocal square.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ReciprocalSquareInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            return MpfrLib.mpfr_rec_sqrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the reciprocal square of a <see cref="MpfrFloat"/> instance <paramref name="op"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the reciprocal square of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the reciprocal square of <paramref name="op"/>.</returns>
    public static MpfrFloat ReciprocalSquare(MpfrFloat op, int? precision, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ReciprocalSquareInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the cubic root of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cubic root.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CubicRootInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            return MpfrLib.mpfr_cbrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the cubic root of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cubic root of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the cubic root of <paramref name="op"/>.</returns>
    public static MpfrFloat CubicRoot(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CubicRootInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the <paramref name="n"/>-th root of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the root.</param>
    /// <param name="n">The positive integer root order.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int RootNInplace(MpfrFloat rop, MpfrFloat op, uint n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            return MpfrLib.mpfr_rootn_ui((IntPtr)pr, (IntPtr)p1, n, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the <paramref name="n"/>th root of <paramref name="op"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the root of.</param>
    /// <param name="n">The positive integer root to compute.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed root.</returns>
    public static MpfrFloat RootN(MpfrFloat op, uint n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RootNInplace(rop, op, n, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the <paramref name="n"/>th root of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the root.</param>
    /// <param name="n">The positive integer value of the root.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    [Obsolete("use RootN")]
    public static int RootInplace(MpfrFloat rop, MpfrFloat op, uint n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            return MpfrLib.mpfr_rootn_ui((IntPtr)pr, (IntPtr)p1, n, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the <paramref name="n"/>th root of <paramref name="op"/> and returns a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to calculate the root of.</param>
    /// <param name="n">The positive integer root to calculate.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the calculated root.</returns>
    [Obsolete("use RootN")]
    public static MpfrFloat Root(MpfrFloat op, uint n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RootNInplace(rop, op, n, rounding);
        return rop;
    }

    /// <summary>
    /// Negates the value of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to negate.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int NegateInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_neg((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Negates the given <paramref name="op"/> <see cref="MpfrFloat"/> instance and returns a new instance with the result.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to negate.</param>
    /// <param name="precision">The precision of the result. If not specified, the precision of <paramref name="op"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the negated value of <paramref name="op"/>.</returns>
    public static MpfrFloat Negate(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        NegateInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Negates the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to negate.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the negated value.</returns>
    public static MpfrFloat operator -(MpfrFloat op) => Negate(op);

    /// <summary>
    /// Computes the absolute value of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the absolute value.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AbsInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_abs((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the absolute value of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the absolute value of.</param>
    /// <param name="precision">The precision to use for the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the absolute value of <paramref name="op"/>.</returns>
    public static MpfrFloat Abs(MpfrFloat op, int? precision, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AbsInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the dimension of the rectangle defined by <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance defining the rectangle.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance defining the rectangle.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DimInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_dim((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the dimension of two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/> or <paramref name="op2"/> if they have the same precision, otherwise use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the dimension of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Dim(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DimInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiply <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_2ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply2Exp(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiply the <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be multiplied.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_2si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Multiplies a <see cref="MpfrFloat"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    public static MpfrFloat Multiply2Exp(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide a <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The power of 2 to divide <paramref name="op1"/> by.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_2ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide a <see cref="MpfrFloat"/> instance by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to divide.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision of the result, default to the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide2Exp(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide a <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The power of 2 to divide by.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_2si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Divide a <see cref="MpfrFloat"/> instance by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to divide.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="precision">The precision of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide2Exp(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the factorial of a non-negative integer <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The non-negative integer to compute the factorial of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FactorialInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_fac_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the factorial of a given unsigned integer <paramref name="op"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The unsigned integer to calculate the factorial of.</param>
    /// <param name="precision">The precision in bits of the resulting <see cref="MpfrFloat"/>. If null, the default precision will be used.</param>
    /// <param name="rounding">The rounding mode to use in the calculation. If null, the default rounding mode will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the factorial of <paramref name="op"/>.</returns>
    public static MpfrFloat Factorial(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FactorialInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the fused multiply-add of <paramref name="op1"/> and <paramref name="op2"/> and adds <paramref name="op3"/> to the result, storing the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The destination <see cref="MpfrFloat"/> to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> operand to add to the result.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FMAInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        fixed (Mpfr_t* p3 = &op3.Raw)
        {
            return MpfrLib.mpfr_fma((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, (IntPtr)p3, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the fused multiply-addition of three <see cref="MpfrFloat"/> values <paramref name="op1"/>, <paramref name="op2"/> and <paramref name="op3"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the fused multiply-addition.</returns>
    public static MpfrFloat FMA(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMAInplace(rop, op1, op2, op3, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the fused multiply-subtract operation (op1 * op2 - op3) and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> operand.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FMSInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        fixed (Mpfr_t* p3 = &op3.Raw)
        {
            return MpfrLib.mpfr_fms((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, (IntPtr)p3, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the fused multiply-subtract of three <see cref="MpfrFloat"/> values <paramref name="op1"/>, <paramref name="op2"/> and <paramref name="op3"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> value.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the fused multiply-subtract operation.</returns>
    /// <remarks>
    /// The fused multiply-subtract operation computes <c>(op1 * op2) - op3</c> as a single operation with a single rounding.
    /// </remarks>
    public static MpfrFloat FMS(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMSInplace(rop, op1, op2, op3, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the fused multiply-add operation (op1 * op2 + op3 * op4) and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op4">The fourth <see cref="MpfrFloat"/> operand.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FMMAInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        fixed (Mpfr_t* p3 = &op3.Raw)
        fixed (Mpfr_t* p4 = &op4.Raw)
        {
            return MpfrLib.mpfr_fmma((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, (IntPtr)p3, (IntPtr)p4, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the fused multiply-add operation (op1 * op2 + op3) * op4 and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <param name="op3">The third operand.</param>
    /// <param name="op4">The fourth operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the fused multiply-add operation.</returns>
    public static MpfrFloat FMMA(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMMAInplace(rop, op1, op2, op3, op4, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the fused multiply-subtract of four <see cref="MpfrFloat"/> operands <paramref name="op1"/>, <paramref name="op2"/>, <paramref name="op3"/> and <paramref name="op4"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op3">The third <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op4">The fourth <see cref="MpfrFloat"/> operand.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FMMSInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        fixed (Mpfr_t* p3 = &op3.Raw)
        fixed (Mpfr_t* p4 = &op4.Raw)
        {
            return MpfrLib.mpfr_fmms((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, (IntPtr)p3, (IntPtr)p4, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the fused multiply-subtract of four <see cref="MpfrFloat"/> operands <paramref name="op1"/>, <paramref name="op2"/>, <paramref name="op3"/> and <paramref name="op4"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <param name="op3">The third operand.</param>
    /// <param name="op4">The fourth operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of the operands is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the fused multiply-subtract operation.</returns>
    /// <remarks>
    /// The fused multiply-subtract operation is computed as (op1 * op2) - (op3 * op4).
    /// </remarks>
    /// <seealso cref="FMMSInplace(MpfrFloat, MpfrFloat, MpfrFloat, MpfrFloat, MpfrFloat, MpfrRounding?)"/>
    public static MpfrFloat FMMS(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMMSInplace(rop, op1, op2, op3, op4, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the hypotenuse of a right-angled triangle with legs <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first leg of the triangle.</param>
    /// <param name="op2">The second leg of the triangle.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int HypotInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_hypot((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hypotenuse of two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hypotenuse of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    /// <remarks>
    /// The hypotenuse is computed as sqrt(op1^2 + op2^2).
    /// </remarks>
    /// <seealso cref="HypotInplace(MpfrFloat, MpfrFloat, MpfrFloat, MpfrRounding?)"/>
    public static MpfrFloat Hypot(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        HypotInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the sum of a collection of <see cref="MpfrFloat"/> instances and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="tab">The collection of <see cref="MpfrFloat"/> instances to sum.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The <paramref name="rop"/> instance will be modified to store the result of the sum.
    /// </remarks>
    public static int SumInplace(MpfrFloat rop, IEnumerable<MpfrFloat> tab, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            GCHandle[] handles = tab.Select(x => GCHandle.Alloc(x.Raw, GCHandleType.Pinned)).ToArray();
            try
            {
                IntPtr[] ptrs = handles.Select(x => x.AddrOfPinnedObject()).ToArray();
                fixed (IntPtr* ptab = &ptrs[0])
                {
                    return MpfrLib.mpfr_sum((IntPtr)pr, (IntPtr)ptab, (uint)ptrs.Length, rounding ?? DefaultRounding);
                }
            }
            finally
            {
                foreach (GCHandle handle in handles) handle.Free();
            }
        }
    }

    /// <summary>
    /// Computes the sum of a sequence of <see cref="MpfrFloat"/> values.
    /// </summary>
    /// <param name="tab">A sequence of <see cref="MpfrFloat"/> values to calculate the sum of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of the first element in <paramref name="tab"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sum of the values in <paramref name="tab"/>.</returns>
    public static MpfrFloat Sum(IEnumerable<MpfrFloat> tab, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SumInplace(rop, tab, rounding);
        return rop;
    }
    #endregion

    #region 6. Comparison Functions
    #region Compares

    /// <summary>
    /// Compares the current <see cref="GmpFloat"/> instance with the specified object and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a valid type.</exception>
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            uint ui => Compare(this, ui),
            int i => Compare(this, i),
            double d => Compare(this, d),
            GmpFloat f => Compare(this, f),
            MpfrFloat f => Compare(this, f),
            GmpInteger z => Compare(this, z),
            GmpRational r => Compare(this, r),
            _ => throw new ArgumentException("Invalid type", nameof(obj))
        };
    }

    /// <summary>
    /// Compares this instance to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="other">The <see cref="MpfrFloat"/> to compare with this instance.</param>
    /// <returns>A signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    /// This method returns a value less than zero if this instance is less than <paramref name="other"/>,
    /// zero if this instance is equal to <paramref name="other"/>, and greater than zero if this instance is greater than <paramref name="other"/>.
    /// </remarks>
    public int CompareTo([AllowNull] MpfrFloat other) => other is null ? 1 : Compare(this, other);

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static int Compare(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compare two <see cref="MpfrFloat"/> instances and returns true if the first operand is greater than the second operand.
    /// </summary>
    /// <param name="op1">The first operand to compare.</param>
    /// <param name="op2">The second operand to compare.</param>
    /// <returns>True if <paramref name="op1"/> is greater than <paramref name="op2"/>, false otherwise.</returns>
    public static bool CompareGreater(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_greater_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> values and returns a value indicating whether one is greater than or equal to the other.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool CompareGreaterOrEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_greaterequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> values and returns a value indicating whether one is less than the other.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, false.</returns>
    public static bool CompareLess(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_less_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> values and returns a value indicating whether one is less than or equal to the other.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool CompareLessOrEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_lessequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> instances for equality.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool CompareEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_equal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> object is equal to another <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="other">The <see cref="MpfrFloat"/> to compare with the current object.</param>
    /// <returns><c>true</c> if the specified <see cref="MpfrFloat"/> is equal to the current <see cref="MpfrFloat"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(MpfrFloat? other) => (other is not null) && CompareEquals(this, other);

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, MpfrFloat op2) => CompareEquals(op1, op2);

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, MpfrFloat op2) => !CompareEquals(op1, op2);

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is greater than the second <see cref="MpfrFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, MpfrFloat op2) => CompareGreater(op1, op2);

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is less than the second <see cref="MpfrFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, MpfrFloat op2) => CompareLess(op1, op2);

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is greater than or equal to the second <see cref="MpfrFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(MpfrFloat op1, MpfrFloat op2) => CompareGreaterOrEquals(op1, op2);

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is less than or equal to the second <see cref="MpfrFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if the first <see cref="MpfrFloat"/> operand is less than or equal to the second <see cref="MpfrFloat"/> operand; otherwise, <c>false</c>.</returns>
    public static bool operator <=(MpfrFloat op1, MpfrFloat op2) => CompareLessOrEquals(op1, op2);

    /// <summary>
    /// Compares a <see cref="MpfrFloat"/> instance with an unsigned integer value.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The unsigned integer value to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static int Compare(MpfrFloat op1, uint op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_ui((IntPtr)p1, op2);
        }
    }

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is equal to a specified unsigned integer.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second unsigned integer to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, uint op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is not equal to a specified unsigned integer.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second unsigned integer to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is not equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, uint op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> object is greater than the specified unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second unsigned integer to compare.</param>
    /// <returns><c>true</c> if the value of the <paramref name="op1"/> object is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, uint op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> object is less than the specified unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second unsigned integer to compare.</param>
    /// <returns><see langword="true"/> if the value of the <paramref name="op1"/> object is less than the value of <paramref name="op2"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(MpfrFloat op1, uint op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is greater than or equal to a specified unsigned integer.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second unsigned integer to compare.</param>
    /// <returns>true if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >=(MpfrFloat op1, uint op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> object is less than or equal to the <paramref name="op2"/> value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second <see cref="uint"/> value to compare.</param>
    /// <returns>true if the value of the <paramref name="op1"/> object is less than or equal to the <paramref name="op2"/> value; otherwise, false.</returns>
    public static bool operator <=(MpfrFloat op1, uint op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether a specified unsigned integer is equal to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first unsigned integer to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> object to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(uint op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether a specified unsigned integer value is not equal to a specified <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first operand to compare.</param>
    /// <param name="op2">The second operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is not equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(uint op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether a specified unsigned integer is greater than a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first operand to compare.</param>
    /// <param name="op2">The second operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(uint op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a specified unsigned integer is less than a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first unsigned integer to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> object to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(uint op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether a specified unsigned integer is greater than or equal to a specified <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The unsigned integer to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(uint op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a specified unsigned integer is less than or equal to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first operand to compare.</param>
    /// <param name="op2">The second operand to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(uint op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares a <see cref="MpfrFloat"/> instance with an integer value.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The integer value to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static int Compare(MpfrFloat op1, int op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_si((IntPtr)p1, op2);
        }
    }

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is equal to a specified integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, int op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is not equal to a specified integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is not equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, int op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is greater than a specified integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, int op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> is less than the <paramref name="op2"/> integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns><c>true</c> if the value of the <paramref name="op1"/> is less than the <paramref name="op2"/> integer value; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, int op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is greater than or equal to a specified integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is greater than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >=(MpfrFloat op1, int op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether a specified <see cref="MpfrFloat"/> object is less than or equal to a specified integer value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second integer value to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(MpfrFloat op1, int op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether an integer value is equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if the integer value is equal to the <see cref="MpfrFloat"/> value; otherwise, <c>false</c>.</returns>
    public static bool operator ==(int op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether an integer value is not equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is not equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(int op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether an integer value is greater than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(int op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether an integer value is less than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(int op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether an integer value is greater than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(int op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether an integer value is less than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The integer value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(int op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares a <see cref="MpfrFloat"/> instance with a double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The double-precision floating-point number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static int Compare(MpfrFloat op1, double op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_d((IntPtr)p1, op2);
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="MpfrFloat"/> object is equal to the specified double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second double-precision floating-point number to compare.</param>
    /// <returns><c>true</c> if the two values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, double op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="double"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, double op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether a <see cref="MpfrFloat"/> object is greater than a double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second double-precision floating-point number to compare.</param>
    /// <returns>true if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >(MpfrFloat op1, double op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is less than the value of the second <see cref="double"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="double"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is less than the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, double op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> object is greater than or equal to the specified <paramref name="op2"/> double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second double-precision floating-point number to compare.</param>
    /// <returns>true if the value of the <paramref name="op1"/> object is greater than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >=(MpfrFloat op1, double op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> object is less than or equal to the specified <paramref name="op2"/> double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> object to compare.</param>
    /// <param name="op2">The second double-precision floating-point number to compare.</param>
    /// <returns>true if the value of the <paramref name="op1"/> object is less than or equal to the specified <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(MpfrFloat op1, double op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether a specified double-precision floating-point number is equal to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(double op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether a specified double-precision floating-point number is not equal to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is not equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(double op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether a double-precision floating-point number is greater than a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The double-precision floating-point number to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(double op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a double-precision floating-point number is less than a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first double-precision floating-point number to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> object to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(double op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether a specified double-precision floating-point number is greater than or equal to a specified <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="op1">The first double-precision floating-point number to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> object to compare.</param>
    /// <returns>true if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator >=(double op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a double-precision floating-point number is less than or equal to a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The double-precision floating-point number to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(double op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares the value of a <see cref="MpfrFloat"/> instance with a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static int Compare(MpfrFloat op1, GmpInteger op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_z((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpInteger"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpInteger"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is greater than the value of the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is greater than the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is less than the value of the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is less than the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is greater than or equal to the value of the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="op1"/> is greater than or equal to the value of <paramref name="op2"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is less than or equal to the value of the second <see cref="GmpInteger"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpInteger"/> and <see cref="MpfrFloat"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator ==(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpInteger"/> and <see cref="MpfrFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is greater than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first operand to compare.</param>
    /// <param name="op2">The second operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is less than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is greater than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpInteger"/> value is less than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> value to compare.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares two numbers, one represented by a <see cref="MpfrFloat"/> instance and the other by a <see cref="GmpRational"/> instance.
    /// </summary>
    /// <param name="op1">The first number to compare.</param>
    /// <param name="op2">The second number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static int Compare(MpfrFloat op1, GmpRational op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_q((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpRational"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpRational"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is greater than the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is greater than the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is less than the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is less than the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is greater than or equal to the value of the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is greater than or equal to the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is less than or equal to the value of the second <see cref="GmpRational"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpRational"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> and <see cref="MpfrFloat"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpRational"/> and <see cref="MpfrFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpRational"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether a <see cref="GmpRational"/> value is greater than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a <see cref="GmpRational"/> value is less than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether a <see cref="GmpRational"/> value is greater than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than or equal to <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpRational"/> value is less than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares two <see cref="MpfrFloat"/> and <see cref="GmpFloat"/> instances and returns an integer that indicates whether the first operand is less than, equal to, or greater than the second operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> instance to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static int Compare(MpfrFloat op1, GmpFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpf_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_f((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpFloat"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator ==(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="MpfrFloat"/> and <see cref="GmpFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) != 0;

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is greater than the second <see cref="GmpFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is greater than the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator >(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) > 0;

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is less than the second <see cref="GmpFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) < 0;

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is greater than or equal to the second <see cref="GmpFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is greater than or equal to the second operand; otherwise, <c>false</c>.</returns>
    public static bool operator >=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) >= 0;

    /// <summary>
    /// Determines whether the value of the first <see cref="MpfrFloat"/> operand is less than or equal to the value of the second <see cref="GmpFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="GmpFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is less than or equal to the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpFloat"/> and <see cref="MpfrFloat"/> objects have the same value.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns>true if the value of <paramref name="op1"/> is the same as the value of <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator ==(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) == 0;

    /// <summary>
    /// Determines whether two specified <see cref="GmpFloat"/> and <see cref="MpfrFloat"/> objects have different values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> to compare.</param>
    /// <returns><c>true</c> if the value of <paramref name="op1"/> is different from the value of <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) != 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> value is greater than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) < 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> value is less than a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) > 0;

    /// <summary>
    /// Determines whether the value of the <paramref name="op1"/> is greater than or equal to the value of the <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if the value of the <paramref name="op1"/> is greater than or equal to the value of the <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) <= 0;

    /// <summary>
    /// Determines whether a <see cref="GmpFloat"/> value is less than or equal to a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op1">The first value to compare.</param>
    /// <param name="op2">The second value to compare.</param>
    /// <returns>true if <paramref name="op1"/> is less than or equal to <paramref name="op2"/>; otherwise, false.</returns>
    public static bool operator <=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) >= 0;
    #endregion

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> instance is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            MpfrFloat r => this == r,
            uint ui => this == ui,
            int si => this == si,
            double dbl => this == dbl,
            GmpInteger z => this == z,
            GmpRational q => this == q,
            GmpFloat f => this == f,
            _ => false,
        };
    }

    /// <summary>
    /// Computes a hash code for the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <returns>A hash code for the current <see cref="MpfrFloat"/> instance.</returns>
    public override int GetHashCode()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return ((WindowsMpfr_t*)pthis)->GetHashCode();
            }
            else
            {
                return ((Mpfr_t*)pthis)->GetHashCode();
            }
        }
    }

    /// <summary>
    /// Compares the value of a <see cref="MpfrFloat"/> instance with a given unsigned integer multiplied by a power of 2.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The unsigned integer value to multiply by a power of 2 and compare with.</param>
    /// <param name="e">The power of 2 to multiply the unsigned integer value with.</param>
    /// <returns>A signed integer indicating the relative values of <paramref name="op1"/> and <paramref name="op2"/> * 2^<paramref name="e"/>.</returns>
    public static int Compare2Exp(MpfrFloat op1, uint op2, int e)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_ui_2exp((IntPtr)p1, op2, e);
        }
    }

    /// <summary>
    /// Compares the value of a <see cref="MpfrFloat"/> instance with a value of the form op2 * 2^e.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The integer value to multiply by 2^e and compare with <paramref name="op1"/>.</param>
    /// <param name="e">The exponent to multiply <paramref name="op2"/> by 2^e and compare with <paramref name="op1"/>.</param>
    /// <returns>A signed integer indicating the relative values of <paramref name="op1"/> and op2 * 2^e.</returns>
    /// <remarks>
    /// The return value is negative if <paramref name="op1"/> is less than op2 * 2^e, zero if <paramref name="op1"/> is equal to op2 * 2^e, and positive if <paramref name="op1"/> is greater than op2 * 2^e.
    /// </remarks>
    public static int Compare2Exp(MpfrFloat op1, int op2, int e)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_si_2exp((IntPtr)p1, op2, e);
        }
    }

    /// <summary>
    /// Compares the absolute values of two <see cref="MpfrFloat"/> numbers.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> number to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static int CompareAbs(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmpabs((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compares the absolute value of a <see cref="MpfrFloat"/> instance with an unsigned integer.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The unsigned integer to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static int CompareAbs(MpfrFloat op1, uint op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmpabs_ui((IntPtr)p1, op2);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MpfrFloat"/> instance represents a NaN (not-a-number) value.
    /// </summary>
    /// <returns><c>true</c> if this instance represents a NaN value; otherwise, <c>false</c>.</returns>
    public bool IsNaN
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_nan_p((IntPtr)pthis) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current <see cref="MpfrFloat"/> instance represents positive or negative infinity.
    /// </summary>
    /// <returns><c>true</c> if the current instance represents positive or negative infinity; otherwise, <c>false</c>.</returns>
    public bool IsInfinity
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_inf_p((IntPtr)pthis) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current instance is a number.
    /// </summary>
    /// <returns><c>true</c> if the current instance is a number; otherwise, <c>false</c>.</returns>
    public bool IsNumber
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_number_p((IntPtr)pthis) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MpfrFloat"/> instance is zero.
    /// </summary>
    /// <returns><c>true</c> if this instance is zero; otherwise, <c>false</c>.</returns>
    public bool IsZero
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_zero_p((IntPtr)pthis) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MpfrFloat"/> instance is a regular number.
    /// </summary>
    /// <remarks>
    /// A regular number is a number that is neither NaN nor infinity.
    /// </remarks>
    /// <returns><c>true</c> if this instance is a regular number; otherwise, <c>false</c>.</returns>
    public bool IsRegular
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_regular_p((IntPtr)pthis) != 0;
            }
        }
    }

    /// <summary>
    /// Gets the sign of the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <returns>
    /// A signed integer value indicating the sign of the current <see cref="MpfrFloat"/> instance.
    /// -1 if the value is negative, 0 if the value is zero, and 1 if the value is positive.
    /// </returns>
    public int Sign
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_sgn((IntPtr)pthis);
            }
        }
    }

    /// <summary>
    /// Determines whether one <see cref="MpfrFloat"/> instance is less than or greater than another <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> is less than or greater than <paramref name="op2"/>; otherwise, <c>false</c>.</returns>
    public static bool IsLessOrGreater(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_lessequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Determines whether two <see cref="MpfrFloat"/> values are unordered.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value to compare.</param>
    /// <returns><c>true</c> if <paramref name="op1"/> and <paramref name="op2"/> are unordered; otherwise, <c>false</c>.</returns>
    public static bool IsUnordered(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_unordered_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Determines whether the first <see cref="MpfrFloat"/> operand is less than or equal to the second <see cref="MpfrFloat"/> operand.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand to compare.</param>
    /// <returns><c>true</c> if the first operand is less than or equal to the second operand; otherwise, <c>false</c>.</returns>
    public static bool TotalOrderLessOrEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_total_order_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }
    #endregion

    #region 7. Transcendental Functions

    /// <summary>
    /// Calculates the natural logarithm of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the logarithm.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int LogInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the natural logarithm of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the natural logarithm of <paramref name="op"/>.</returns>
    public static MpfrFloat Log(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the natural logarithm of an unsigned integer <paramref name="op"/> and stores the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The unsigned integer operand to compute the natural logarithm of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int LogInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_log_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the natural logarithm of a specified unsigned integer <paramref name="op"/> and returns a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The unsigned integer value to compute the natural logarithm of.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the natural logarithm of <paramref name="op"/>.</returns>
    public static MpfrFloat Log(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        LogInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the base-2 logarithm of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the logarithm of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Log2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the base-2 logarithm of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the base-2 logarithm of <paramref name="op"/>.</returns>
    public static MpfrFloat Log2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log2Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the base-10 logarithm of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the logarithm.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Log10Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log10((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the base-10 logarithm of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the base-10 logarithm of <paramref name="op"/>.</returns>
    public static MpfrFloat Log10(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log10Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the natural logarithm of 1 plus <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the natural logarithm of 1 plus.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int LogP1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log1p((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the natural logarithm of (1 + <paramref name="op"/>) and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the natural logarithm of (1 + <paramref name="op"/>).</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the natural logarithm of (1 + <paramref name="op"/>).</returns>
    public static MpfrFloat LogP1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogP1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the logarithm of <paramref name="op"/> to the base 2 and adds 1 to the result, storing the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the logarithm of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Log2P1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log2p1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the natural logarithm of 1 plus the input <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> to compute the logarithm of 1 plus.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the natural logarithm of 1 plus the input <paramref name="op"/>.</returns>
    public static MpfrFloat Log2P1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log2P1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the logarithm base 10 of (1 + <paramref name="op"/>) and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Log10P1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log10p1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the natural logarithm of 1 plus the input <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> instance.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the natural logarithm of 1 plus the input <paramref name="op"/>.</returns>
    public static MpfrFloat Log10P1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log10P1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the exponential function of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the exponential function.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ExpInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the exponential function of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> value to calculate the exponential function of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the exponential function.</returns>
    public static MpfrFloat Exp(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ExpInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the base-2 exponential function of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the exponential function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Exp2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the base-2 exponential function of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to calculate the exponential function of.</param>
    /// <param name="precision">The precision of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the exponential function.</returns>
    public static MpfrFloat Exp2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp2Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates the base-10 exponential function of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the exponential function.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Exp10Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp10((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the 10 raised to the power of <paramref name="op"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the power of 10.</param>
    /// <param name="precision">The precision of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of 10 raised to the power of <paramref name="op"/>.</returns>
    public static MpfrFloat Exp10(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp10Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the value of e raised to the power of <paramref name="op"/> minus 1, and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the exponent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ExpM1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_expm1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the value of e raised to the power of <paramref name="op"/> minus 1, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> exponent.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the computation.</returns>
    public static MpfrFloat ExpM1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ExpM1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Calculates 2^(x)-1 and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the exponent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Exp2M1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp2m1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the value of 2^x - 1 for the given <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the value of 2^x - 1 for.</param>
    /// <param name="precision">The precision in bits to use for the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value of 2^x - 1.</returns>
    public static MpfrFloat Exp2M1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp2M1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the exponential of 10 minus 1 and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the exponential of 10 minus 1.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result is computed as exp(10) - 1, with a relative error of less than 1 ulp (unit in the last place).
    /// </remarks>
    public static int Exp10M1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp10m1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the value of 10^<paramref name="op"/> - 1 and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the value of 10^<paramref name="op"/> - 1 from.</param>
    /// <param name="precision">The precision in bits of the resulting <see cref="MpfrFloat"/> instance. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the value of 10^<paramref name="op"/> - 1.</returns>
    public static MpfrFloat Exp10M1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp10M1Inplace(rop, op, rounding);
        return rop;
    }

    #region Power

    /// <summary>
    /// Computes the power of <paramref name="op1"/> to <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_pow((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> raised to the <paramref name="op2"/> power.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/>.</param>
    /// <param name="op2">The exponent <see cref="MpfrFloat"/>.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat Power(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> raised to the <paramref name="op2"/> power.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/>.</param>
    /// <param name="op2">The exponent <see cref="MpfrFloat"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat operator ^(MpfrFloat op1, MpfrFloat op2) => Power(op1, op2);

    /// <summary>
    /// Raises <paramref name="op1"/> to the power of <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int PowerRInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_powr((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> raised to the <paramref name="op2"/> power, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="MpfrFloat"/> instance.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat PowerR(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerRInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Raises <paramref name="op1"/> to the power of <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent as an unsigned integer.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result of the operation is rounded according to the specified <paramref name="rounding"/> mode.
    /// </remarks>
    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_pow_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> to the <paramref name="op2"/> integer, and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The base value.</param>
    /// <param name="op2">The exponent value.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat Power(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the power of a <see cref="MpfrFloat"/> instance to an unsigned integer exponent.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The unsigned integer exponent.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat operator ^(MpfrFloat op1, uint op2) => Power(op1, op2);

    /// <summary>
    /// Calculates the power of <paramref name="op1"/> to the integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The integer exponent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result is rounded according to the specified <paramref name="rounding"/> mode.
    /// </remarks>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_pow_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the power of <paramref name="op1"/> to the integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The integer exponent.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the calculated power.</returns>
    public static MpfrFloat Power(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the power of a <see cref="MpfrFloat"/> instance to an integer exponent.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The integer exponent.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat operator ^(MpfrFloat op1, int op2) => Power(op1, op2);

    /// <summary>
    /// Calculates the power of <paramref name="op1"/> to the integer <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="GmpInteger"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_pow_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> raised to the <paramref name="op2"/> integer, and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="GmpInteger"/> instance.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op1"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the power of <paramref name="op1"/> raised to the <paramref name="op2"/> integer.</returns>
    public static MpfrFloat Power(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> raised to the <paramref name="op2"/> integer exponent.
    /// </summary>
    /// <param name="op1">The base <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The exponent <see cref="GmpInteger"/> instance.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the power of <paramref name="op1"/> raised to the <paramref name="op2"/> integer exponent.</returns>
    public static MpfrFloat operator ^(MpfrFloat op1, GmpInteger op2) => Power(op1, op2);

    /// <summary>
    /// Raises an unsigned integer <paramref name="op1"/> to the power of another unsigned integer <paramref name="op2"/> and stores the result in-place in the <paramref name="rop"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op1">The base unsigned integer.</param>
    /// <param name="op2">The exponent unsigned integer.</param>
    /// <param name="rounding">The optional rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result of the operation is computed as <paramref name="rop"/> = <paramref name="op1"/>^<paramref name="op2"/>.
    /// </remarks>
    public static int PowerInplace(MpfrFloat rop, uint op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_ui_pow_ui((IntPtr)pr, op1, op2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of <paramref name="op1"/> to the <paramref name="op2"/> exponent, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op1">The base value.</param>
    /// <param name="op2">The exponent value.</param>
    /// <param name="precision">The precision in bits, or <see langword="null"/> to use default precision.</param>
    /// <param name="rounding">The rounding mode, or <see langword="null"/> to use default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat Power(uint op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Raises an unsigned integer <paramref name="op1"/> to the power of <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The unsigned integer base.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> exponent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int PowerInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_pow((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the power of a <paramref name="op1"/> to a <paramref name="op2"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The base value.</param>
    /// <param name="op2">The exponent value.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat Power(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the power of an unsigned integer <paramref name="op1"/> to a <see cref="MpfrFloat"/> <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The unsigned integer base.</param>
    /// <param name="op2">The <see cref="MpfrFloat"/> exponent.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the power operation.</returns>
    public static MpfrFloat operator ^(uint op1, MpfrFloat op2) => Power(op1, op2);
    #endregion

    /// <summary>
    /// Compound an integer <paramref name="n"/> with <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compound with.</param>
    /// <param name="n">The integer value to compound with <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CompoundInplace(MpfrFloat rop, MpfrFloat op, int n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_compound_si((IntPtr)pr, (IntPtr)pop, n, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the compound interest of <paramref name="op"/> with <paramref name="n"/> periods and returns a new <see cref="MpfrFloat"/> instance representing the result.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance representing the principal amount.</param>
    /// <param name="n">The number of periods.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the compound interest of <paramref name="op"/> with <paramref name="n"/> periods.</returns>
    public static MpfrFloat Compound(MpfrFloat op, int n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CompoundInplace(rop, op, n, rounding);
        return rop;
    }

    #region Trigonometric function

    /// <summary>
    /// Compute the cosine of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosine.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CosInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cos((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the cosine of a <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosine of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the cosine of <paramref name="op"/>.</returns>
    public static MpfrFloat Cos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the sine of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SinInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sin((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the sine of a <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sine of <paramref name="op"/>.</returns>
    public static MpfrFloat Sin(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the tangent of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the tangent.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int TanInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tan((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the tangent of a specified <paramref name="op"/> <see cref="MpfrFloat"/> number.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> number to compute the tangent of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the tangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Tan(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the cosine of <paramref name="op"/> in radians, with the result stored in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosine of.</param>
    /// <param name="u">The number of bits of the angle to use, default to 360.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CosUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cosu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the cosine of <paramref name="op"/> multiplied by <paramref name="u"/> degrees, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to compute the cosine of.</param>
    /// <param name="u">The degree to multiply <paramref name="op"/> by.</param>
    /// <param name="precision">The precision in bits to use for the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed cosine value.</returns>
    public static MpfrFloat CosU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the sine of <paramref name="op"/> in place and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine.</param>
    /// <param name="u">The angle unit in degrees, default to 360.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SinUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the sine of <paramref name="op"/> in degrees, with a period of <paramref name="u"/> degrees.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine of.</param>
    /// <param name="u">The period of the sine function in degrees.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sine of <paramref name="op"/>.</returns>
    /// <remarks>
    /// The result is computed with a precision of <paramref name="precision"/> bits and rounded according to <paramref name="rounding"/>.
    /// </remarks>
    public static MpfrFloat SinU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the tangent of <paramref name="op"/> multiplied by <paramref name="u"/> in unsigned mode and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the tangent.</param>
    /// <param name="u">The multiplier for <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int TanUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the tangent of <paramref name="op"/> multiplied by <paramref name="u"/> in unsigned mode.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="u">The unsigned integer multiplier.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the rounding mode of <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed tangent.</returns>
    public static MpfrFloat TanU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the cosine of pi times <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosine of pi times.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CosPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cospi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the cosine of pi times <paramref name="op"/> and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the cosine of pi times <paramref name="op"/>.</returns>
    public static MpfrFloat CosPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the sine of pi times <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine of pi times.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SinPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the sine of pi times <paramref name="op"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the sine of pi times <paramref name="op"/>.</returns>
    public static MpfrFloat SinPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the tangent of pi times <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the tangent of pi times.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int TanPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the tangent of pi times <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed tangent.</returns>
    /// <remarks>
    /// The result is computed as sin(pi * <paramref name="op"/>) / cos(pi * <paramref name="op"/>).
    /// </remarks>
    public static MpfrFloat TanPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the sine and cosine of <paramref name="op"/> and store the results in <paramref name="sop"/> and <paramref name="cop"/> respectively.
    /// </summary>
    /// <param name="sop">The <see cref="MpfrFloat"/> instance to store the sine result.</param>
    /// <param name="cop">The <see cref="MpfrFloat"/> instance to store the cosine result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the sine and cosine.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Return 0 iff both results are exact, more precisely it returns s + 4c where s = 0 if sop is exact, s = 1 if sop is larger than the sine of op, s = 2 if sop is smaller than the sine of op, and similarly for c and the cosine of op.
    /// </returns>
    /// <remarks>
    /// The computation is performed in-place, i.e., the value of <paramref name="op"/> may be modified.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sop"/>, <paramref name="cop"/> or <paramref name="op"/> is null.</exception>
    public static int SinCosInplace(MpfrFloat sop, MpfrFloat cop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* psin = &sop.Raw)
        fixed (Mpfr_t* pcos = &cop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sin_cos((IntPtr)psin, (IntPtr)pcos, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the sine and cosine of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute sine and cosine for.</param>
    /// <param name="precision">The precision in bits to use for the computation. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If not specified, the default rounding mode is used.</param>
    /// <returns>A tuple containing the computed sine and cosine values as <see cref="MpfrFloat"/> instances.</returns>
    public static (MpfrFloat sin, MpfrFloat cos) SinCos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat sop = new(precision ?? op.Precision);
        MpfrFloat cop = new(precision ?? op.Precision);
        SinCosInplace(sop, cop, op, rounding);
        return (sop, cop);
    }

    /// <summary>
    /// Compute the secant of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the secant.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SecInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sec((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the secant of a specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the secant of <paramref name="op"/>.</returns>
    public static MpfrFloat Sec(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SecInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the cosecant of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosecant of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CscInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_csc((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the cosecant of a <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cosecant of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the cosecant of <paramref name="op"/>.</returns>
    public static MpfrFloat Csc(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CscInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the cotangent of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the cotangent.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CotInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cot((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Calculates the cotangent of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to calculate the cotangent of.</param>
    /// <param name="precision">The precision of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the cotangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Cot(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CotInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse cosine of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse cosine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AcosInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acos((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse cosine of a <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse cosine of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse cosine of <paramref name="op"/>.</returns>
    public static MpfrFloat Acos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse sine of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse sine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AsinInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asin((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse sine of <paramref name="op"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse sine of <paramref name="op"/>.</returns>
    public static MpfrFloat Asin(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the arctangent of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the arctangent.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AtanInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atan((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the arctangent of <paramref name="op"/> and return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> instance.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the arctangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Atan(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse cosine of <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse cosine of.</param>
    /// <param name="u">The maximum value of the input <paramref name="op"/> in unsigned integer format.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AcosUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acosu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the inverse cosine of <paramref name="op"/> in degrees, with optional <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> value.</param>
    /// <param name="u">The maximum value of the input in degrees, default to 360.</param>
    /// <param name="precision">The precision in bits, default to the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use, default to the rounding mode of <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse cosine of <paramref name="op"/>.</returns>
    public static MpfrFloat AcosU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse sine of <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse sine of.</param>
    /// <param name="u">The upper bound of the input value, default to 360.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AsinUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse sine of <paramref name="op"/> in degrees, with a maximum value of <paramref name="u"/> degrees.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="u">The maximum value of the inverse sine in degrees.</param>
    /// <param name="precision">The precision of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse sine of <paramref name="op"/>.</returns>
    public static MpfrFloat AsinU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse tangent of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse tangent.</param>
    /// <param name="u">The unsigned integer value to specify the base of the input.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AtanUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse tangent of <paramref name="op"/> multiplied by <paramref name="u"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse tangent of.</param>
    /// <param name="u">The multiplier to apply to <paramref name="op"/> before computing the inverse tangent.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed inverse tangent.</returns>
    public static MpfrFloat AtanU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse cosine of <paramref name="op"/> multiplied by pi, and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse cosine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AcosPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acospi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse cosine of <paramref name="op"/> multiplied by pi.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the operation.</returns>
    /// <remarks>
    /// The result is in the range [0, 1].
    /// </remarks>
    public static MpfrFloat AcosPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse sine of <paramref name="op"/> multiplied by pi and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse sine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AsinPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the inverse sine of <paramref name="op"/> multiplied by pi.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse sine of <paramref name="op"/> multiplied by pi.</returns>
    public static MpfrFloat AsinPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the arctangent of <paramref name="op"/> and multiplies the result by 1/π, storing the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the arctangent of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AtanPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the arctangent of <paramref name="op"/> divided by pi, return the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the arctangent of.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed arctangent of <paramref name="op"/> divided by pi.</returns>
    public static MpfrFloat AtanPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the arctangent of the quotient of two specified <see cref="MpfrFloat"/> numbers and stores the result in the specified <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> to store the result in.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> representing the numerator.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> representing the denominator.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The arctangent is the angle whose tangent is the quotient of the two specified numbers. The return value is expressed in radians.
    /// </remarks>
    public static int Atan2Inplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2((IntPtr)pr, (IntPtr)py, (IntPtr)px, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the arctangent of the quotient of its arguments, with the result in radians.
    /// </summary>
    /// <param name="y">The numerator of the quotient.</param>
    /// <param name="x">The denominator of the quotient.</param>
    /// <param name="precision">The precision in bits of the result. If null, use <see cref="DefaultPrecision"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the arctangent of the quotient of its arguments.</returns>
    public static MpfrFloat Atan2(MpfrFloat y, MpfrFloat x, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2Inplace(rop, y, x, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the arc tangent of y/x using unsigned arguments and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> instance representing the y-coordinate.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> instance representing the x-coordinate.</param>
    /// <param name="u">The number of bits to use for the result.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The result is in the range [0, 2^u-1].
    /// </remarks>
    public static int Atan2UInplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2u((IntPtr)pr, (IntPtr)py, (IntPtr)px, u, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the arc tangent of <paramref name="y"/>/<paramref name="x"/> with unsigned result in degrees, and stores the result in a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="y">The numerator of the fraction y/x.</param>
    /// <param name="x">The denominator of the fraction y/x.</param>
    /// <param name="u">The number of degrees in a circle. Default is 360.</param>
    /// <param name="precision">The precision of the result in bits. Default is <see cref="MpfrFloat.DefaultPrecision"/>.</param>
    /// <param name="rounding">The rounding mode to use. Default is <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed arc tangent.</returns>
    /// <remarks>
    /// The result is in the range [0, <paramref name="u"/>).
    /// </remarks>
    public static MpfrFloat Atan2U(MpfrFloat y, MpfrFloat x, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2UInplace(rop, y, x, u, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the inverse tangent of <paramref name="y"/> over <paramref name="x"/> and multiply the result by 1/π, store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> instance representing the numerator of the tangent.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> instance representing the denominator of the tangent.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Atan2PiInplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2pi((IntPtr)pr, (IntPtr)py, (IntPtr)px, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the value of atan2(y, x) in the range [0, 1) and stores the result in a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="precision">The precision in bits of the result. If null, use <see cref="DefaultPrecision"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the value of atan2(y, x) in the range [0, 1).</returns>
    public static MpfrFloat Atan2Pi(MpfrFloat y, MpfrFloat x, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2PiInplace(rop, y, x, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the hyperbolic cosine of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic cosine.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CoshInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cosh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic cosine of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic cosine of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic cosine of <paramref name="op"/>.</returns>
    public static MpfrFloat Cosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CoshInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the hyperbolic sine of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic sine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SinhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the hyperbolic sine of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic sine of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic sine of <paramref name="op"/>.</returns>
    public static MpfrFloat Sinh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinhInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the hyperbolic tangent of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic tangent.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int TanhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic tangent of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic tangent of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic tangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Tanh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanhInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the hyperbolic sine and cosine of <paramref name="op"/> and store the results in <paramref name="sop"/> and <paramref name="cop"/> respectively.
    /// </summary>
    /// <param name="sop">The <see cref="MpfrFloat"/> to store the hyperbolic sine result.</param>
    /// <param name="cop">The <see cref="MpfrFloat"/> to store the hyperbolic cosine result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> to compute the hyperbolic sine and cosine.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SinhCoshInplace(MpfrFloat sop, MpfrFloat cop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* psin = &sop.Raw)
        fixed (Mpfr_t* pcos = &cop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinh_cosh((IntPtr)psin, (IntPtr)pcos, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic sine and cosine of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic sine and cosine of.</param>
    /// <param name="precision">The precision in bits to use for the computation. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If null, the default rounding mode is used.</param>
    /// <returns>A tuple containing the hyperbolic sine and cosine of <paramref name="op"/> as <see cref="MpfrFloat"/> instances.</returns>
    public static (MpfrFloat sinh, MpfrFloat cosh) SinhCosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat sop = new(precision ?? op.Precision);
        MpfrFloat cop = new(precision ?? op.Precision);
        SinhCoshInplace(sop, cop, op, rounding);
        return (sop, cop);
    }

    /// <summary>
    /// Compute the hyperbolic secant of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic secant.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int SechInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sech((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic secant of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic secant of <paramref name="op"/>.</returns>
    public static MpfrFloat Sech(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SechInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the hyperbolic cosecant of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic cosecant of.</param>
    /// <param name="rounding">The rounding mode to use (optional, defaults to <see cref="DefaultRounding"/>).</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CschInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_csch((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic cosecant of a specified <paramref name="op"/> <see cref="MpfrFloat"/> number.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> number to compute the hyperbolic cosecant of.</param>
    /// <param name="precision">The precision, in bits, of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic cosecant of <paramref name="op"/>.</returns>
    public static MpfrFloat Csch(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CschInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the hyperbolic cotangent of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the hyperbolic cotangent.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CothInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_coth((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the hyperbolic cotangent of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the hyperbolic cotangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Coth(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CothInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the inverse hyperbolic cosine of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse hyperbolic cosine of.</param>
    /// <param name="rounding">The rounding mode to use (optional, defaults to <see cref="DefaultRounding"/>).</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AcoshInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acosh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse hyperbolic cosine of a <see cref="MpfrFloat"/> value.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> value to compute the inverse hyperbolic cosine of.</param>
    /// <param name="precision">The precision, in bits, of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse hyperbolic cosine of <paramref name="op"/>.</returns>
    public static MpfrFloat Acosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcoshInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the inverse hyperbolic sine of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse hyperbolic sine of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AsinhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse hyperbolic sine of a <see cref="MpfrFloat"/> <paramref name="op"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse hyperbolic sine of <paramref name="op"/>.</returns>
    public static MpfrFloat Asinh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinhInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the inverse hyperbolic tangent of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the inverse hyperbolic tangent of.</param>
    /// <param name="rounding">The rounding mode to use (optional, defaults to <see cref="DefaultRounding"/>).</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AtanhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the inverse hyperbolic tangent of a <see cref="MpfrFloat"/> <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the inverse hyperbolic tangent of <paramref name="op"/>.</returns>
    public static MpfrFloat Atanh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanhInplace(rop, op, rounding);
        return rop;
    }
    #endregion

    /// <summary>
    /// Computes the nearest integer to <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the nearest integer from.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int EintInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_eint((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the integral part of <paramref name="op"/> and stores the result in a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the integral part of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the integral part of <paramref name="op"/>.</returns>
    /// <remarks>
    /// The integral part of a number is the largest integer less than or equal to the number. For example, the integral part of 3.14 is 3, and the integral part of -3.14 is -4.
    /// </remarks>
    public static MpfrFloat Eint(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        EintInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the logarithm integral of <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm integral of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Li2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_li2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the dilogarithm function of <paramref name="op"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The input value to compute the dilogarithm function.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the dilogarithm function.</returns>
    public static MpfrFloat Li2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Li2Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the gamma function of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the gamma function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int GammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_gamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the gamma function of the specified <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the gamma function on.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the gamma function of <paramref name="op"/>.</returns>
    public static MpfrFloat Gamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        GammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the incomplete gamma function and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The first <see cref="MpfrFloat"/> argument.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> argument.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int GammaIncInplace(MpfrFloat rop, MpfrFloat op, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        fixed (Mpfr_t* pop2 = &op2.Raw)
        {
            return MpfrLib.mpfr_gamma_inc((IntPtr)pr, (IntPtr)pop, (IntPtr)pop2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the incomplete gamma function with the first argument <paramref name="op"/> and the second argument <paramref name="op2"/>.
    /// </summary>
    /// <param name="op">The first argument of the incomplete gamma function.</param>
    /// <param name="op2">The second argument of the incomplete gamma function.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the incomplete gamma function.</returns>
    public static MpfrFloat GammaInc(MpfrFloat op, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        GammaIncInplace(rop, op, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the natural logarithm of the absolute value of the gamma function of <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of the gamma function of.</param>
    /// <param name="rounding">The rounding mode to use (defaults to <see cref="DefaultRounding"/> if not specified).</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int LogGammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_lngamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the logarithm of the absolute value of the gamma function of <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of the absolute value of the gamma function of.</param>
    /// <param name="precision">The precision in bits to use for the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value.</returns>
    public static MpfrFloat LogGamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogGammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the natural logarithm of the absolute value of the gamma function of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the gamma function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>A tuple containing the sign of the gamma function and the return value of the mpfr_lgamma function.</returns>
    public static (int sign, int round) LGammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            int sign;
            int round = MpfrLib.mpfr_lgamma((IntPtr)pr, (IntPtr)(&sign), (IntPtr)pop, rounding ?? DefaultRounding);
            return (sign, round);
        }
    }

    /// <summary>
    /// Compute the logarithm of the absolute value of the gamma function of <paramref name="op"/> and return the sign, value and rounding mode used.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the logarithm of the absolute value of the gamma function.</param>
    /// <param name="precision">The precision in bits to use for the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A tuple containing the sign of the result, a new instance of <see cref="MpfrFloat"/> representing the computed value and the rounding mode used.</returns>
    /// <remarks>
    /// The computation is performed in-place on a new instance of <see cref="MpfrFloat"/> with the specified <paramref name="precision"/> and <paramref name="rounding"/>.
    /// </remarks>
    public static (int sign, MpfrFloat value, int round) LGamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        (int sign, int round) = LGammaInplace(rop, op, rounding);
        return (sign, rop, round);
    }

    /// <summary>
    /// Compute the digamma function of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the digamma function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int DigammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_digamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the digamma function of the given <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the digamma function of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the digamma function.</returns>
    public static MpfrFloat Digamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        DigammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the beta function of <paramref name="op1"/> and <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int BetaInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_beta((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the beta function of <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op1">The first argument of the beta function.</param>
    /// <param name="op2">The second argument of the beta function.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op1"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the beta function.</returns>
    public static MpfrFloat Beta(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        BetaInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Riemann zeta function at <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Riemann zeta function at.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ZetaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_zeta((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the Riemann zeta function of <paramref name="op"/> and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> to compute the Riemann zeta function of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the Riemann zeta function of <paramref name="op"/>.</returns>
    public static MpfrFloat Zeta(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ZetaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Riemann zeta function at integer <paramref name="op"/> in place and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The integer argument of the Riemann zeta function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ZetaInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_zeta_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the Riemann zeta function at the given positive integer <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The positive integer argument of the Riemann zeta function.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the computation.</returns>
    public static MpfrFloat Zeta(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ZetaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the error function of <paramref name="op"/> and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the error function of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ErrorFunctionInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_erf((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the error function of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> operand.</param>
    /// <param name="precision">The precision of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the error function of <paramref name="op"/>.</returns>
    public static MpfrFloat ErrorFunction(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ErrorFunctionInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the complementary error function of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the complementary error function of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ComplementaryErrorFunctionInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_erfc((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the complementary error function of the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to compute the complementary error function of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the complementary error function of <paramref name="op"/>.</returns>
    public static MpfrFloat ComplementaryErrorFunction(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ComplementaryErrorFunctionInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Bessel function of the first kind of order zero of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the first kind of order zero.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int J0Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_j0((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the Bessel function of the first kind of order zero (J0) of the <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the first kind of order zero (J0) from.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the Bessel function of the first kind of order zero (J0) of the <paramref name="op"/> <see cref="MpfrFloat"/> instance.</returns>
    public static MpfrFloat J0(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        J0Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Bessel function of the first kind of order one for <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the first kind of order one.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int J1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_j1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the Bessel function of the first kind of order one for the <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the first kind of order one for.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value.</returns>
    public static MpfrFloat J1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        J1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the nth order Bessel function of the first kind in place and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="n">The order of the Bessel function.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int JNInplace(MpfrFloat rop, int n, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_jn((IntPtr)pr, n, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the n-th order Bessel function of the first kind, Jn(op), and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="n">The order of the Bessel function.</param>
    /// <param name="op">The input value to compute the Bessel function.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value of Jn(op).</returns>
    public static MpfrFloat JN(int n, MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        JNInplace(rop, n, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Bessel function of the second kind of order 0 of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the second kind of order 0.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Y0Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_y0((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the Bessel function of the second kind of order 0 of <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the second kind of order 0.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the Bessel function of the second kind of order 0 of <paramref name="op"/>.</returns>
    /// <remarks>
    /// The Bessel function of the second kind of order 0 is defined as the solution of the Bessel differential equation:
    /// x^2*y'' + x*y' + (x^2 - 1)y = 0
    /// </remarks>
    public static MpfrFloat Y0(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Y0Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Bessel function of the second kind of order 1 for <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function of the second kind of order 1.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int Y1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_y1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the Y1 function of the <paramref name="op"/> <see cref="MpfrFloat"/> instance and return the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Y1 function on.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the Y1 function.</returns>
    public static MpfrFloat Y1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Y1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the n-th order Bessel function of the second kind, Y_n(op), in-place and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="n">The order of the Bessel function.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Bessel function on.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int YNInplace(MpfrFloat rop, int n, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_yn((IntPtr)pr, n, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the n-th order Bessel function of the second kind, Yn(op), and store the result in a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="n">The order of the Bessel function.</param>
    /// <param name="op">The input value to compute the Bessel function.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="op"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value of Yn(op).</returns>
    public static MpfrFloat YN(int n, MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        YNInplace(rop, n, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the arithmetic-geometric mean of <paramref name="op1"/> and <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AGMInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_agm((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the arithmetic-geometric mean of two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of the result will be the maximum of the precisions of <paramref name="op1"/> and <paramref name="op2"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the arithmetic-geometric mean of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat AGM(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AGMInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Airy function Ai(x) and store the result in-place in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the Airy function on.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int AiryInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_ai((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the Airy function of the first kind Ai(x) for the given <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> to compute the Airy function for.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If not specified, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed Airy function.</returns>
    public static MpfrFloat Airy(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AiryInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the logarithm of 2 and stores the result in-place in the <paramref name="rop"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ConstLog2Inplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_log2((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the constant logarithm of 2 with the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the constant logarithm of 2.</returns>
    public static MpfrFloat ConstLog2(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstLog2Inplace(rop, rounding);
        return rop;
    }

    /// <summary>
    /// Set the value of <paramref name="rop"/> to the mathematical constant pi, using the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to set the value of.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ConstPiInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_pi((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the mathematical constant pi with the specified <paramref name="precision"/> and <paramref name="rounding"/> mode, and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="precision">The precision in bits of the result. If null, the default precision is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the value of pi.</returns>
    public static MpfrFloat ConstPi(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstPiInplace(rop, rounding);
        return rop;
    }

    /// <summary>
    /// Set the value of <paramref name="rop"/> to the mathematical constant Euler's number (e) with the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to set the value to Euler's number.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ConstEulerInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_euler((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Returns a new instance of <see cref="MpfrFloat"/> representing the Euler's constant with the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="precision">The precision in bits of the returned value. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the Euler's constant.</returns>
    public static MpfrFloat ConstEuler(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstEulerInplace(rop, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the Catalan's constant and store the result in-place in the <paramref name="rop"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ConstCatalanInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_catalan((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the mathematical constant Catalan to the specified <paramref name="precision"/> and <paramref name="rounding"/> mode, and returns the result as a new instance of <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="precision">The precision in bits to use for the computation. If null, the default precision is used.</param>
    /// <param name="rounding">The rounding mode to use for the computation. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the computed value of Catalan.</returns>
    public static MpfrFloat ConstCatalan(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstCatalanInplace(rop, rounding);
        return rop;
    }
    #endregion

    #region 10. Integer and Remainder Related Functions

    /// <summary>
    /// Rounds the <paramref name="op"/> to the nearest integer and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <returns>The sign of the rounded value.</returns>
    /// <remarks>
    /// The function rounds the <paramref name="op"/> to the nearest integer, using the specified <paramref name="rounding"/> mode, and stores the result in <paramref name="rop"/>. 
    /// If the fractional part of <paramref name="op"/> is exactly 0.5, the rounding is done away from zero.
    /// </remarks>
    public static int RIntInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> to the nearest integer using the specified <paramref name="rounding"/> mode and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <param name="precision">The precision of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the rounded value.</returns>
    public static MpfrFloat RInt(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the ceiling of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the ceiling of.</param>
    /// <returns>0 if the operation succeeded, or a non-zero value otherwise.</returns>
    public static int CeilingInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_ceil((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Computes the smallest integral value greater than or equal to the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the ceiling value of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the ceiling value of <paramref name="op"/>.</returns>
    public static MpfrFloat Ceiling(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CeilingInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Computes the largest integer value less than or equal to the specified <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the floor value.</param>
    /// <returns>Returns 0 if the operation is successful, -1 otherwise.</returns>
    public static int FloorInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_floor((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Computes the largest integral value less than or equal to the specified <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the floor value of.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the floor value of <paramref name="op"/>.</returns>
    /// <remarks>
    /// The result is rounded towards negative infinity.
    /// </remarks>
    public static MpfrFloat Floor(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> value and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to be rounded.</param>
    /// <returns>The rounding mode used for the operation.</returns>
    public static int RoundInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_round((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> <see cref="MpfrFloat"/> to the specified <paramref name="precision"/> if provided, or to its original precision if not.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> to round.</param>
    /// <param name="precision">The precision to round to, in bits. If null, the original precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the rounded value.</returns>
    public static MpfrFloat Round(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> to the nearest even integer and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <returns>The rounding direction, as an integer in the range [-1, 1].</returns>
    public static int RoundEvenInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_roundeven((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> <see cref="MpfrFloat"/> instance to the nearest even value with the specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="precision">The precision to use for the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the rounded value.</returns>
    public static MpfrFloat RoundEven(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundEvenInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Truncates the value of <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the truncated result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to be truncated.</param>
    /// <returns>Returns 0 if the operation is successful, otherwise returns a non-zero value.</returns>
    public static int TruncateInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_trunc((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Truncates the given <paramref name="op"/> <see cref="MpfrFloat"/> to an integer value using round-to-even rounding mode, and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to truncate.</param>
    /// <param name="precision">The precision of the result, default to the precision of <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the truncated value of <paramref name="op"/>.</returns>
    /// <remarks>
    /// The truncation is performed using round-to-even rounding mode.
    /// </remarks>
    public static MpfrFloat Truncate(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundEvenInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Rounds the value of <paramref name="op"/> to the nearest integer greater than or equal to it, and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int RIntCeilingInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_ceil((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the smallest integer greater than or equal to the given <paramref name="op"/> <see cref="MpfrFloat"/> instance, with the specified <paramref name="rounding"/> mode and optional <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the smallest integer greater than or equal to.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <param name="precision">The precision in bits, optional. If not specified, use the precision of <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the smallest integer greater than or equal to the given <paramref name="op"/>.</returns>
    public static MpfrFloat RIntCeiling(MpfrFloat op, MpfrRounding? rounding = null, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntCeilingInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Rounds the value of <paramref name="op"/> to the nearest integer towards negative infinity, and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The value of <paramref name="op"/> is rounded to the nearest integer towards negative infinity, and the result is stored in <paramref name="rop"/>. 
    /// The rounding mode is determined by <paramref name="rounding"/>. 
    /// The sign of the rounded value is returned.
    /// </remarks>
    public static int RIntFloorInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_floor((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the largest integer less than or equal to a given <paramref name="op"/> <see cref="MpfrFloat"/> instance, with the specified <paramref name="rounding"/> mode and optional <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the floor of.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <param name="precision">The precision in bits to use, or <see langword="null"/> to use the precision of <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the largest integer less than or equal to <paramref name="op"/>.</returns>
    public static MpfrFloat RIntFloor(MpfrFloat op, MpfrRounding? rounding = null, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntFloorInplace(rop, op, rounding ?? DefaultRounding);
        return rop;
    }

    /// <summary>
    /// Round the value of <paramref name="op"/> to the nearest integer and store the result in <paramref name="rop"/> with the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int RIntRoundInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_round((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Rounds the <paramref name="op"/> to the nearest integer using the specified <paramref name="rounding"/> mode and returns a new <see cref="MpfrFloat"/> instance with the result.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the rounded value.</returns>
    public static MpfrFloat RIntRound(MpfrFloat op, MpfrRounding? rounding = null, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntRoundInplace(rop, op, rounding ?? DefaultRounding);
        return rop;
    }

    /// <summary>
    /// Rounds the value of <paramref name="op"/> to the nearest integer using "round to even" rule, and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// This function rounds the value of <paramref name="op"/> to the nearest integer using "round to even" rule, and stores the result in <paramref name="rop"/>.
    /// If the fractional part of <paramref name="op"/> is exactly 0.5, the result is the nearest even integer.
    /// </remarks>
    public static int RIntRoundEvenInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_roundeven((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Rounds the given <paramref name="op"/> to the nearest integer using the specified <paramref name="rounding"/> mode and returns a new <see cref="MpfrFloat"/> instance with the result.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to round.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <param name="precision">The precision of the result. If null, the precision of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the rounded value.</returns>
    public static MpfrFloat RIntRoundEven(MpfrFloat op, MpfrRounding? rounding = null, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntRoundEvenInplace(rop, op, rounding ?? DefaultRounding);
        return rop;
    }

    /// <summary>
    /// Truncates the value of <paramref name="op"/> to an integer and stores the result in <paramref name="rop"/> using the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to truncate.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int RIntTruncateInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_trunc((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Returns a new <see cref="MpfrFloat"/> instance representing the integer part of the input <paramref name="op"/> with the specified <paramref name="rounding"/> mode and optional <paramref name="precision"/>.
    /// </summary>
    /// <param name="op">The input <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <param name="precision">The precision of the result. If not specified, use the precision of the input <paramref name="op"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the integer part of the input <paramref name="op"/>.</returns>
    public static MpfrFloat RIntTruncate(MpfrFloat op, MpfrRounding? rounding = null, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntTruncateInplace(rop, op, rounding ?? DefaultRounding);
        return rop;
    }

    /// <summary>
    /// Compute the fractional part of <paramref name="op"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the fractional part.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int FractionalInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_frac((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the fractional part of a <paramref name="op"/> <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the fractional part of.</param>
    /// <param name="precision">The precision in bits of the result. If not specified, use <see cref="DefaultPrecision"/>.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, use <see cref="DefaultRounding"/>.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the fractional part of <paramref name="op"/>.</returns>
    public static MpfrFloat Fractional(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        FractionalInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the fractional part of <paramref name="iop"/> and stores it in <paramref name="fop"/>. The integral part of <paramref name="iop"/> is stored in <paramref name="op"/>.
    /// </summary>
    /// <param name="iop">The <see cref="MpfrFloat"/> to compute the fractional part from.</param>
    /// <param name="fop">The <see cref="MpfrFloat"/> to store the fractional part of <paramref name="iop"/>.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> to store the integral part of <paramref name="iop"/>.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ModFractionalInplace(MpfrFloat iop, MpfrFloat fop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* fi = &iop.Raw)
        fixed (Mpfr_t* ff = &fop.Raw)
        fixed (Mpfr_t* fo = &op.Raw)
        {
            return MpfrLib.mpfr_modf((IntPtr)fi, (IntPtr)ff, (IntPtr)fo, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the integer and fractional parts of a <see cref="MpfrFloat"/> instance <paramref name="op"/> and returns them along with the rounding mode used.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to compute the integer and fractional parts of.</param>
    /// <param name="precision">The precision in bits to use for the resulting <see cref="MpfrFloat"/> instances. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode of <paramref name="op"/> is used.</param>
    /// <returns>A tuple containing the integer part, fractional part, and the rounding mode used.</returns>
    public static (MpfrFloat iop, MpfrFloat fop, int round) ModFractional(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat iop = new(precision ?? op.Precision);
        MpfrFloat fop = new(precision ?? op.Precision);
        int round = ModFractionalInplace(iop, fop, op, rounding);
        return (iop, fop, round);
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="x"/> by <paramref name="y"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> instance to divide by.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// The remainder is defined as <paramref name="x"/> - n * <paramref name="y"/> where n is the quotient of <paramref name="x"/> divided by <paramref name="y"/> rounded to the nearest integer.
    /// </remarks>
    public static int ModInplace(MpfrFloat rop, MpfrFloat x, MpfrFloat y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            return MpfrLib.mpfr_fmod((IntPtr)pr, (IntPtr)px, (IntPtr)py, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="x"/> by <paramref name="y"/> using the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="x">The dividend.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the remainder of the division of <paramref name="x"/> by <paramref name="y"/>.</returns>
    public static MpfrFloat Mod(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ModInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the remainder of dividing one <see cref="MpfrFloat"/> by another <see cref="MpfrFloat"/>.
    /// </summary>
    /// <param name="x">The <see cref="MpfrFloat"/> to be divided.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> to divide by.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the remainder of dividing <paramref name="x"/> by <paramref name="y"/>.</returns>
    public static MpfrFloat operator %(MpfrFloat x, MpfrFloat y) => Mod(x, y, x.Precision);

    /// <summary>
    /// Computes the remainder of <paramref name="x"/> divided by <paramref name="y"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="rounding">The rounding mode to be used. If null, <see cref="DefaultRounding"/> will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ModInplace(MpfrFloat rop, MpfrFloat x, uint y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_fmod_ui((IntPtr)pr, (IntPtr)px, y, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the modulo of <paramref name="x"/> and <paramref name="y"/> and returns the result as a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="x">The <see cref="MpfrFloat"/> instance to compute the modulo of.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, the precision of <paramref name="x"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the modulo of <paramref name="x"/> and <paramref name="y"/>.</returns>
    public static MpfrFloat Mod(MpfrFloat x, uint y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ModInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the remainder of dividing a <see cref="MpfrFloat"/> by an unsigned integer.
    /// </summary>
    /// <param name="x">The <see cref="MpfrFloat"/> to be divided.</param>
    /// <param name="y">The unsigned integer divisor.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the remainder of the division.</returns>
    public static MpfrFloat operator %(MpfrFloat x, uint y) => Mod(x, y, x.Precision);

    /// <summary>
    /// Computes the quotient and remainder of <paramref name="x"/> divided by <paramref name="y"/> and stores the quotient in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the quotient.</param>
    /// <param name="x">The dividend <see cref="MpfrFloat"/> instance.</param>
    /// <param name="y">The divisor <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>A tuple of two integers, the quotient and the remainder.</returns>
    public static (int quotient, int round) ModQuotientInplace(MpfrFloat rop, MpfrFloat x, MpfrFloat y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            int q;
            int round = MpfrLib.mpfr_fmodquo((IntPtr)pr, (IntPtr)(&q), (IntPtr)px, (IntPtr)py, rounding ?? DefaultRounding);
            return (q, round);
        }
    }

    /// <summary>
    /// Computes the quotient and remainder of the division of <paramref name="x"/> by <paramref name="y"/> with the specified <paramref name="precision"/> and <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="x">The dividend.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A tuple containing the quotient, remainder, and the rounding mode used.</returns>
    public static (MpfrFloat rop, int quotient, int round) ModQuotient(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int quotient, int round) = ModQuotientInplace(rop, x, y, rounding);
        return (rop, quotient, round);
    }

    /// <summary>
    /// Computes the remainder of <paramref name="x"/> divided by <paramref name="y"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="x">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="y">The <see cref="MpfrFloat"/> instance to divide by.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int ReminderInplace(MpfrFloat rop, MpfrFloat x, MpfrFloat y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            return MpfrLib.mpfr_remainder((IntPtr)pr, (IntPtr)px, (IntPtr)py, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="x"/> by <paramref name="y"/> and stores the result in a new <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <param name="x">The dividend.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the precision of <paramref name="x"/>.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the remainder of the division of <paramref name="x"/> by <paramref name="y"/>.</returns>
    public static MpfrFloat Reminder(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ReminderInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the remainder and quotient of <paramref name="x"/> divided by <paramref name="y"/> with rounding mode <paramref name="rounding"/> and store the remainder in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the remainder.</param>
    /// <param name="x">The dividend <see cref="MpfrFloat"/> instance.</param>
    /// <param name="y">The divisor <see cref="MpfrFloat"/> instance.</param>
    /// <param name="rounding">The rounding mode to use, default to <see cref="DefaultRounding"/>.</param>
    /// <returns>A tuple of two integers, the quotient and the rounding flag.</returns>
    public static (int quotient, int round) ReminderQuotientInplace(MpfrFloat rop, MpfrFloat x, MpfrFloat y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            int q;
            int round = MpfrLib.mpfr_remquo((IntPtr)pr, (IntPtr)(&q), (IntPtr)px, (IntPtr)py, rounding ?? DefaultRounding);
            return (q, round);
        }
    }

    /// <summary>
    /// Computes the remainder and quotient of two <see cref="MpfrFloat"/> numbers <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// <param name="x">The dividend.</param>
    /// <param name="y">The divisor.</param>
    /// <param name="precision">The precision in bits of the result. If null, use the default precision.</param>
    /// <param name="rounding">The rounding mode to use. If null, use the default rounding mode.</param>
    /// <returns>A tuple containing the remainder as a new <see cref="MpfrFloat"/> instance, the quotient as an integer, and the rounding mode used.</returns>
    /// <remarks>
    /// The remainder is computed as <paramref name="x"/> - <paramref name="y"/> * quotient, where quotient is the integer part of <paramref name="x"/> / <paramref name="y"/>.
    /// </remarks>
    public static (MpfrFloat rop, int quotient, int round) ReminderQuotient(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int quotient, int round) = ReminderQuotientInplace(rop, x, y, rounding);
        return (rop, quotient, round);
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MpfrFloat"/> instance is an integer.
    /// </summary>
    /// <returns><c>true</c> if this instance is an integer; otherwise, <c>false</c>.</returns>
    public bool IsInteger
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_integer_p((IntPtr)pthis) != 0;
            }
        }
    }
    #endregion

    #region 11. Rounding-Related Functions

    /// <summary>
    /// Gets or sets the default rounding mode used by MPFR library.
    /// </summary>
    /// <value>The default rounding mode used by MPFR library.</value>
    public static MpfrRounding DefaultRounding
    {
        get => MpfrLib.mpfr_get_default_rounding_mode();
        set => MpfrLib.mpfr_set_default_rounding_mode(value);
    }

    /// <summary>
    /// Rounds the current <see cref="MpfrFloat"/> instance to the specified <paramref name="precision"/> with the specified <paramref name="rounding"/> mode.
    /// </summary>
    /// <param name="precision">The precision to round to.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public int RoundToPrecision(int precision, MpfrRounding? rounding = null)
    {
        CheckPrecision(precision);
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_prec_round((IntPtr)pthis, precision, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="MpfrFloat"/> instance can be rounded to the specified precision with the specified rounding modes and error bound.
    /// </summary>
    /// <param name="error">The error bound.</param>
    /// <param name="round1">The first rounding mode to use.</param>
    /// <param name="round2">The second rounding mode to use.</param>
    /// <param name="precision">The precision to round to.</param>
    /// <returns><see langword="true"/> if the current instance can be rounded to the specified precision with the specified rounding modes and error bound; otherwise, <see langword="false"/>.</returns>
    public bool CanRound(int error, MpfrRounding round1, MpfrRounding round2, int precision)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_can_round((IntPtr)pthis, error, round1, round2, precision) != 0;
        }
    }

    /// <summary>
    /// Gets the minimal precision of the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <returns>The minimal precision of the current <see cref="MpfrFloat"/> instance.</returns>
    public int MinimalPrecision
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_min_prec((IntPtr)pthis);
            }
        }
    }
    #endregion

    #region 12. Miscellaneous Functions

    /// <summary>
    /// Set the value of this <see cref="MpfrFloat"/> to the next representable value toward <paramref name="y"/>.
    /// </summary>
    /// <param name="y">The target value to approach.</param>
    /// <remarks>
    /// If this <see cref="MpfrFloat"/> is already equal to <paramref name="y"/>, it will remain unchanged.
    /// </remarks>
    public void NextToward(MpfrFloat y)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            MpfrLib.mpfr_nexttoward((IntPtr)pthis, (IntPtr)py);
        }
    }

    /// <summary>
    /// Set the value of this <see cref="MpfrFloat"/> to the next representable value above the current value.
    /// </summary>
    public void NextAbove()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_nextabove((IntPtr)pthis);
        }
    }

    /// <summary>
    /// Set the value of the current <see cref="MpfrFloat"/> instance to the next representable number below the current value.
    /// </summary>
    public void NextBelow()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_nextbelow((IntPtr)pthis);
        }
    }

    /// <summary>
    /// Computes the minimum value between two <see cref="MpfrFloat"/> instances and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> is used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>The result is stored in <paramref name="rop"/> and returned as the sign of the result.</remarks>
    public static int MinInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_min((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Returns the minimum value between two <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="precision">The precision of the result. If not specified, the maximum precision between <paramref name="op1"/> and <paramref name="op2"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the minimum value between <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Min(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? Math.Max(op1.Precision, op2.Precision));
        MinInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Computes the maximum value between two <see cref="MpfrFloat"/> instances and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/> if not specified.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>The result is stored in <paramref name="rop"/> and returned as the sign of the result.</remarks>
    public static int MaxInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_max((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Returns the maximum value between two <see cref="MpfrFloat"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="precision">The precision of the result. If not specified, the maximum precision between <paramref name="op1"/> and <paramref name="op2"/> will be used.</param>
    /// <param name="rounding">The rounding mode to use. If not specified, the default rounding mode will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the maximum value between <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static MpfrFloat Max(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? Math.Max(op1.Precision, op2.Precision));
        MaxInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Gets or sets the exponent of the current <see cref="MpfrFloat"/> instance.
    /// </summary>
    /// <value>The exponent of the current <see cref="MpfrFloat"/> instance.</value>
    public int Exponent
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_get_exp((IntPtr)pthis);
            }
        }
        set
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                _ = MpfrLib.mpfr_set_exp((IntPtr)pthis, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sign bit of the current <see cref="MpfrFloat"/> instance is set.
    /// </summary>
    /// <value><c>true</c> if the sign bit is set; otherwise, <c>false</c>.</value>
    public bool SignBit
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_signbit((IntPtr)pthis) != 0;
            }
        }
        set
        {
            CopySetSignInplace(this, this, value);
        }
    }

    /// <summary>
    /// Sets the sign of <paramref name="rop"/> to the sign of <paramref name="op"/> and returns the result.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to set the sign.</param>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to get the sign from.</param>
    /// <param name="sign">The sign to set to <paramref name="rop"/>.</param>
    /// <param name="rounding">The rounding mode to use, defaults to <see cref="DefaultRounding"/>.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CopySetSignInplace(MpfrFloat rop, MpfrFloat op, bool sign, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_setsign((IntPtr)pr, (IntPtr)pop, sign ? 1 : 0, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="MpfrFloat"/> with the same value and sign as <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to copy the value from.</param>
    /// <param name="sign">The sign of the new instance.</param>
    /// <param name="precision">The precision of the new instance. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the rounding mode of <paramref name="op"/> is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> with the same value and sign as <paramref name="op"/>.</returns>
    public static MpfrFloat CopySetSign(MpfrFloat op, bool sign, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CopySetSignInplace(rop, op, sign, rounding);
        return rop;
    }

    /// <summary>
    /// Copies the absolute value of <paramref name="op"/> to <paramref name="rop"/> and sets its sign to the sign of <paramref name="signOp"/>.
    /// </summary>
    /// <param name="rop">The destination <see cref="MpfrFloat"/> instance.</param>
    /// <param name="op">The source <see cref="MpfrFloat"/> instance.</param>
    /// <param name="signOp">The <see cref="MpfrFloat"/> instance whose sign will be used to set the sign of <paramref name="rop"/>.</param>
    /// <param name="rounding">The rounding mode to use (defaults to <see cref="DefaultRounding"/>).</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    public static int CopySetSignInplace(MpfrFloat rop, MpfrFloat op, MpfrFloat signOp, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        fixed (Mpfr_t* psop = &signOp.Raw)
        {
            return MpfrLib.mpfr_copysign((IntPtr)pr, (IntPtr)pop, (IntPtr)psop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="MpfrFloat"/> with the same value and sign as <paramref name="op"/> and <paramref name="signOp"/> respectively.
    /// </summary>
    /// <param name="op">The <see cref="MpfrFloat"/> instance to copy the value from.</param>
    /// <param name="signOp">The <see cref="MpfrFloat"/> instance to copy the sign from.</param>
    /// <param name="precision">The precision in bits of the new instance. If null, the precision of <paramref name="op"/> is used.</param>
    /// <param name="rounding">The rounding mode to use. If null, the default rounding mode is used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> with the same value and sign as <paramref name="op"/> and <paramref name="signOp"/> respectively.</returns>
    public static MpfrFloat CopySetSign(MpfrFloat op, MpfrFloat signOp, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CopySetSignInplace(rop, op, signOp, rounding);
        return rop;
    }
    #endregion

    #region 13 Exception Related Functions

    /// <summary>
    /// Gets or sets the minimum exponent value for subnormal numbers in the current environment.
    /// </summary>
    /// <value>The minimum exponent value for subnormal numbers.</value>
    public static int EMin
    {
        get => MpfrLib.mpfr_get_emin();
        set => _ = MpfrLib.mpfr_set_emin(value);
    }

    /// <summary>
    /// Gets or sets the maximum value of the exponent for the current environment.
    /// </summary>
    /// <value>The maximum value of the exponent for the current environment.</value>
    public static int EMax
    {
        get => MpfrLib.mpfr_get_emax();
        set => _ = MpfrLib.mpfr_set_emax(value);
    }

    /// <summary>
    /// Gets the minimum exponent value for subnormal numbers.
    /// </summary>
    /// <returns>The minimum exponent value for subnormal numbers.</returns>
    public static int MinEMin => MpfrLib.mpfr_get_emin_min();

    /// <summary>
    /// Gets the maximum exponent for the subnormal numbers in the current floating-point format.
    /// </summary>
    /// <returns>The maximum exponent for the subnormal numbers.</returns>
    public static int MaxEMin => MpfrLib.mpfr_get_emin_max();

    /// <summary>
    /// Gets the minimum and maximum values of the exponent for the current system.
    /// </summary>
    /// <returns>The minimum and maximum values of the exponent as a tuple.</returns>
    public static int MinEMax => MpfrLib.mpfr_get_emax_min();

    /// <summary>
    /// Gets the maximum value of the unbiased exponent for the current platform.
    /// </summary>
    /// <returns>The maximum value of the unbiased exponent.</returns>
    public static int MaxEMax => MpfrLib.mpfr_get_emax_max();

    /// <summary>
    /// Subnormalize the <see cref="MpfrFloat"/> instance, which means shifting the significand to the right until the most significant bit is set.
    /// </summary>
    /// <param name="t">The number of bits to shift the significand to the right before subnormalizing. Default is 0.</param>
    /// <param name="rounding">The rounding mode to use. If null, <see cref="DefaultRounding"/> will be used.</param>
    /// <returns>
    /// Returns a ternary value indicating the success of the operation.
    /// <para>If the value is 0, the result stored in the destination variable is exact.</para>
    /// <para>If the value is positive, the result is greater than the exact result, </para>
    /// <para>if the value is negative, the result is lower than the exact result. </para>
    /// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
    /// <para>A NaN result always corresponds to an exact return value. </para>
    /// <para>The opposite of the returned ternary value is representable in an int.</para>
    /// </returns>
    /// <remarks>
    /// This method modifies the current instance of <see cref="MpfrFloat"/>.
    /// </remarks>
    public int SubNormalize(int t = 0, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_subnormalize((IntPtr)pthis, t, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Gets or sets the current error flags of the MPFR library.
    /// </summary>
    /// <value>The current error flags of the MPFR library.</value>
    public static MpfrErrorFlags ErrorFlags
    {
        get => (MpfrErrorFlags)MpfrLib.mpfr_flags_save();
        set => MpfrLib.mpfr_flags_restore((uint)value, 0b_0011_1111);
    }
    #endregion

    #region 14. Memory Handling Functions

    /// <summary>
    /// Frees the cache of MPFR memory blocks.
    /// </summary>
    /// <param name="way">The cache to free, default is <see cref="MpfrFreeCache.Local"/> and <see cref="MpfrFreeCache.Global"/>.</param>
    public static void FreeCache(MpfrFreeCache way = MpfrFreeCache.Local | MpfrFreeCache.Global)
    {
        MpfrLib.mpfr_free_cache2(way);
    }

    /// <summary>
    /// Frees the memory allocated for the memory pool used by MPFR functions.
    /// </summary>
    public static void FreePool() => MpfrLib.mpfr_free_pool();

    /// <summary>
    /// Cleans up the memory allocated by the GMP library for multi-precision floating-point numbers.
    /// </summary>
    /// <exception cref="Exception">Thrown when memory cleanup fails.</exception>
    public static void MemoryCleanup()
    {
        int ret = MpfrLib.mpfr_mp_memory_cleanup();
        if (ret != 0) throw new Exception("Memory cleanup failed.");
    }
    #endregion

    #region 15. Compatibility With MPF

    /// <summary>
    /// Sets the precision of the <see cref="MpfrFloat"/> instance to the specified <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">The new precision to set.</param>
    /// <remarks>
    /// This method sets the precision of the <see cref="MpfrFloat"/> instance to the specified <paramref name="precision"/>.
    /// </remarks>
    public void SetRawPrecision(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_set_prec_raw((IntPtr)ptr, precision);
        }
    }

    /// <summary>
    /// Compare two <see cref="MpfrFloat"/> instances for equality with a given precision.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> instance to compare.</param>
    /// <param name="op3">The precision to use for the comparison.</param>
    /// <returns>1 if the two instances are equal, 0 otherwise.</returns>
    public int MpfEquals(MpfrFloat op1, MpfrFloat op2, uint op3)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_eq((IntPtr)p1, (IntPtr)p2, op3);
        }
    }

    /// <summary>
    /// Calculates the relative difference between two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> operand.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> operand.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <remarks>
    /// The relative difference is defined as (op1 - op2) / ((op1 + op2) / 2).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/>, <paramref name="op1"/>, or <paramref name="op2"/> is null.</exception>
    public static unsafe void RelDiffInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            MpfrLib.mpfr_reldiff((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding);
        }
    }

    /// <summary>
    /// Calculates the relative difference between two <see cref="MpfrFloat"/> values <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="MpfrFloat"/> value.</param>
    /// <param name="op2">The second <see cref="MpfrFloat"/> value.</param>
    /// <param name="rounding">The rounding mode to use in the calculation.</param>
    /// <param name="precision">The precision in bits to use in the calculation. If not specified, the default precision of <see cref="DefaultPrecision"/> will be used.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the relative difference between <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    /// <remarks>
    /// The relative difference is calculated as (op1 - op2) / ((op1 + op2) / 2).
    /// </remarks>
    public static MpfrFloat RelDiff(MpfrFloat op1, MpfrFloat op2, MpfrRounding rounding, int precision = 0)
    {
        MpfrFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Multiply the <paramref name="op1"/> by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <returns>The result of the multiplication operation.</returns>
    public static int Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// Divide a <see cref="MpfrFloat"/> instance by 2 raised to the power of <paramref name="op2"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="MpfrFloat"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to be divided.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <returns>The result of the operation.</returns>
    public static int Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// Multiply a <see cref="MpfrFloat"/> instance by 2 raised to the power of <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to multiply.</param>
    /// <param name="op2">The power of 2 to raise.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <param name="precision">The precision in bits of the result.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the multiplication.</returns>
    /// <remarks>
    /// The result is rounded according to the specified <paramref name="rounding"/> mode and has a precision of <paramref name="precision"/> bits.
    /// </remarks>
    public static MpfrFloat Multiply2Exp(MpfrFloat op1, uint op2, MpfrRounding rounding, int precision)
    {
        MpfrFloat rop = new(precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// Divide a <see cref="MpfrFloat"/> instance by 2 raised to the power of <paramref name="op2"/> with the specified <paramref name="rounding"/> and <paramref name="precision"/>.
    /// </summary>
    /// <param name="op1">The <see cref="MpfrFloat"/> instance to divide.</param>
    /// <param name="op2">The power of 2 to raise to.</param>
    /// <param name="rounding">The rounding mode to use.</param>
    /// <param name="precision">The precision in bits of the result.</param>
    /// <returns>A new instance of <see cref="MpfrFloat"/> representing the result of the division.</returns>
    public static MpfrFloat Divide2Exp(MpfrFloat op1, uint op2, MpfrRounding rounding, int precision)
    {
        MpfrFloat rop = new(precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }
    #endregion

    #region Clear & Dispose
    private bool _disposed;

    private void Clear()
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_clear((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Releases the resources used by the <see cref="MpfrFloat"/> object.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    /// <remarks>
    /// If <paramref name="disposing"/> is true, this method releases all resources held by any managed objects that this <see cref="MpfrFloat"/> references.
    /// If <paramref name="disposing"/> is false, this method releases only the unmanaged resources.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for <see cref="MpfrFloat"/> class.
    /// </summary>
    ~MpfrFloat()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
