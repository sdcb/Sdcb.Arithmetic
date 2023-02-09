using Sdcb.Arithmetic.Gmp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdcb.Arithmetic.Mpfr;

public unsafe class MpfrFloat : IDisposable
{
    public readonly Mpfr_t Raw;

    #region 1. Initialization Functions
    /// <summary>
    /// Initialize, set its precision to be exactly prec bits and its value to NaN.
    /// (Warning: the corresponding MPF function initializes to zero instead.)
    /// </summary>
    public MpfrFloat(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init2((IntPtr)ptr, precision);
        }
    }

    public MpfrFloat(Mpfr_t raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Initialize, set its precision to the default precision, and set its value to NaN.
    /// The default precision can be changed by a call to mpfr_set_default_prec.
    /// </summary>
    public MpfrFloat()
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_init((IntPtr)ptr);
        }
    }

    internal static MpfrFloat CreateWithNullablePrecision(int? precision) => precision switch
    {
        null => new MpfrFloat(),
        _ => new MpfrFloat(precision.Value)
    };

    /// <summary>
    /// The current default MPFR precision in bits.
    /// </summary>
    public static int DefaultPrecision
    {
        get => MpfrLib.mpfr_get_default_prec();
        set => MpfrLib.mpfr_set_default_prec(value);
    }

    public const int MaximalSupportedPrecision = int.MaxValue - 256;
    public const int MinimalSupportedPrecision = 1;

    /// <summary>
    /// The number of bits used to store its significand.
    /// </summary>
    public int Precision
    {
        get
        {
            fixed (Mpfr_t* ptr = &Raw)
            {
                return MpfrLib.mpfr_get_prec((IntPtr)ptr);
            }
        }
        set
        {
            RoundToPrecision(Precision, DefaultRounding);
        }
    }

    private static void CheckPrecision(int precision)
    {
        if (precision < 1 || precision > MaximalSupportedPrecision)
            throw new ArgumentOutOfRangeException(nameof(Precision), $"Precision should in range of [{MinimalSupportedPrecision}..{MaximalSupportedPrecision}].");
    }

    /// <remarks>Note: reset precision will clear the value.</remarks>
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
    public int Assign(MpfrFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public int Assign(uint val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_ui((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public int Assign(int val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_si((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public int Assign(float val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_flt((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public int Assign(double val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_d((IntPtr)pthis, val, rounding ?? DefaultRounding);
        }
    }

    public int Assign(GmpInteger val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_z((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public int Assign(GmpRational val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpq_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_q((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    public int Assign(GmpFloat val, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpf_t* pval = &val.Raw)
        {
            return MpfrLib.mpfr_set_f((IntPtr)pthis, (IntPtr)pval, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public int Assign2Exp(uint op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_ui_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public int Assign2Exp(int op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_set_si_2exp((IntPtr)pthis, op, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set the value of rop from op multiplied by two to the power e,
    /// rounded toward the given direction rnd.
    /// Note that the input 0 is converted to +0.
    /// </summary>
    public int Assign2Exp(GmpInteger op, int e, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_set_z_2exp((IntPtr)pthis, (IntPtr)pop, e, rounding ?? DefaultRounding);
        }
    }

    /// <summary>Set rop to the value of the string s in base base, rounded in the direction rnd. </summary>
    /// <exception cref="FormatException" />
    public void Assign(string s, int @base = 0, MpfrRounding? rounding = null)
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
            }
        }
    }

    /// <summary>Set to the value of the string s in base base, rounded in the direction rnd.</summary>
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
    /// set to NaN(not-a-number)
    /// </summary>
    public void AssignNaN()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_nan((IntPtr)pthis);
        }
    }

    /// <summary>
    /// set to +inf if sign >= 0, otherwise -inf
    /// </summary>
    public void AssignInfinity(int sign)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_inf((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// set to +0 if sign >= 0, otherwise -0
    /// </summary>
    /// <param name="sign"></param>
    public void AssignZero(int sign = 0)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_set_zero((IntPtr)pthis, sign);
        }
    }

    /// <summary>
    /// Swap the structures pointed to by x and y.
    /// In particular, the values are exchanged without rounding
    /// (this may be different from three mpfr_set calls using a third auxiliary variable).
    /// </summary>
    public static void Swap(MpfrFloat x, MpfrFloat y)
    {
        fixed (Mpfr_t* p1 = &x.Raw)
        fixed (Mpfr_t* p2 = &y.Raw)
        {
            MpfrLib.mpfr_swap((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Swap the structures pointed to by this and op.
    /// In particular, the values are exchanged without rounding
    /// (this may be different from three mpfr_set calls using a third auxiliary variable).
    /// </summary>
    public void Swap(MpfrFloat op) => Swap(this, op);
    #endregion

    #region 3. Combined Initialization and Assignment Functions
    public static MpfrFloat From(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(int op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(double op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(GmpInteger op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(GmpRational op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    public static MpfrFloat From(GmpFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        rop.Assign(op, rounding);
        return rop;
    }

    /// <exception cref="FormatException" />
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
    public double ToDouble(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_d((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator double(MpfrFloat op) => op.ToDouble();

    public float ToFloat(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_flt((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator float(MpfrFloat op) => op.ToFloat();

    public int ToInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_si((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator int(MpfrFloat op) => op.ToInt32();

    public uint ToUInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_get_ui((IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public static explicit operator uint(MpfrFloat op) => op.ToUInt32();

    /// <summary>
    /// <para>
    /// Return d and set exp (formally, the value pointed to by exp) such that 0.5 &lt;= abs(d) &lt; 1
    /// and d times 2 raised to exp equals op rounded to double (resp. long double) precision, 
    /// using the given rounding mode.
    /// </para>
    /// <para>
    /// If op is zero, then a zero of the same sign (or an unsigned zero, if the implementation
    /// does not have signed zeros) is returned, and exp is set to 0. If op is NaN or an infinity,
    /// then the corresponding double precision (resp. long-double precision) value is returned,
    /// and exp is undefined.
    /// </para>
    /// </summary>
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
    /// y * 2^exp = x
    /// </summary>
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
    /// y * 2^exp = x
    /// </summary>
    public (MpfrFloat y, int exp, bool overflowed) Frexp(int? precision = 0, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int exp, bool overflowed) = FrexpInplace(rop, rounding);
        return (rop, exp, overflowed);
    }

    /// <summary>
    /// z * 2^exp = x
    /// </summary>
    public int GetZ2ExpInplace(GmpInteger z)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pz = &z.Raw)
        {
            return MpfrLib.mpfr_get_z_2exp((IntPtr)pz, (IntPtr)pthis);
        }
    }

    /// <summary>
    /// y * 2^exp = x
    /// </summary>
    public (GmpInteger z, int exp) Z2Exp
    {
        get
        {
            GmpInteger z = new();
            int exp = GetZ2ExpInplace(z);
            return (z, exp);
        }
    }

    public int ToGmpIntegerInplace(GmpInteger z, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpz_t* pz = &z.Raw)
        {
            return MpfrLib.mpfr_get_z((IntPtr)pz, (IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public GmpInteger ToGmpInteger(MpfrRounding? rounding = null)
    {
        GmpInteger rop = new();
        ToGmpIntegerInplace(rop, rounding);
        return rop;
    }

    public static explicit operator GmpInteger(MpfrFloat r) => r.ToGmpInteger();

    public void ToGmpRationalInplace(GmpRational q, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpq_t* pq = &q.Raw)
        {
            MpfrLib.mpfr_get_q((IntPtr)pq, (IntPtr)pthis);
        }
    }

    public GmpRational ToGmpRational()
    {
        GmpRational rop = new();
        ToGmpRationalInplace(rop);
        return rop;
    }

    public static explicit operator GmpRational(MpfrFloat r) => r.ToGmpRational();

    public int ToGmpFloatInplace(GmpFloat f, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpf_t* pf = &f.Raw)
        {
            return MpfrLib.mpfr_get_f((IntPtr)pf, (IntPtr)pthis, rounding ?? DefaultRounding);
        }
    }

    public GmpFloat ToGmpFloat(uint? precision = null, MpfrRounding? rounding = null)
    {
        GmpFloat rop = GmpFloat.CreateWithNullablePrecision(precision);
        ToGmpFloatInplace(rop, rounding);
        return rop;
    }

    public static explicit operator GmpFloat(MpfrFloat r) => r.ToGmpFloat();

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

    public override string ToString() => ToString(10);

    public string ToString(int @base = 10, MpfrRounding? rounding = null)
    {
        byte[] resbuf = new byte[GetMaxStringLength(Raw.Precision, @base)];
        fixed (Mpfr_t* pthis = &Raw)
        fixed (byte* srcptr = &resbuf[0])
        {
            int exp;
            IntPtr ret = MpfrLib.mpfr_get_str((IntPtr)srcptr, (IntPtr)(&exp), @base, resbuf.Length, (IntPtr)pthis, rounding ?? DefaultRounding);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert {nameof(MpfrFloat)} to string.");
            }

            return GmpFloat.ToString(ret, Sign, exp);
        }
    }

    public bool FitsUInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_ulong_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    public bool FitsInt32(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_slong_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    public bool FitsUInt16(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_ushort_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    public bool FitsInt16(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_sshort_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    public bool FitsUInt64(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_uintmax_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }

    public bool FitsInt64(MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_fits_intmax_p((IntPtr)pthis, rounding ?? DefaultRounding) != 0;
        }
    }
    #endregion

    #region 5.  Arithmetic Functions
    #region Add
    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, MpfrFloat op2) => Add(op1, op2, op1.Precision);

    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, uint op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator +(uint op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, int op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator +(int op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_add_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, double op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator +(double op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, GmpInteger op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator +(GmpInteger op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int AddInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_add_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Add(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator +(MpfrFloat op1, GmpRational op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator +(GmpRational op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);
    #endregion

    #region Subtract
    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, MpfrFloat op2) => Subtract(op1, op2, op1.Precision);

    public static int SubtractInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(uint op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, uint op2) => Subtract(op1, op2, op1.Precision);

    public static int SubtractInplace(MpfrFloat rop, int op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_si_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(int op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(int op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, int op2) => Subtract(op1, op2, op1.Precision);

    public static int SubtractInplace(MpfrFloat rop, double op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_d_sub((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(double op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(double op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_sub_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, double op2) => Subtract(op1, op2, op1.Precision);

    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, GmpInteger op2) => Subtract(op1, op2, op1.Precision);

    public static int SubtractInplace(MpfrFloat rop, GmpInteger op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_z_sub((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(GmpInteger op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(GmpInteger op1, MpfrFloat op2) => Subtract(op1, op2, op2.Precision);

    public static int SubtractInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_sub_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Subtract(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SubtractInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator -(MpfrFloat op1, GmpRational op2) => Subtract(op1, op2, op1.Precision);
    #endregion

    #region Multiply
    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, MpfrFloat op2) => Add(op1, op2, op1.Precision);

    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, uint op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator *(uint op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, int op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator *(int op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, double op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator *(double op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, GmpInteger op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator *(GmpInteger op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);

    public static int MultiplyInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_mul_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        MultiplyInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator *(MpfrFloat op1, GmpRational op2) => Add(op1, op2, op1.Precision);
    public static MpfrFloat operator *(GmpRational op1, MpfrFloat op2) => Add(op2, op1, op2.Precision);
    #endregion

    public static void SquareInplace(MpfrFloat rop, MpfrFloat op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_sqr((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Square(MpfrFloat op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        SquareInplace(rop, op1, rounding);
        return rop;
    }

    #region Divide
    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, MpfrFloat op2) => Divide(op1, op2, op1.Precision);

    public static int DivideInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(uint op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, uint op2) => Divide(op1, op2, op1.Precision);

    public static int DivideInplace(MpfrFloat rop, int op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_si_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(int op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(int op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, int op2) => Divide(op1, op2, op1.Precision);

    public static int DivideInplace(MpfrFloat rop, double op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_d_div((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(double op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(double op1, MpfrFloat op2) => Divide(op1, op2, op2.Precision);

    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, double op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_d((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, double op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, double op2) => Divide(op1, op2, op1.Precision);

    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AddInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, GmpInteger op2) => Divide(op1, op2, op1.Precision);

    public static int DivideInplace(MpfrFloat rop, MpfrFloat op1, GmpRational op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_div_q((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide(MpfrFloat op1, GmpRational op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DivideInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator /(MpfrFloat op1, GmpRational op2) => Divide(op1, op2, op1.Precision);
    #endregion

    public static void SqrtInplace(MpfrFloat rop, MpfrFloat op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_sqrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Sqrt(MpfrFloat op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        SqrtInplace(rop, op1, rounding);
        return rop;
    }

    public static void SqrtInplace(MpfrFloat rop, uint op1, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            MpfrLib.mpfr_sqrt_ui((IntPtr)pr, op1, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Sqrt(uint op1, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SqrtInplace(rop, op1, rounding);
        return rop;
    }

    /// <summary>
    /// set rop to 1/sqrt(op)
    /// </summary>
    public static void ReciprocalSquareInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            MpfrLib.mpfr_rec_sqrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1/sqrt(op)</returns>
    public static MpfrFloat ReciprocalSquare(MpfrFloat op, int? precision, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ReciprocalSquareInplace(rop, op, rounding);
        return rop;
    }

    public static void CubicRootInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            MpfrLib.mpfr_cbrt((IntPtr)pr, (IntPtr)p1, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat CubicRoot(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CubicRootInplace(rop, op, rounding);
        return rop;
    }

    public static void RootNInplace(MpfrFloat rop, MpfrFloat op, uint n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            MpfrLib.mpfr_rootn_ui((IntPtr)pr, (IntPtr)p1, n, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat RootN(MpfrFloat op, uint n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RootNInplace(rop, op, n, rounding);
        return rop;
    }

    /// <summary>
    /// This function is the same as mpfr_rootn_ui except when op is −0 and n is even:
    /// the result is −0 instead of +0 (the reason was to be consistent with mpfr_sqrt).
    /// Said otherwise, if op is zero, set rop to op
    /// </summary>
    [Obsolete("use RootN")]
    public static void RootInplace(MpfrFloat rop, MpfrFloat op, uint n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op.Raw)
        {
            MpfrLib.mpfr_rootn_ui((IntPtr)pr, (IntPtr)p1, n, rounding ?? DefaultRounding);
        }
    }

    [Obsolete("use RootN")]
    public static MpfrFloat Root(MpfrFloat op, uint n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RootNInplace(rop, op, n, rounding);
        return rop;
    }

    public static int NegateInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_neg((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Negate(MpfrFloat op, int? precision, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        NegateInplace(rop, op, rounding);
        return rop;
    }

    public static int AbsInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_abs((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Abs(MpfrFloat op, int? precision, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AbsInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = op1 - op2(if op1 > op2), +0 (otherwise)</summary>
    public static int DimInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_dim((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <returns>op1 - op2(if op1 > op2), +0 (otherwise)</returns>
    public static MpfrFloat Dim(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        DimInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_2ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply2Exp(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_mul_2si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Multiply2Exp(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_2ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide2Exp(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_div_2si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Divide2Exp(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op1.Precision);
        Divide2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int FactorialInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_fac_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Factorial(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FactorialInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// set rop to (op1 * op2) + op3
    /// </summary>
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

    /// <returns>(op1 * op2) + op3</returns>
    public static MpfrFloat FMA(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMAInplace(rop, op1, op2, op3, rounding);
        return rop;
    }

    /// <summary>
    /// set rop to (op1 * op2) - op3
    /// </summary>
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

    /// <returns>(op1 * op2) - op3</returns>
    public static MpfrFloat FMS(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMSInplace(rop, op1, op2, op3, rounding);
        return rop;
    }

    /// <summary>
    /// set rop to (op1 * op2) + (op3 * op4)
    /// </summary>
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

    /// <returns>(op1 * op2) + (op3 * op4)</returns>
    public static MpfrFloat FMMA(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMMAInplace(rop, op1, op2, op3, op4, rounding);
        return rop;
    }

    /// <summary>
    /// set rop to (op1 * op2) - (op3 * op4)
    /// </summary>
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

    /// <returns>(op1 * op2) - (op3 * op4)</returns>
    public static MpfrFloat FMMS(MpfrFloat op1, MpfrFloat op2, MpfrFloat op3, MpfrFloat op4, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        FMMSInplace(rop, op1, op2, op3, op4, rounding);
        return rop;
    }

    /// <summary>set rop to sqrt(op1 * op2 + op2 * op2)</summary>
    public static int HypotInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_hypot((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <returns>sqrt(op1 * op2 + op2 * op2)</returns>
    public static MpfrFloat Hypot(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        HypotInplace(rop, op1, op2, rounding);
        return rop;
    }

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

    public static MpfrFloat Sum(IEnumerable<MpfrFloat> tab, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        SumInplace(rop, tab, rounding);
        return rop;
    }
    #endregion

    #region 6. Comparison Functions
    #region Compares
    public static int Compare(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static bool CompareGreater(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_greater_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    public static bool CompareGreaterOrEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_greaterequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    public static bool CompareLess(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_less_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    public static bool CompareLessOrEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_lessequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    public static bool CompareEquals(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_equal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    public static bool operator ==(MpfrFloat op1, MpfrFloat op2) => CompareEquals(op1, op2);
    public static bool operator !=(MpfrFloat op1, MpfrFloat op2) => !CompareEquals(op1, op2);
    public static bool operator >(MpfrFloat op1, MpfrFloat op2) => CompareGreater(op1, op2);
    public static bool operator <(MpfrFloat op1, MpfrFloat op2) => CompareLess(op1, op2);
    public static bool operator >=(MpfrFloat op1, MpfrFloat op2) => CompareGreaterOrEquals(op1, op2);
    public static bool operator <=(MpfrFloat op1, MpfrFloat op2) => CompareLessOrEquals(op1, op2);

    public static int Compare(MpfrFloat op1, uint op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_ui((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(MpfrFloat op1, uint op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, uint op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, uint op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, uint op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, uint op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, uint op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(uint op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(uint op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(uint op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(uint op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(uint op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(uint op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    public static int Compare(MpfrFloat op1, int op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_si((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(MpfrFloat op1, int op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, int op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, int op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, int op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, int op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, int op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(int op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(int op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(int op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(int op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(int op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(int op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    public static int Compare(MpfrFloat op1, double op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_d((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(MpfrFloat op1, double op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, double op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, double op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, double op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, double op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, double op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(double op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(double op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(double op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(double op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(double op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(double op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    public static int Compare(MpfrFloat op1, GmpInteger op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_z((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static bool operator ==(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, GmpInteger op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(GmpInteger op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    public static int Compare(MpfrFloat op1, GmpRational op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpq_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_q((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static bool operator ==(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, GmpRational op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(GmpRational op1, MpfrFloat op2) => Compare(op2, op1) >= 0;

    public static int Compare(MpfrFloat op1, GmpFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpf_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmp_f((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static bool operator ==(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) == 0;
    public static bool operator !=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) != 0;
    public static bool operator >(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) > 0;
    public static bool operator <(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) < 0;
    public static bool operator >=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(MpfrFloat op1, GmpFloat op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) == 0;
    public static bool operator !=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) != 0;
    public static bool operator >(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) < 0;
    public static bool operator <(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) > 0;
    public static bool operator >=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(GmpFloat op1, MpfrFloat op2) => Compare(op2, op1) >= 0;
    #endregion

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

    public override int GetHashCode()
    {
        return Raw.GetHashCode();
    }

    /// <summary> compare op1 and op2 * 2 ^ e</summary>
    public static int Compare2Exp(MpfrFloat op1, uint op2, int e)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_ui_2exp((IntPtr)p1, op2, e);
        }
    }

    /// <summary> compare op1 and op2 * 2 ^ e</summary>
    public static int Compare2Exp(MpfrFloat op1, int op2, int e)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmp_si_2exp((IntPtr)p1, op2, e);
        }
    }

    public static int CompareAbs(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_cmpabs((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static int CompareAbs(MpfrFloat op1, uint op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_cmpabs_ui((IntPtr)p1, op2);
        }
    }

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

    /// <summary>neither NaN, infinity nor zero</summary>
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

    /// <returns>true if (op1 &lt; op2) or (op1 &gt; op2), false otherwise(NaN, or equals)</returns>
    public static bool IsLessOrGreater(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_lessequal_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <returns>true if op1 or op2 is NaN, false otherwise</returns>
    public static bool IsUnordered(MpfrFloat op1, MpfrFloat op2)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_unordered_p((IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// This function implements the totalOrder predicate from IEEE 754,
    /// where -NaN &lt; -Inf &lt; negative finite numbers &lt; -0 &lt; +0 &lt; positive finite numbers &lt; +Inf &lt; +NaN.
    /// </summary>
    /// <returns>true when x is smaller than or equal to y for this order relation, false otherwise.</returns>
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
    public static int LogInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Log(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogInplace(rop, op, rounding);
        return rop;
    }

    public static int LogInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_log_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Log(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        LogInplace(rop, op, rounding);
        return rop;
    }

    public static int Log2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Log2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log2Inplace(rop, op, rounding);
        return rop;
    }

    public static int Log10Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log10((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Log10(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log10Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = log(op + 1)</summary>
    public static int LogP1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log1p((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>log(op + 1)</returns>
    public static MpfrFloat LogP1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogP1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = log2(op + 1)</summary>
    public static int Log2P1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log2p1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>log2(op + 1)</returns>
    public static MpfrFloat Log2P1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log2P1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = log10(op + 1)</summary>
    public static int Log10P1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_log10p1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>log10(op + 1)</returns>
    public static MpfrFloat Log10P1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Log10P1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = e ^ op</summary>
    public static int ExpInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>e ^ op</returns>
    public static MpfrFloat Exp(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ExpInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 2 ^ op</summary>
    public static int Exp2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>2 ^ op</returns>
    public static MpfrFloat Exp2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp2Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 10 ^ op</summary>
    public static int Exp10Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp10((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>10 ^ op</returns>
    public static MpfrFloat Exp10(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp10Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = e ^ op - 1</summary>
    public static int ExpM1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_expm1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>e ^ op - 1</returns>
    public static MpfrFloat ExpM1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ExpM1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = e ^ op - 1</summary>
    public static int Exp2M1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp2m1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>e ^ op - 1</returns>
    public static MpfrFloat Exp2M1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp2M1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = e ^ op - 1</summary>
    public static int Exp10M1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_exp10m1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>e ^ op - 1</returns>
    public static MpfrFloat Exp10M1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Exp10M1Inplace(rop, op, rounding);
        return rop;
    }

    #region Power
    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_pow((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator ^(MpfrFloat op1, MpfrFloat op2) => Power(op1, op2);

    /// <summary>
    /// <para>powr(x,y) is NaN for x=NaN or x &lt; 0 (a)</para>
    /// <para>powr(+/-0,+/-0) is NaN whereas pow(x,+/-0) = 1 if x is not NaN (b)</para>
    /// <para>powr(+Inf,+/-0) is NaN whereas pow(x,+/-0) = 1 if x is not NaN (b)</para>
    /// </summary>
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
    /// <para>powr(x,y) is NaN for x=NaN or x &lt; 0 (a)</para>
    /// <para>powr(+/-0,+/-0) is NaN whereas pow(x,+/-0) = 1 if x is not NaN (b)</para>
    /// <para>powr(+Inf,+/-0) is NaN whereas pow(x,+/-0) = 1 if x is not NaN (b)</para>
    /// </summary>
    public static MpfrFloat PowerR(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerRInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_pow_ui((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(MpfrFloat op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator ^(MpfrFloat op1, uint op2) => Power(op1, op2);

    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, int op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            return MpfrLib.mpfr_pow_si((IntPtr)pr, (IntPtr)p1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(MpfrFloat op1, int op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator ^(MpfrFloat op1, int op2) => Power(op1, op2);

    public static int PowerInplace(MpfrFloat rop, MpfrFloat op1, GmpInteger op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_pow_z((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(MpfrFloat op1, GmpInteger op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator ^(MpfrFloat op1, GmpInteger op2) => Power(op1, op2);

    public static int PowerInplace(MpfrFloat rop, uint op1, uint op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_ui_pow_ui((IntPtr)pr, op1, op2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(uint op1, uint op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int PowerInplace(MpfrFloat rop, uint op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_ui_pow((IntPtr)pr, op1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Power(uint op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        PowerInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static MpfrFloat operator ^(uint op1, MpfrFloat op2) => Power(op1, op2);
    #endregion

    /// <summary>rop = (1 + op) * 2 ^ n</summary>
    public static int CompoundInplace(MpfrFloat rop, MpfrFloat op, int n, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_compound_si((IntPtr)pr, (IntPtr)pop, n, rounding ?? DefaultRounding);
        }
    }

    /// <returns>(1 + op) * 2 ^ n</returns>
    public static MpfrFloat Compound(MpfrFloat op, int n, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CompoundInplace(rop, op, n, rounding);
        return rop;
    }

    #region Trigonometric function
    public static int CosInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cos((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Cos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosInplace(rop, op, rounding);
        return rop;
    }

    public static int SinInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sin((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Sin(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinInplace(rop, op, rounding);
        return rop;
    }

    public static int TanInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tan((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Tan(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = cos(op * 2𝝿 / u)</summary>
    public static int CosUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cosu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>cos(op * 2𝝿 / u)</returns>
    public static MpfrFloat CosU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = sin(op * 2𝝿 / u)</summary>
    public static int SinUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>sin(op * 2𝝿 / u)</returns>
    public static MpfrFloat SinU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = tan(op * 2𝝿 / u)</summary>
    public static int TanUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>tan(op * 2𝝿 / u)</returns>
    public static MpfrFloat TanU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = cos(op * 𝝿)</summary>
    public static int CosPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cospi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>cos(op * 𝝿)</returns>
    public static MpfrFloat CosPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CosPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = sin(op * 𝝿)</summary>
    public static int SinPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>sin(op * 𝝿)</returns>
    public static MpfrFloat SinPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = tan(op * 𝝿)</summary>
    public static int TanPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>tan(op * 𝝿)</returns>
    public static MpfrFloat TanPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanPiInplace(rop, op, rounding);
        return rop;
    }

    public static int SinCosInplace(MpfrFloat sop, MpfrFloat cop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* psin = &sop.Raw)
        fixed (Mpfr_t* pcos = &cop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sin_cos((IntPtr)psin, (IntPtr)pcos, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static (MpfrFloat sin, MpfrFloat cos) SinCos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat sop = new(precision ?? op.Precision);
        MpfrFloat cop = new(precision ?? op.Precision);
        SinCosInplace(sop, cop, op, rounding);
        return (sop, cop);
    }

    /// <summary>rop = 1 / cos(op)</summary>
    public static int SecInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sec((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1 / cos(op)</returns>
    public static MpfrFloat Sec(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SecInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 1/ sin(op)</summary>
    public static int CscInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_csc((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>sin(op)</returns>
    public static MpfrFloat Csc(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CscInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 1 / tan(op)</summary>
    public static int CotInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cot((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1 / tan(op)</returns>
    public static MpfrFloat Cot(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CotInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = acos(op)</summary>
    public static int AcosInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acos((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>acos(op)</returns>
    public static MpfrFloat Acos(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = asin(op)</summary>
    public static int AsinInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asin((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>asin(op)</returns>
    public static MpfrFloat Asin(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = atan(op)</summary>
    public static int AtanInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atan((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan(op)</returns>
    public static MpfrFloat Atan(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = acos(op) / 2𝝿 * u</summary>
    public static int AcosUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acosu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>acos(op) / 2𝝿 * u</returns>
    public static MpfrFloat AcosU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = asin(op) / 2𝝿 * u</summary>
    public static int AsinUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>asin(op) / 2𝝿 * u</returns>
    public static MpfrFloat AsinU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = atan(op) / 2𝝿 * u</summary>
    public static int AtanUInplace(MpfrFloat rop, MpfrFloat op, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanu((IntPtr)pr, (IntPtr)pop, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan(op) / 2𝝿 * u</returns>
    public static MpfrFloat AtanU(MpfrFloat op, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanUInplace(rop, op, u, rounding);
        return rop;
    }

    /// <summary>rop = acos(op) / 𝝿</summary>
    public static int AcosPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acospi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>acos(op) / 𝝿</returns>
    public static MpfrFloat AcosPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcosPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = asin(op) / 𝝿</summary>
    public static int AsinPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>asin(op) / 𝝿</returns>
    public static MpfrFloat AsinPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = atan(op) / 𝝿</summary>
    public static int AtanPiInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanpi((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan(op) / 𝝿</returns>
    public static MpfrFloat AtanPi(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanPiInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = atan2(y, x)</summary>
    public static int Atan2Inplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2((IntPtr)pr, (IntPtr)py, (IntPtr)px, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan2(op)</returns>
    public static MpfrFloat Atan2(MpfrFloat y, MpfrFloat x, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2Inplace(rop, y, x, rounding);
        return rop;
    }

    /// <summary>rop = atan2(y, x) / 2𝝿 * u</summary>
    public static int Atan2UInplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, uint u = 360, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2u((IntPtr)pr, (IntPtr)py, (IntPtr)px, u, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan2(y, x) / 2𝝿 * u</returns>
    public static MpfrFloat Atan2U(MpfrFloat y, MpfrFloat x, uint u = 360, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2UInplace(rop, y, x, u, rounding);
        return rop;
    }

    /// <summary>rop = atan2(y, x) / 𝝿</summary>
    public static int Atan2PiInplace(MpfrFloat rop, MpfrFloat y, MpfrFloat x, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* py = &y.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_atan2pi((IntPtr)pr, (IntPtr)py, (IntPtr)px, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atan2(y, x) / 𝝿</returns>
    public static MpfrFloat Atan2Pi(MpfrFloat y, MpfrFloat x, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        Atan2PiInplace(rop, y, x, rounding);
        return rop;
    }

    public static int CoshInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_cosh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Cosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CoshInplace(rop, op, rounding);
        return rop;
    }

    public static int SinhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Sinh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SinhInplace(rop, op, rounding);
        return rop;
    }

    public static int TanhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_tanh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Tanh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        TanhInplace(rop, op, rounding);
        return rop;
    }

    public static int SinhCoshInplace(MpfrFloat sop, MpfrFloat cop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* psin = &sop.Raw)
        fixed (Mpfr_t* pcos = &cop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sinh_cosh((IntPtr)psin, (IntPtr)pcos, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static (MpfrFloat sinh, MpfrFloat cosh) SinhCosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat sop = new(precision ?? op.Precision);
        MpfrFloat cop = new(precision ?? op.Precision);
        SinhCoshInplace(sop, cop, op, rounding);
        return (sop, cop);
    }

    /// <summary>rop = 1 / cosh(op)</summary>
    public static int SechInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_sech((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1 / cosh(op)</returns>
    public static MpfrFloat Sech(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        SechInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 1/ sinh(op)</summary>
    public static int CschInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_csch((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1 / sinh(op)</returns>
    public static MpfrFloat Csch(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CschInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = 1 / tanh(op)</summary>
    public static int CothInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_coth((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>1 / tanh(op)</returns>
    public static MpfrFloat Coth(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CothInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = acosh(op)</summary>
    public static int AcoshInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_acosh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>acosh(op)</returns>
    public static MpfrFloat Acosh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AcoshInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = asinh(op)</summary>
    public static int AsinhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_asinh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>asinh(op)</returns>
    public static MpfrFloat Asinh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AsinhInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>rop = atanh(op)</summary>
    public static int AtanhInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_atanh((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>atanh(op)</returns>
    public static MpfrFloat Atanh(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AtanhInplace(rop, op, rounding);
        return rop;
    }
    #endregion

    public static int EintInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_eint((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Eint(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        EintInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Compute the real part of the dilogarithm defined by:
    /// <para>Li2(x) = -\Int_{t=0}^x log(1-t)/t dt</para>
    /// </summary>
    public static int Li2Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_li2((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Compute the real part of the dilogarithm defined by:
    /// <para>Li2(x) = -\Int_{t=0}^x log(1-t)/t dt</para>
    /// </summary>
    public static MpfrFloat Li2(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Li2Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the Gamma function on op.</summary>
    public static int GammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_gamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the Gamma function on op.</returns>
    public static MpfrFloat Gamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        GammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>upper incomplete Gamma function</summary>
    public static int GammaIncInplace(MpfrFloat rop, MpfrFloat op, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        fixed (Mpfr_t* pop2 = &op2.Raw)
        {
            return MpfrLib.mpfr_gamma_inc((IntPtr)pr, (IntPtr)pop, (IntPtr)pop2, rounding ?? DefaultRounding);
        }
    }

    /// <summary>upper incomplete Gamma function</summary>
    public static MpfrFloat GammaInc(MpfrFloat op, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        GammaIncInplace(rop, op, op2, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the logarithm of the Gamma function on op, rounded in the direction rnd.</summary>
    public static int LogGammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_lngamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>the value of the logarithm of the Gamma function on op, rounded in the direction rnd.</returns>
    public static MpfrFloat LogGamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        LogGammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the logarithm of the absolute value of the Gamma function on op</summary>
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

    /// <returns>The value of the logarithm of the absolute value of the Gamma function on op</returns>
    public static (int sign, MpfrFloat value, int round) LGamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        (int sign, int round) = LGammaInplace(rop, op, rounding);
        return (sign, rop, round);
    }

    /// <summary>
    /// Set rop to the value of the Digamma (sometimes also called Psi) function on op, rounded in the direction rnd.
    /// When op is a negative integer, set rop to NaN.
    /// </summary>
    public static int DigammaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_digamma((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>
    /// The value of the Digamma (sometimes also called Psi) function on op, rounded in the direction rnd.
    /// When op is a negative integer, set rop to NaN.
    /// </returns>
    public static MpfrFloat Digamma(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        DigammaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the Beta function at arguments op1 and op2.</summary>
    /// <remarks>Note: the current code does not try to avoid internal overflow or underflow, and might use a huge internal precision in some cases.</remarks>
    public static int BetaInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_beta((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the Beta function at arguments op1 and op2.</returns>
    /// <remarks>Note: the current code does not try to avoid internal overflow or underflow, and might use a huge internal precision in some cases.</remarks>
    public static MpfrFloat Beta(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        BetaInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the Riemann Zeta function on op, rounded in the direction rnd.</summary>
    public static int ZetaInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_zeta((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the Riemann Zeta function on op, rounded in the direction rnd.</returns>
    public static MpfrFloat Zeta(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ZetaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the Riemann Zeta function on op, rounded in the direction rnd.</summary>
    public static int ZetaInplace(MpfrFloat rop, uint op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_zeta_ui((IntPtr)pr, op, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the Riemann Zeta function on op, rounded in the direction rnd.</returns>
    public static MpfrFloat Zeta(uint op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ZetaInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the error function on op, rounded in the direction rnd.</summary>
    public static int ErrorFunctionInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_erf((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the error function on op, rounded in the direction rnd.</returns>
    public static MpfrFloat ErrorFunction(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ErrorFunctionInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the complementary error function on op, rounded in the direction rnd.</summary>
    public static int ComplementaryErrorFunctionInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_erfc((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the complementary error function on op, rounded in the direction rnd.</returns>
    public static MpfrFloat ComplementaryErrorFunction(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        ComplementaryErrorFunctionInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int J0Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_j0((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat J0(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        J0Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int J1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_j1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat J1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        J1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int JNInplace(MpfrFloat rop, int n, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_jn((IntPtr)pr, n, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat JN(int n, MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        JNInplace(rop, n, op, rounding);
        return rop;
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int Y0Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_y0((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat Y0(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Y0Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int Y1Inplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_y1((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat Y1(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        Y1Inplace(rop, op, rounding);
        return rop;
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static int YNInplace(MpfrFloat rop, int n, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_yn((IntPtr)pr, n, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set rop to the value of the second kind Bessel function of order 0 (resp. 1 and n) on op, rounded in the direction rnd.
    /// <list type="bullet">
    /// <item>When op is NaN or negative, rop is always set to NaN.</item>
    /// <item>When op is +Inf, rop is set to +0.</item>
    /// <item>When op is zero, rop is set to +Inf or -Inf depending on the parity and sign of n.</item>
    /// </list>
    /// </summary>
    public static MpfrFloat YN(int n, MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        YNInplace(rop, n, op, rounding);
        return rop;
    }

    /// <summary>Set rop to the arithmetic-geometric mean of op1 and op2, rounded in the direction rnd.</summary>
    public static int AGMInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_agm((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The arithmetic-geometric mean of op1 and op2, rounded in the direction rnd.</returns>
    public static MpfrFloat AGM(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        AGMInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>Set rop to the value of the Airy function Ai on x, rounded in the direction rnd.</summary>
    /// <remarks>
    /// When x is NaN, rop is always set to NaN.
    /// When x is +Inf or -Inf, rop is +0.
    /// The current implementation is not intended to be used with large arguments.
    /// It works with abs(x) typically smaller than 500.
    /// For larger arguments, other methods should be used and will be implemented in a future version.
    /// </remarks>
    public static int AiryInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_ai((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    /// <returns>The value of the Airy function Ai on x, rounded in the direction rnd.</returns>
    /// <remarks>
    /// When x is NaN, rop is always set to NaN.
    /// When x is +Inf or -Inf, rop is +0.
    /// The current implementation is not intended to be used with large arguments.
    /// It works with abs(x) typically smaller than 500.
    /// For larger arguments, other methods should be used and will be implemented in a future version.
    /// </remarks>
    public static MpfrFloat Airy(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        AiryInplace(rop, op, rounding);
        return rop;
    }

    public static int ConstLog2Inplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_log2((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat ConstLog2(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstLog2Inplace(rop, rounding);
        return rop;
    }

    public static int ConstPiInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_pi((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat ConstPi(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstPiInplace(rop, rounding);
        return rop;
    }

    public static int ConstEulerInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_euler((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat ConstEuler(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstEulerInplace(rop, rounding);
        return rop;
    }

    public static int ConstCatalanInplace(MpfrFloat rop, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        {
            return MpfrLib.mpfr_const_catalan((IntPtr)pr, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat ConstCatalan(int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ConstCatalanInplace(rop, rounding);
        return rop;
    }
    #endregion

    #region 10. Integer and Remainder Related Functions
    /// <summary>rounds to the nearest representable integer in the given direction rnd.</summary>
    public static int RIntInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>rounds to the nearest representable integer in the given direction rnd.</returns>
    public static MpfrFloat RInt(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>to the next higher or equal representable integer (like mpfr_rint with MPFR_RNDU)</summary>
    public static int CeilingInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_ceil((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>to the next higher or equal representable integer (like mpfr_rint with MPFR_RNDU)</summary>
    public static MpfrFloat Ceiling(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        CeilingInplace(rop, op);
        return rop;
    }

    /// <summary>to the next lower or equal representable integer (like mpfr_rint with MPFR_RNDD)</summary>
    public static int FloorInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_floor((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>to the next lower or equal representable integer (like mpfr_rint with MPFR_RNDD)</summary>
    public static MpfrFloat Floor(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        FloorInplace(rop, op);
        return rop;
    }

    /// <summary>to the nearest representable integer, rounding halfway cases away from zero (as in the roundTiesToAway mode of IEEE 754)</summary>
    public static int RoundInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_round((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>to the nearest representable integer, rounding halfway cases away from zero (as in the roundTiesToAway mode of IEEE 754)</summary>
    public static MpfrFloat Round(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundInplace(rop, op);
        return rop;
    }

    /// <summary>to the nearest representable integer, rounding halfway cases with the even-rounding rule (like mpfr_rint with MPFR_RNDN)</summary>
    public static int RoundEvenInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_roundeven((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>to the nearest representable integer, rounding halfway cases with the even-rounding rule (like mpfr_rint with MPFR_RNDN)</summary>
    public static MpfrFloat RoundEven(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundEvenInplace(rop, op);
        return rop;
    }

    /// <summary>to the next representable integer toward zero (like mpfr_rint with MPFR_RNDZ).</summary>
    public static int TruncateInplace(MpfrFloat rop, MpfrFloat op)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_trunc((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>to the next representable integer toward zero (like mpfr_rint with MPFR_RNDZ).</summary>
    public static MpfrFloat Truncate(MpfrFloat op, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RoundEvenInplace(rop, op);
        return rop;
    }

    /// <summary>to the next higher or equal integer</summary>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static int RIntCeilingInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_ceil((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>to the next higher or equal integer</returns>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static MpfrFloat RIntCeiling(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntCeilingInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>to the next lower or equal integer</summary>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static int RIntFloorInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_floor((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>to the next lower or equal integer</returns>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static MpfrFloat RIntFloor(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntFloorInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>to the nearest integer, rounding halfway cases away from zero.</summary>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static int RIntRoundInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_round((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>to the nearest integer, rounding halfway cases away from zero.</returns>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static MpfrFloat RIntRound(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntRoundInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>to the nearest integer, rounding halfway cases to the nearest even integer</summary>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static int RIntRoundEvenInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_roundeven((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>to the nearest integer, rounding halfway cases to the nearest even integer</returns>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static MpfrFloat RIntRoundEven(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntRoundEvenInplace(rop, op, rounding);
        return rop;
    }

    /// <summary>to the next integer toward zero</summary>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static int RIntTruncateInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_rint_trunc((IntPtr)pr, (IntPtr)pop, rounding);
        }
    }

    /// <returns>to the next integer toward zero</returns>
    /// <remarks>Contrary to mpfr_rint, those functions do perform a double rounding</remarks>
    public static MpfrFloat RIntTruncate(MpfrFloat op, MpfrRounding rounding, int? precision = null)
    {
        MpfrFloat rop = new(precision ?? op.Precision);
        RIntTruncateInplace(rop, op, rounding);
        return rop;
    }

    public static int FractionalInplace(MpfrFloat rop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* pop = &op.Raw)
        {
            return MpfrLib.mpfr_frac((IntPtr)pr, (IntPtr)pop, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Fractional(MpfrFloat op, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? DefaultPrecision);
        FractionalInplace(rop, op, rounding);
        return rop;
    }

    public static int ModFractionalInplace(MpfrFloat iop, MpfrFloat fop, MpfrFloat op, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* fi = &iop.Raw)
        fixed (Mpfr_t* ff = &fop.Raw)
        fixed (Mpfr_t* fo = &op.Raw)
        {
            return MpfrLib.mpfr_modf((IntPtr)fi, (IntPtr)ff, (IntPtr)fo, rounding ?? DefaultRounding);
        }
    }

    public static (MpfrFloat iop, MpfrFloat fop, int round) ModFractional(MpfrFloat op, int? precision = null, MpfrRounding ? rounding = null)
    {
        MpfrFloat iop = new(precision ?? op.Precision);
        MpfrFloat fop = new(precision ?? op.Precision);
        int round = ModFractionalInplace(iop, fop, op, rounding);
        return (iop, fop, round);
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
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
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static MpfrFloat Mod(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ModInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static MpfrFloat operator %(MpfrFloat x, MpfrFloat y) => Mod(x, y, x.Precision);

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static int ModInplace(MpfrFloat rop, MpfrFloat x, uint y, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* px = &x.Raw)
        {
            return MpfrLib.mpfr_fmod_ui((IntPtr)pr, (IntPtr)px, y, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static MpfrFloat Mod(MpfrFloat x, uint y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ModInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static MpfrFloat operator %(MpfrFloat x, uint y) => Mod(x, y, x.Precision);

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
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
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y, rounded toward zero
    /// </summary>
    public static (MpfrFloat rop, int quotient, int round) ModQuotient(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int quotient, int round) = ModQuotientInplace(rop, x, y, rounding);
        return (rop, quotient, round);
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y,
    /// rounded to the nearest integer (ties rounded to even)
    /// </summary>
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
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y,
    /// rounded to the nearest integer (ties rounded to even)
    /// </summary>
    public static MpfrFloat Reminder(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        ReminderInplace(rop, x, y, rounding);
        return rop;
    }

    /// <summary>
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y,
    /// rounded to the nearest integer (ties rounded to even)
    /// </summary>
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
    /// Set r to the value of x - ny, rounded according to the direction rnd, where n is the integer quotient of x divided by y,
    /// rounded to the nearest integer (ties rounded to even)
    /// </summary>
    public static (MpfrFloat rop, int quotient, int round) ReminderQuotient(MpfrFloat x, MpfrFloat y, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = CreateWithNullablePrecision(precision);
        (int quotient, int round) = ReminderQuotientInplace(rop, x, y, rounding);
        return (rop, quotient, round);
    }

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
    public static MpfrRounding DefaultRounding
    {
        get => MpfrLib.mpfr_get_default_rounding_mode();
        set => MpfrLib.mpfr_set_default_rounding_mode(value);
    }

    /// <summary>
    /// Round x according to rnd with precision prec, which must be an integer between MPFR_PREC_MIN and MPFR_PREC_MAX (otherwise the behavior is undefined).
    /// If prec is greater than or equal to the precision of x, then new space is allocated for the significand, and it is filled with zeros.
    /// Otherwise, the significand is rounded to precision prec with the given direction; no memory reallocation to free the unused limbs is done.
    /// In both cases, the precision of x is changed to prec.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException" />
    public int RoundToPrecision(int precision, MpfrRounding? rounding = null)
    {
        CheckPrecision(precision);
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_prec_round((IntPtr)pthis, precision, rounding ?? DefaultRounding);
        }
    }

    /// <summary>
    /// <see cref="MpfrLib.mpfr_can_round"/>
    /// </summary>
    public bool CanRound(int error, MpfrRounding round1, MpfrRounding round2, int precision)
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            return MpfrLib.mpfr_can_round((IntPtr)pthis, error, round1, round2, precision) != 0;
        }
    }

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
    public void NextToward(MpfrFloat y)
    {
        fixed (Mpfr_t* pthis = &Raw)
        fixed (Mpfr_t* py = &y.Raw)
        {
            MpfrLib.mpfr_nexttoward((IntPtr)pthis, (IntPtr)py);
        }
    }

    public void NextAbove()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_nextabove((IntPtr)pthis);
        }
    }

    public void NextBelow()
    {
        fixed (Mpfr_t* pthis = &Raw)
        {
            MpfrLib.mpfr_nextbelow((IntPtr)pthis);
        }
    }

    public static int MinInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_min((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Min(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? Math.Max(op1.Precision, op2.Precision));
        MinInplace(rop, op1, op2, rounding);
        return rop;
    }

    public static int MaxInplace(MpfrFloat rop, MpfrFloat op1, MpfrFloat op2, MpfrRounding? rounding = null)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_max((IntPtr)pr, (IntPtr)p1, (IntPtr)p2, rounding ?? DefaultRounding);
        }
    }

    public static MpfrFloat Max(MpfrFloat op1, MpfrFloat op2, int? precision = null, MpfrRounding? rounding = null)
    {
        MpfrFloat rop = new(precision ?? Math.Max(op1.Precision, op2.Precision));
        MaxInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>Get/set the exponent of value to e if x is a non-zero ordinary number and e is in the current exponent range.</summary>
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
                MpfrLib.mpfr_set_exp((IntPtr)pthis, value);
            }
        }
    }

    /// <summary>Return a true if op has its sign bit set (i.e., if it is negative, -0, or a NaN whose representation has its sign bit set).</summary>
    public bool SignBit
    {
        get
        {
            fixed (Mpfr_t* pthis = &Raw)
            {
                return MpfrLib.mpfr_signbit((IntPtr)pthis) != 0;
            }
        }
    }

    //public static int SetSignInplace(MpfrFloat rop, MpfrFloat op, bool sign, MpfrRounding? rounding = null)
    //{
    //    fixed (Mpfr_t* pr = &rop.Raw)
    //    fixed (Mpfr_t* pop = &op.Raw)
    //    {
    //        return MpfrLib.mpfr_setsign((IntPtr))
    //    }
    //}
    #endregion

    #region 15. Compatibility With MPF
    /// <summary>
    /// Reset the precision of x to be exactly prec bits.
    /// The only difference with mpfr_set_prec is that prec 
    /// is assumed to be small enough so that the 
    /// significand fits into the current allocated memory 
    /// space for x. 
    /// Otherwise the behavior is undefined.
    /// </summary>
    public void SetRawPrecision(int precision)
    {
        fixed (Mpfr_t* ptr = &Raw)
        {
            MpfrLib.mpfr_set_prec_raw((IntPtr)ptr, precision);
        }
    }

    /// <summary>
    /// Return non-zero if op1 and op2 are both non-zero ordinary 
    /// numbers with the same exponent and the same first op3 bits,
    /// both zero, or both infinities of the same sign. Return 
    /// zero otherwise. 
    /// This function is defined for compatibility with MPF, we do 
    /// not recommend to use it otherwise. 
    /// Do not use it either if you want to know whether two numbers 
    /// are close to each other; for instance, 1.011111 and 1.100000 
    /// are regarded as different for any value of op3 larger than 1.
    /// </summary>
    public int MpfEquals(MpfrFloat op1, MpfrFloat op2, uint op3)
    {
        fixed (Mpfr_t* p1 = &op1.Raw)
        fixed (Mpfr_t* p2 = &op2.Raw)
        {
            return MpfrLib.mpfr_eq((IntPtr)p1, (IntPtr)p2, op3);
        }
    }

    /// <summary>
    /// <para>rop = abs(op1-op2)/op1</para>
    /// <para>
    /// Compute the relative difference between op1 and op2 and store the result in rop.
    /// This function does not guarantee the correct rounding on the relative difference;
    /// it just computes |op1 - op2| / op1</para>
    /// </summary>
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
    /// <para>
    /// Compute the relative difference between op1 and op2 and store the result in rop.
    /// This function does not guarantee the correct rounding on the relative difference;
    /// it just computes |op1 - op2| / op1</para>
    /// </summary>
    /// <param name="precision">the result precision</param>
    /// <returns>abs(op1-op2)/op1</returns>
    public static MpfrFloat RelDiff(MpfrFloat op1, MpfrFloat op2, MpfrRounding rounding, int precision = 0)
    {
        MpfrFloat rop = new(precision);
        RelDiffInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static void Multiply2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_mul_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static void Divide2ExpInplace(MpfrFloat rop, MpfrFloat op1, uint op2, MpfrRounding rounding)
    {
        fixed (Mpfr_t* pr = &rop.Raw)
        fixed (Mpfr_t* p1 = &op1.Raw)
        {
            MpfrLib.mpfr_div_2exp((IntPtr)pr, (IntPtr)p1, op2, rounding);
        }
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
    public static MpfrFloat Multiply2Exp(MpfrFloat op1, uint op2, MpfrRounding rounding, int precision)
    {
        MpfrFloat rop = new(precision);
        Multiply2ExpInplace(rop, op1, op2, rounding);
        return rop;
    }

    /// <summary>
    /// These functions are identical to mpfr_mul_2ui and mpfr_div_2ui respectively.
    /// These functions are only kept for compatibility with MPF, one should prefer mpfr_mul_2ui and mpfr_div_2ui otherwise.
    /// </summary>
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

    ~MpfrFloat()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

[StructLayout(LayoutKind.Sequential)]
public record struct Mpfr_t
{
    public int Precision;
    public int Sign;
    public int Exponent;
    public IntPtr Limbs;

    public static unsafe int RawSize => sizeof(Mpfr_t);

    private int LimbCount => (Precision - 1) / (IntPtr.Size * 8) + 1;

    private unsafe Span<nint> GetLimbData() => new((void*)Limbs, LimbCount);

    public override int GetHashCode()
    {
        HashCode c = new();
        c.Add(Precision);
        c.Add(Sign);
        c.Add(Exponent);
        foreach (nint i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}