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

    public static MpfrRounding DefaultRounding
    {
        get => MpfrLib.mpfr_get_default_rounding_mode();
        set => MpfrLib.mpfr_set_default_rounding_mode(value);
    }

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
            fixed (Mpfr_t* ptr = &Raw)
            {
                MpfrLib.mpfr_set_prec((IntPtr)ptr, value);
            }
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
                    string location = Marshal.PtrToStringUTF8((IntPtr)endptr);
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

    public override bool Equals(object obj)
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

    // TODO: https://www.mpfr.org/mpfr-current/mpfr.html#index-mpfr_005fcompound_005fsi
    #endregion

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