using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Globalization;

namespace Sdcb.Arithmetic.Gmp;

public class GmpInteger : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpLib.__gmpf_get_default_prec();
        set => GmpLib.__gmpf_set_default_prec(value);
    }

    public Mpz_t Raw = new();
    private bool _disposed = false;

    #region Initializing Integers
    public unsafe GmpInteger()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_init((IntPtr)ptr);
        }
    }

    public unsafe GmpInteger(Mpz_t raw)
    {
        Raw = raw;
    }

    public unsafe GmpInteger(uint bitCount)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_init2((IntPtr)ptr, bitCount);
        }
    }

    /// <summary>
    /// Change the space for integer to new_alloc limbs. The value in integer is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByLimbs(int limbs)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_realloc((IntPtr)ptr, limbs);
        }
    }

    /// <summary>
    /// Change the space allocated for x to n bits. The value in x is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByBits(uint bits)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_realloc2((IntPtr)ptr, bits);
        }
    }

    public unsafe void ReallocToFit() => ReallocByLimbs(System.Math.Abs(Raw.Size));
    #endregion

    #region Assignment Functions
    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(uint op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_ui((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(int op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_si((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(double op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_d((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set_q((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(GmpFloat op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set_f((IntPtr)ptr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe void Assign(string op, int opBase = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            byte[] opBytes = Encoding.UTF8.GetBytes(op);
            fixed (byte* opPtr = opBytes)
            {
                int ret = GmpLib.__gmpz_set_str((IntPtr)ptr, (IntPtr)opPtr, opBase);
                if (ret != 0)
                {
                    throw new FormatException($"Failed to parse \"{op}\", base={opBase} to BigInteger, __gmpz_set_str returns {ret}");
                }
            }
        }
    }

    public static unsafe void Swap(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_swap((IntPtr)pop1, (IntPtr)pop2);
        }
    }
    #endregion

    #region Combined Initialization and Assignment Functions
    public static unsafe GmpInteger From(GmpInteger op)
    {
        Mpz_t raw = new();
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_init_set((IntPtr)(&raw), (IntPtr)pop);
        }
        return new GmpInteger(raw);
    }

    public static unsafe GmpInteger From(uint op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_ui((IntPtr)(&raw), op);
        return new GmpInteger(raw);
    }

    public static unsafe GmpInteger From(int op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_si((IntPtr)(&raw), op);
        return new GmpInteger(raw);
    }

    public static unsafe GmpInteger From(double op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_d((IntPtr)(&raw), op);
        return new GmpInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static GmpInteger Parse(string val, int valBase = 0)
    {
        Mpz_t raw = new();
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpLib.__gmpz_init_set_str((IntPtr)(&raw), (IntPtr)pval, valBase);
            if (ret != 0)
            {
                GmpLib.__gmpz_clear((IntPtr)(&raw));
                throw new FormatException($"Failed to parse {val}, base={valBase} to BigInteger, __gmpf_init_set_str returns {ret}");
            }
        }
        return new GmpInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out GmpInteger result, int valBase = 10)
    {
        Mpz_t raw = new();
        Mpz_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpLib.__gmpz_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
            if (rt != 0)
            {
                GmpLib.__gmpz_clear((IntPtr)ptr);
                result = null;
                return false;
            }
            else
            {
                result = new GmpInteger(raw);
                return true;
            }
        }
    }
    #endregion

    #region Dispose and Clear

    private unsafe void Clear()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_clear((IntPtr)ptr);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            Clear();
            _disposed = true;
        }
    }

    ~GmpInteger()
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

    #region Arithmetic Functions
    public static unsafe void AddInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_add((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static GmpInteger Add(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator +(GmpInteger a, GmpInteger b) => Add(a, b);

    public static unsafe void AddInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_add_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static GmpInteger Add(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator +(GmpInteger a, uint b) => Add(a, b);
    public static GmpInteger operator +(uint a, GmpInteger b) => Add(b, a);

    public static unsafe void SubtractInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_sub((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static GmpInteger Subtract(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1, GmpInteger op2) => Subtract(op1, op2);

    public static unsafe void SubtractInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_sub_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static GmpInteger Subtract(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1, uint op2) => Subtract(op1, op2);

    public static unsafe void SubtractInplace(GmpInteger r, uint op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_ui_sub((IntPtr)pr, op1, (IntPtr)pop2);
        }
    }

    public static GmpInteger Subtract(uint op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(uint op1, GmpInteger op2) => Subtract(op1, op2);

    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_mul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static GmpInteger Multiply(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator *(GmpInteger op1, GmpInteger op2) => Multiply(op1, op2);

    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, int op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_si((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static GmpInteger Multiply(GmpInteger op1, int op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator *(GmpInteger op1, int op2) => Multiply(op1, op2);
    public static GmpInteger operator *(int op1, GmpInteger op2) => Multiply(op2, op1);

    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static GmpInteger Multiply(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator *(GmpInteger op1, uint op2) => Multiply(op1, op2);
    public static GmpInteger operator *(uint op1, GmpInteger op2) => Multiply(op2, op1);

    /// <summary>
    /// r += op1 * op2
    /// </summary>
    public static unsafe void AddMultiply(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_addmul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// r += op1 * op2
    /// </summary>
    public static unsafe void AddMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_addmul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static unsafe void SubtractMultiply(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_submul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static unsafe void SubtractMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_submul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void Multiply2ExpInplace(GmpInteger r, GmpInteger op1, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_2exp((IntPtr)pr, (IntPtr)pop1, exp2);
        }
    }

    public static unsafe GmpInteger Multiply2Exp(GmpInteger op1, uint exp2)
    {
        GmpInteger r = new();
        Multiply2ExpInplace(r, op1, exp2);
        return r;
    }

    public void LeftShift(uint bits) => Multiply2ExpInplace(this, this, bits);

    public static GmpInteger operator <<(GmpInteger op1, uint exp2) => Multiply2Exp(op1, exp2);

    public static unsafe void NegateInplace(GmpInteger r, GmpInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_neg((IntPtr)pr, (IntPtr)pop1);
        }
    }

    public static GmpInteger Negate(GmpInteger op1)
    {
        GmpInteger r = new();
        NegateInplace(r, op1);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1) => Negate(op1);

    public static unsafe void AbsInplace(GmpInteger r, GmpInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_abs((IntPtr)pr, (IntPtr)pop1);
        }
    }

    public static GmpInteger Abs(GmpInteger op1)
    {
        GmpInteger r = new();
        AbsInplace(r, op1);
        return r;
    }
    #endregion

    #region Conversion Functions
    public unsafe uint ToUInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_get_ui((IntPtr)ptr);
        }
    }

    public unsafe int ToInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_get_si((IntPtr)ptr);
        }
    }

    public unsafe double ToDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_get_d((IntPtr)ptr);
        }
    }

    public static explicit operator uint(GmpInteger op) => op.ToUInt32();
    public static explicit operator int(GmpInteger op) => op.ToInt32();
    public static explicit operator double(GmpInteger op) => op.ToDouble();
    public static explicit operator GmpFloat(GmpInteger op) => GmpFloat.From(op);

    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            int exp = default;
            double val = GmpLib.__gmpz_get_d_2exp((IntPtr)(&exp), (IntPtr)ptr);
            return new ExpDouble(exp, val);
        }
    }

    public override string ToString() => ToString(10);

    public unsafe string ToString(int opBase)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            IntPtr ret = GmpLib.__gmpz_get_str(IntPtr.Zero, opBase, (IntPtr)ptr);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert BigInteger to string.");
            }

            try
            {
                return Marshal.PtrToStringUTF8(ret)!;
            }
            finally
            {
                GmpMemory.Free(ret);
            }
        }
    }
    #endregion

    #region Division Functions
    #region Ceililng
    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    public static unsafe void CeilingDivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_cdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (ceiling)
    /// </summary>
    public static unsafe GmpInteger CeilingDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    public static unsafe void CeilingReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_cdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static unsafe GmpInteger CeilingReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    public static unsafe void CeilingDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_cdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_cdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static unsafe GmpInteger CeilingDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_cdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_cdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (ceiling)</returns>
    public static unsafe uint CeilingReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_cdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (ceiling)</returns>
    public static GmpInteger CeilingReminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        CeilingReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void CeilingDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_cdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (ceiling)
    /// </summary>
    public static unsafe GmpInteger CeilingDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        CeilingDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void CeilingReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_cdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (ceiling)
    /// </summary>
    public static unsafe GmpInteger CeilingReminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        CeilingReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Floor
    /// <summary>
    /// q = n / d (Floor)
    /// </summary>
    public static unsafe void FloorDivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_fdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (Floor)
    /// </summary>
    public static unsafe GmpInteger FloorDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    public static unsafe void FloorReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_fdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static unsafe GmpInteger FloorReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    public static unsafe void FloorDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_fdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_fdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static unsafe GmpInteger FloorDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_fdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_fdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Floor)</returns>
    public static unsafe uint FloorReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_fdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (Floor)</returns>
    public static unsafe GmpInteger FloorReminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        FloorReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void FloorDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_fdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (Floor)
    /// </summary>
    public static unsafe GmpInteger FloorDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        FloorDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void FloorReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_fdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (Floor)
    /// </summary>
    public static unsafe GmpInteger FloorReminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        FloorReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Truncate
    /// <summary>
    /// q = n / d (Truncate)
    /// </summary>
    public static unsafe void DivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_tdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (Truncate)
    /// </summary>
    public static unsafe GmpInteger Divide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    public static unsafe void ReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_tdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static unsafe GmpInteger Reminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        ReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    public static unsafe void DivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_tdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint DivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_tdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static unsafe GmpInteger Divide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint ReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_tdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint DivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_tdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static unsafe (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Truncate)</returns>
    public static unsafe uint ReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_tdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (Truncate)</returns>
    public static unsafe GmpInteger Reminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        ReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void Divide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_tdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (Truncate)
    /// </summary>
    public static unsafe GmpInteger Divide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        Divide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void Reminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_tdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (Truncate)
    /// </summary>
    public static unsafe GmpInteger Reminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        Reminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region operators
    public static GmpInteger operator /(GmpInteger op1, GmpInteger op2) => Divide(op1, op2);

    public static GmpInteger operator %(GmpInteger op1, GmpInteger op2) => Reminder(op1, op2);

    public static GmpInteger operator /(GmpInteger op1, uint op2) => Divide(op1, op2);

    public static GmpInteger operator %(GmpInteger op1, uint op2) => Reminder(op1, op2);
    #endregion

    #region Others
    /// <summary>
    /// Set r to n mod d. The sign of the divisor is ignored; the result is always non-negative.
    /// </summary>
    public static unsafe void ModInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_mod((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d. The sign of the divisor is ignored; the result is always non-negative.
    /// </summary>
    public static unsafe GmpInteger Mod(GmpInteger n, GmpInteger d)
    {
        GmpInteger r = new();
        ModInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// <para>Set r to n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static unsafe uint ModInplace(GmpInteger r, GmpInteger n, uint d) => FloorReminderInplace(r, n, d);

    /// <summary>
    /// <para>return n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static unsafe GmpInteger Mod(GmpInteger n, uint d) => FloorReminder(n, d);

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe void DivExactInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpLib.__gmpz_divexact((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe GmpInteger DivExact(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe void DivExactInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_divexact_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe GmpInteger DivExact(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// <para>n is congruent to c mod d if there exists an integer q satisfying n = c + q*d.</para>
    /// <para>Unlike the other division functions, d=0 is accepted and following the rule it can be seen that n and c are considered congruent mod 0 only when exactly equal.</para>
    /// </summary>
    /// <returns>true if n = c mod d</returns>
    public static unsafe bool Congruent(GmpInteger n, GmpInteger c, GmpInteger d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pc = &c.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            return GmpLib.__gmpz_congruent_p((IntPtr)pn, (IntPtr)pc, (IntPtr)pd) != 0;
        }
    }

    /// <summary>
    /// <para>n is congruent to c mod d if there exists an integer q satisfying n = c + q*d.</para>
    /// <para>Unlike the other division functions, d=0 is accepted and following the rule it can be seen that n and c are considered congruent mod 0 only when exactly equal.</para>
    /// </summary>
    /// <returns>true if n = c mod d</returns>
    public static unsafe bool Congruent(GmpInteger n, uint c, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_congruent_ui_p((IntPtr)pn, c, d) != 0;
        }
    }

    /// <summary>
    /// <para>n is congruent to c mod (2^b) if there exists an integer q satisfying n = c + q*(2^b).</para>
    /// </summary>
    /// <returns>true if n = c mod (2^b)</returns>
    public static unsafe bool Congruent2Exp(GmpInteger n, GmpInteger c, uint b)
    {
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pc = &c.Raw)
        {
            return GmpLib.__gmpz_congruent_2exp_p((IntPtr)pn, (IntPtr)pc, b) != 0;
        }
    }
    #endregion
    #endregion

    #region Exponentiation Functions
    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    public static unsafe void PowerModInplace(GmpInteger r, GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pexp = &exp.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpLib.__gmpz_powm((IntPtr)pr, (IntPtr)pbase, (IntPtr)pexp, (IntPtr)pmod);
        }
    }

    /// <returns>(@base ^ exp) % mod</returns>
    public static GmpInteger PowerMod(GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    public static unsafe void PowerModInplace(GmpInteger r, GmpInteger @base, uint exp, GmpInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpLib.__gmpz_powm_ui((IntPtr)pr, (IntPtr)pbase, exp, (IntPtr)pmod);
        }
    }

    /// <returns>(@base ^ exp) % mod</returns>
    public static GmpInteger PowerMod(GmpInteger @base, uint exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    /// <remarks>
    /// <para>
    /// This function is designed to take the same time and have the same cache access patterns for any two same-size arguments,
    /// assuming that function arguments are placed at the same position and that the machine state is identical upon function entry. 
    /// </para>
    /// <para>This function is intended for cryptographic purposes, where resilience to side-channel attacks is desired.</para>
    /// </remarks>
    public static unsafe void PowerModSecureInplace(GmpInteger r, GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pexp = &exp.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpLib.__gmpz_powm_sec((IntPtr)pr, (IntPtr)pbase, (IntPtr)pexp, (IntPtr)pmod);
        }
    }

    /// <returns>(@base ^ exp) % mod</returns>
    /// <remarks>
    /// <para>
    /// This function is designed to take the same time and have the same cache access patterns for any two same-size arguments,
    /// assuming that function arguments are placed at the same position and that the machine state is identical upon function entry. 
    /// </para>
    /// <para>This function is intended for cryptographic purposes, where resilience to side-channel attacks is desired.</para>
    /// </remarks>
    public static GmpInteger PowerModSecure(GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModSecureInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// r = base ^ exp
    /// </summary>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static unsafe void PowerInplace(GmpInteger r, GmpInteger @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        {
            GmpLib.__gmpz_pow_ui((IntPtr)pr, (IntPtr)pbase, exp);
        }
    }

    /// <returns>base ^ exp</returns>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static GmpInteger Power(GmpInteger @base, uint exp)
    {
        GmpInteger r = new();
        PowerInplace(r, @base, exp);
        return r;
    }

    /// <summary>
    /// r = base ^ exp
    /// </summary>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static unsafe void PowerInplace(GmpInteger r, uint @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        {
            GmpLib.__gmpz_ui_pow_ui((IntPtr)pr, @base, exp);
        }
    }

    /// <returns>base ^ exp</returns>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static GmpInteger Power(uint @base, uint exp)
    {
        GmpInteger r = new();
        PowerInplace(r, @base, exp);
        return r;
    }

    /// <summary>
    /// power
    /// </summary>
    public static GmpInteger operator ^(GmpInteger @base, uint exp) => Power(@base, exp);
    #endregion

    #region Root Extraction Functions
    /// <summary>
    /// r = sqrt(op, n)
    /// </summary>
    /// <returns>true if computation was exact</returns>
    public static unsafe bool RootInplace(GmpInteger r, GmpInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            return GmpLib.__gmpz_root((IntPtr)pr, (IntPtr)pop, n) != 0;
        }
    }

    /// <returns>sqsrt(op, n)</returns>
    public static GmpInteger Root(GmpInteger op, uint n)
    {
        GmpInteger r = new();
        RootInplace(r, op, n);
        return r;
    }

    /// <summary>
    /// r = sqrt(op, n) + reminder
    /// </summary>\
    public static unsafe void RootReminderInplace(GmpInteger r, GmpInteger reminder, GmpInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* preminder = &reminder.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_rootrem((IntPtr)pr, (IntPtr)preminder, (IntPtr)pop, n);
        }
    }

    /// <returns>(root, reminder)</returns>
    public static (GmpInteger root, GmpInteger reminder) RootReminder(GmpInteger op, uint n)
    {
        GmpInteger root = new(), reminder = new();
        RootReminderInplace(root, reminder, op, n);
        return (root, reminder);
    }

    /// <returns>true if op is a perfect power, i.e., if there exist integers a and b, with b>1, such that op equals a raised to the power b.</returns>
    /// <remarks>
    /// <para>Under this definition both 0 and 1 are considered to be perfect powers.</para>
    /// <para>Negative values of op are accepted, but of course can only be odd perfect powers.</para>
    /// </remarks>
    public unsafe bool HasPerfectPower()
    {
        fixed (Mpz_t* pop = &Raw)
        {
            return GmpLib.__gmpz_perfect_power_p((IntPtr)pop) != 0;
        }
    }

    /// <returns>
    /// <para>true if op is a perfect square, i.e., if the square root of op is an integer.</para>
    /// <para>Under this definition both 0 and 1 are considered to be perfect squares.</para>
    /// </returns>
    public unsafe bool HasPerfectSquare()
    {
        fixed (Mpz_t* pop = &Raw)
        {
            return GmpLib.__gmpz_perfect_square_p((IntPtr)pop) != 0;
        }
    }
    #endregion

    #region Number Theoretic Functions
    /// <summary>
    /// This function performs some trial divisions, a Baillie-PSW probable prime test, then reps-24 Miller-Rabin probabilistic primality tests. A higher reps value will reduce the chances of a non-prime being identified as “probably prime”. A composite number will be identified as a prime with an asymptotic probability of less than 4^(-reps).
    /// </summary>
    /// <param name="reps">Reasonable values of reps are between 15 and 50.</param>
    /// <returns></returns>
    public unsafe PrimePossibility ProbablePrime(int reps = 15)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (PrimePossibility)GmpLib.__gmpz_probab_prime_p((IntPtr)ptr, reps);
        }
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public static unsafe void NextPrimeInplace(GmpInteger rop, GmpInteger op)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_nextprime((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public static GmpInteger NextPrime(GmpInteger op)
    {
        GmpInteger r = new();
        NextPrimeInplace(r, op);
        return r;
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public GmpInteger NextPrime()
    {
        GmpInteger r = new();
        NextPrimeInplace(r, this);
        return r;
    }

    public static unsafe void GcdInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_gcd((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static unsafe GmpInteger Gcd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        GcdInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe void GcdInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpz_gcd_ui((IntPtr)pr, (IntPtr)p1, op2);
        }
    }

    public static unsafe GmpInteger Gcd(GmpInteger op1, uint op2)
    {
        GmpInteger rop = new();
        GcdInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// <para>Set g to the greatest common divisor of a and b, and in addition set s and t to coefficients satisfying a*s + b*t = g.</para>
    /// <para>The value in g is always positive, even if one or both of a and b are negative (or zero if both inputs are zero).</para>
    /// <para>The values in s and t are chosen such that normally, abs(s) &lt; abs(b) / (2 g) and abs(t) &lt; abs(a) / (2 g), and these relations define s and t uniquely.</para>
    /// <para>There are a few exceptional cases:</para>
    /// <list type="bullet">
    /// <item>If abs(a) = abs(b), then s = 0, t = sgn(b).</item>
    /// <item>Otherwise, s = sgn(a) if b = 0 or abs(b) = 2 g, and t = sgn(b) if a = 0 or abs(a) = 2 g.</item>
    /// <item>In all cases, s = 0 if and only if g = abs(b), i.e., if b divides a or a = b = 0.</item>
    /// <item>If t or g is NULL then that value is not computed.</item>
    /// </list>
    /// </summary>
    public static unsafe void Gcd2Inplace(GmpInteger g, GmpInteger s, GmpInteger t, GmpInteger a, GmpInteger b)
    {
        fixed (Mpz_t* pg = &g.Raw)
        fixed (Mpz_t* ps = &s.Raw)
        fixed (Mpz_t* pt = &t.Raw)
        fixed (Mpz_t* pa = &a.Raw)
        fixed (Mpz_t* pb = &b.Raw)
        {
            GmpLib.__gmpz_gcdext((IntPtr)pg, (IntPtr)ps, (IntPtr)pt, (IntPtr)pa, (IntPtr)pb);
        }
    }

    /// <summary>
    /// <para>Set g to the greatest common divisor of a and b, and in addition set s and t to coefficients satisfying a*s + b*t = g.</para>
    /// <para>The value in g is always positive, even if one or both of a and b are negative (or zero if both inputs are zero).</para>
    /// <para>The values in s and t are chosen such that normally, abs(s) &lt; abs(b) / (2 g) and abs(t) &lt; abs(a) / (2 g), and these relations define s and t uniquely.</para>
    /// <para>There are a few exceptional cases:</para>
    /// <list type="bullet">
    /// <item>If abs(a) = abs(b), then s = 0, t = sgn(b).</item>
    /// <item>Otherwise, s = sgn(a) if b = 0 or abs(b) = 2 g, and t = sgn(b) if a = 0 or abs(a) = 2 g.</item>
    /// <item>In all cases, s = 0 if and only if g = abs(b), i.e., if b divides a or a = b = 0.</item>
    /// <item>If t or g is NULL then that value is not computed.</item>
    /// </list>
    /// </summary>
    public static unsafe (GmpInteger g, GmpInteger s, GmpInteger t) Gcd2(GmpInteger a, GmpInteger b)
    {
        GmpInteger g = new();
        GmpInteger s = new();
        GmpInteger t = new();
        Gcd2Inplace(g, s, t, a, b);
        return (g, s, t);
    }

    public static unsafe void LcmInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_lcm((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static unsafe GmpInteger Lcm(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        LcmInplace(rop, op1, op2);
        return rop;
    }

    public static unsafe void LcmInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpz_lcm_ui((IntPtr)pr, (IntPtr)p1, op2);
        }
    }

    public static unsafe GmpInteger Lcm(GmpInteger op1, uint op2)
    {
        GmpInteger rop = new();
        LcmInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Set rop to the modular inverse of op1 mod op2, i.e. b, where op1 * b mod op2 = 1
    /// </summary>
    /// <param name="rop"></param>
    /// <param name="op1"></param>
    /// <param name="op2"></param>
    /// <returns>true if find the inverse.</returns>
    public static unsafe bool InvertInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_invert((IntPtr)pr, (IntPtr)p1, (IntPtr)p2) != 0;
        }
    }

    /// <summary>
    /// Set rop to the modular inverse of op1 mod op2, i.e. b, where op1 * b mod op2 = 1
    /// </summary>
    public static unsafe GmpInteger Invert(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        if (!InvertInplace(rop, op1, op2))
        {
            throw new ArgumentException($"Unable to find inverse of op1 and op2.\n op1: {op1}\n op2: {op2}");
        }
        return rop;
    }

    /// <summary>
    /// Calculate the Jacobi symbol (a/b). This is defined only for b odd.
    /// </summary>
    public static unsafe int Jacobi(GmpInteger a, GmpInteger b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_jacobi((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// <para>Calculate the Legendre symbol (a/p). </para>
    /// <para>This is defined only for p an odd positive prime, and for such p it’s identical to the Jacobi symbol.</para>
    /// </summary>
    public static unsafe int Legendre(GmpInteger a, GmpInteger p) => Jacobi(a, p);

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static unsafe int Kronecker(GmpInteger a, GmpInteger b) => Jacobi(a, b);

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static unsafe int Kronecker(GmpInteger a, int b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        {
            return GmpLib.__gmpz_kronecker_si((IntPtr)p1, b);
        }
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static unsafe int Kronecker(GmpInteger a, uint b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        {
            return GmpLib.__gmpz_kronecker_ui((IntPtr)p1, b);
        }
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static unsafe int Kronecker(int a, GmpInteger b)
    {
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_si_kronecker(a, (IntPtr)p2);
        }
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static unsafe int Kronecker(uint a, GmpInteger b)
    {
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_ui_kronecker(a, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Remove all occurrences of the factor f from op and store the result in rop.
    /// </summary>
    /// <returns>The return value is how many such occurrences were removed.</returns>
    public static unsafe uint RemoveFactorInplace(GmpInteger rop, GmpInteger op, GmpInteger f)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        fixed (Mpz_t* pf = &f.Raw)
        {
            return GmpLib.__gmpz_remove((IntPtr)pr, (IntPtr)pop, (IntPtr)pf);
        }
    }

    /// <summary>
    /// Remove all occurrences of the factor f from op and store the result in rop.
    /// </summary>
    public static GmpInteger RemoveFactor(GmpInteger op, GmpInteger f)
    {
        GmpInteger rop = new();
        RemoveFactorInplace(rop, op, f);
        return rop;
    }

    /// <summary>
    /// Remove all occurrences of the factor f from op and store the result in rop.
    /// </summary>
    public GmpInteger RemoveFactor(GmpInteger f) => RemoveFactor(this, f);

    /// <summary>
    /// computes the plain factorial n!
    /// </summary>
    public static unsafe void FactorialInplace(GmpInteger rop, uint n)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_fac_ui((IntPtr)pr, n);
        }
    }

    /// <summary>
    /// computes the plain factorial n!
    /// </summary>
    public static GmpInteger Factorial(uint n)
    {
        GmpInteger rop = new();
        FactorialInplace(rop, n);
        return rop;
    }

    /// <summary>
    /// computes the double-factorial n!!
    /// </summary>
    public static unsafe void Factorial2Inplace(GmpInteger rop, uint n)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_2fac_ui((IntPtr)pr, n);
        }
    }

    /// <summary>
    /// computes the double-factorial n!!
    /// </summary>
    public static unsafe GmpInteger Factorial2(uint n)
    {
        GmpInteger rop = new();
        Factorial2Inplace(rop, n);
        return rop;
    }

    /// <summary>
    /// computes the m-multi-factorial n!^(m)
    /// </summary>
    public static unsafe void FactorialMInplace(GmpInteger rop, uint n, uint m)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_mfac_uiui((IntPtr)pr, n, m);
        }
    }

    /// <summary>
    /// computes the m-multi-factorial n!^(m)
    /// </summary>
    public static unsafe GmpInteger FactorialM(uint n, uint m)
    {
        GmpInteger rop = new();
        FactorialMInplace(rop, n, m);
        return rop;
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static unsafe void BinomialCoefficientInplace(GmpInteger rop, GmpInteger n, uint k)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_bin_ui((IntPtr)pr, (IntPtr)pn, k);
        }
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static unsafe GmpInteger BinomialCoefficient(GmpInteger n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static unsafe void BinomialCoefficientInplace(GmpInteger rop, uint n, uint k)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_bin_uiui((IntPtr)pr, n, k);
        }
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static unsafe GmpInteger BinomialCoefficient(uint n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static unsafe void FibonacciInplace(GmpInteger fn, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        {
            GmpLib.__gmpz_fib_ui((IntPtr)pr, n);
        }
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static unsafe GmpInteger Fibonacci(uint n)
    {
        GmpInteger fn = new();
        FibonacciInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static unsafe void Fibonacci2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        fixed (Mpz_t* pr1 = &fnsub1.Raw)
        {
            GmpLib.__gmpz_fib2_ui((IntPtr)pr, (IntPtr)pr1, n);
        }
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static unsafe (GmpInteger fn, GmpInteger fnsub1) Fibonacci2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        Fibonacci2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static unsafe void LucasNumInplace(GmpInteger fn, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        {
            GmpLib.__gmpz_lucnum_ui((IntPtr)pr, n);
        }
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static unsafe GmpInteger LucasNum(uint n)
    {
        GmpInteger fn = new();
        LucasNumInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static unsafe void LucasNum2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        fixed (Mpz_t* pr1 = &fnsub1.Raw)
        {
            GmpLib.__gmpz_lucnum2_ui((IntPtr)pr, (IntPtr)pr1, n);
        }
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static unsafe (GmpInteger fn, GmpInteger fnsub1) LucasNum2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        LucasNum2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }
    #endregion

    #region Comparison Functions
    public static unsafe int Compare(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_cmp((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static bool operator ==(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) != 0;
    public static bool operator >(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) > 0;
    public static bool operator <(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) < 0;
    public static bool operator >=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) <= 0;

    public static unsafe int Compare(GmpInteger op1, double op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_d((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(GmpInteger op1, double op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpInteger op1, double op2) => Compare(op1, op2) != 0;
    public static bool operator >(GmpInteger op1, double op2) => Compare(op1, op2) > 0;
    public static bool operator <(GmpInteger op1, double op2) => Compare(op1, op2) < 0;
    public static bool operator >=(GmpInteger op1, double op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(GmpInteger op1, double op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(double op1, GmpInteger op2) => Compare(op2, op1) == 0;
    public static bool operator !=(double op1, GmpInteger op2) => Compare(op2, op1) != 0;
    public static bool operator >(double op1, GmpInteger op2) => Compare(op2, op1) < 0;
    public static bool operator <(double op1, GmpInteger op2) => Compare(op2, op1) > 0;
    public static bool operator >=(double op1, GmpInteger op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(double op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    public static unsafe int Compare(GmpInteger op1, int op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_si((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(GmpInteger op1, int op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpInteger op1, int op2) => Compare(op1, op2) != 0;
    public static bool operator >(GmpInteger op1, int op2) => Compare(op1, op2) > 0;
    public static bool operator <(GmpInteger op1, int op2) => Compare(op1, op2) < 0;
    public static bool operator >=(GmpInteger op1, int op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(GmpInteger op1, int op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(int op1, GmpInteger op2) => Compare(op2, op1) == 0;
    public static bool operator !=(int op1, GmpInteger op2) => Compare(op2, op1) != 0;
    public static bool operator >(int op1, GmpInteger op2) => Compare(op2, op1) < 0;
    public static bool operator <(int op1, GmpInteger op2) => Compare(op2, op1) > 0;
    public static bool operator >=(int op1, GmpInteger op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(int op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    public static unsafe int Compare(GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_ui((IntPtr)p1, op2);
        }
    }

    public static bool operator ==(GmpInteger op1, uint op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpInteger op1, uint op2) => Compare(op1, op2) != 0;
    public static bool operator >(GmpInteger op1, uint op2) => Compare(op1, op2) > 0;
    public static bool operator <(GmpInteger op1, uint op2) => Compare(op1, op2) < 0;
    public static bool operator >=(GmpInteger op1, uint op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(GmpInteger op1, uint op2) => Compare(op1, op2) <= 0;
    public static bool operator ==(uint op1, GmpInteger op2) => Compare(op2, op1) == 0;
    public static bool operator !=(uint op1, GmpInteger op2) => Compare(op2, op1) != 0;
    public static bool operator >(uint op1, GmpInteger op2) => Compare(op2, op1) < 0;
    public static bool operator <(uint op1, GmpInteger op2) => Compare(op2, op1) > 0;
    public static bool operator >=(uint op1, GmpInteger op2) => Compare(op2, op1) <= 0;
    public static bool operator <=(uint op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            GmpInteger gi => this == gi,
            double dbl => this == dbl,
            int si => this == si, 
            uint ui => this == ui, 
            _ => false, 
        };
    }

    public override int GetHashCode() => Raw.GetHashCode();

    public static unsafe int CompareAbs(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_cmpabs((IntPtr)p1, (IntPtr)p2);
        }
    }

    public static unsafe int CompareAbs(GmpInteger op1, double op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmpabs_d((IntPtr)p1, op2);
        }
    }

    public static unsafe int CompareAbs(GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmpabs_ui((IntPtr)p1, op2);
        }
    }

    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;
    #endregion

    #region Logical and Bit Manipulation Functions
    public static unsafe void BitwiseAndInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_and((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static GmpInteger BitwiseAnd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseAndInplace(rop, op1, op2);
        return rop;
    }

    public static GmpInteger operator &(GmpInteger op1, GmpInteger op2) => BitwiseAnd(op1, op2);

    public static unsafe void BitwiseOrInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_ior((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static GmpInteger BitwiseOr(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseOrInplace(rop, op1, op2);
        return rop;
    }

    public static GmpInteger operator |(GmpInteger op1, GmpInteger op2) => BitwiseOr(op1, op2);

    public static unsafe void BitwiseXorInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_xor((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static GmpInteger BitwiseXor(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseXorInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// bitwise xor
    /// </summary>
    public static GmpInteger operator ^(GmpInteger op1, GmpInteger op2) => BitwiseXor(op1, op2);

    /// <summary>
    /// 1001 -> 0110
    /// </summary>
    public static unsafe void ComplementInplace(GmpInteger rop, GmpInteger op)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p = &op.Raw)
        {
            GmpLib.__gmpz_com((IntPtr)pr, (IntPtr)p);
        }
    }

    /// <summary>
    /// 1001 -> 0110
    /// </summary>
    public static GmpInteger Complement(GmpInteger op)
    {
        GmpInteger rop = new();
        ComplementInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// <para>If op &gt;= 0, return the population count of op, which is the number of 1 bits in the binary representation.</para>
    /// <para>If op &lt; 0, the number of 1s is infinite, and the return value is the largest possible mp_bitcnt_t.</para>
    /// </summary>
    /// <returns></returns>
    public unsafe uint PopulationCount()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_popcount((IntPtr)ptr);
        }
    }

    /// <summary>
    /// <para>
    /// If op1 and op2 are both &gt;= 0 or both &lt; 0, return the hamming distance between the two operands, 
    /// which is the number of bit positions where op1 and op2 have different bit values. 
    /// </para>
    /// <para>
    /// If one operand is &gt;=0 and the other &lt; 0 then the number of bits different is infinite, 
    /// and the return value is the largest possible mp_bitcnt_t.
    /// </para>
    /// </summary>
    public static unsafe uint HammingDistance(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_hamdist((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// <para><see cref="GmpLib.__gmpz_scan0"/></para>
    /// <para>Scan op, starting from bit starting_bit, towards more significant bits, until the first 0 is found.</para>
    /// <para>If the bit at starting_bit is already what’s sought, then starting_bit is returned.</para>
    /// <para>
    /// If there’s no bit found, then the largest possible mp_bitcnt_t is returned. 
    /// This will happen in past the end of a negative number.
    /// </para>
    /// </summary>
    /// <returns>The index of the found bit.</returns>
    public unsafe uint FirstIndexOf0(uint startingBit = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_scan0((IntPtr)ptr, startingBit);
        }
    }

    /// <summary>
    /// <para><see cref="GmpLib.__gmpz_scan1"/></para>
    /// <para>Scan op, starting from bit starting_bit, towards more significant bits, until the first 1 is found.</para>
    /// <para>If the bit at starting_bit is already what’s sought, then starting_bit is returned.</para>
    /// <para>
    /// If there’s no bit found, then the largest possible mp_bitcnt_t is returned. 
    /// This will happen in past the end of a nonnegative number.
    /// </para>
    /// </summary>
    /// <returns>The index of the found bit.</returns>
    public unsafe uint FirstIndexOf1(uint startingBit = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_scan1((IntPtr)ptr, startingBit);
        }
    }

    public unsafe void SetBit(uint bitIndex)
    {
        fixed(Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_setbit((IntPtr)ptr, bitIndex);
        }
    }

    public unsafe void ClearBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_clrbit((IntPtr)ptr, bitIndex);
        }
    }

    public unsafe void ComplementBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_combit((IntPtr)ptr, bitIndex);
        }
    }

    public unsafe int TestBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_tstbit((IntPtr)ptr, bitIndex);
        }
    }
    #endregion

    #region Obsoleted Random
    /// <summary>
    /// Generate a random integer of at most max_size limbs.
    /// The generated random number doesn’t satisfy any particular requirements of randomness.
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static unsafe void RandomInplace(GmpInteger rop, int maxLimbCount)
    {
        fixed (Mpz_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpz_random((IntPtr)ptr, maxLimbCount);
        }
    }

    /// <summary>
    /// Generate a random integer of at most max_size limbs.
    /// The generated random number doesn’t satisfy any particular requirements of randomness.
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static unsafe GmpInteger Random(int maxLimbCount)
    {
        GmpInteger rop = new();
        RandomInplace(rop, maxLimbCount);
        return rop;
    }

    /// <summary>
    /// Generate a random integer of at most max_size limbs, 
    /// with long strings of zeros and ones in the binary representation. 
    /// Useful for testing functions and algorithms, 
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static unsafe void Random2Inplace(GmpInteger rop, int maxLimbCount)
    {
        fixed (Mpz_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpz_random((IntPtr)ptr, maxLimbCount);
        }
    }

    /// <summary>
    /// Generate a random integer of at most max_size limbs, 
    /// with long strings of zeros and ones in the binary representation. 
    /// Useful for testing functions and algorithms, 
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static unsafe GmpInteger Random2(int maxLimbCount)
    {
        GmpInteger rop = new();
        RandomInplace(rop, maxLimbCount);
        return rop;
    }
    #endregion
}

public enum PrimePossibility
{
    /// <summary>
    /// definitely non-prime
    /// </summary>
    No = 0,

    /// <summary>
    /// probably prime (without being certain)
    /// </summary>
    Probably = 1,

    /// <summary>
    /// definitely prime
    /// </summary>
    Yes = 2,
}

public record struct Mpz_t
{
    public int Allocated;
    public int Size;
    /// <summary>
    /// nint*
    /// </summary>
    public IntPtr Limbs;

    public static int RawSize => Marshal.SizeOf<Mpz_t>();

    private unsafe Span<nint> GetLimbData() => new((void*)Limbs, Allocated);

    public override int GetHashCode()
    {
        HashCode c = new();
        c.Add(Allocated);
        c.Add(Size);
        foreach (nint i in GetLimbData())
        {
            c.Add(i);
        }
        return c.ToHashCode();
    }
}