﻿using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Sdcb.Math.Gmp;

public class BigInteger : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpNative.__gmpf_get_default_prec();
        set => GmpNative.__gmpf_set_default_prec(value);
    }

    public Mpz_t Raw = new();
    private bool _disposed = false;

    #region Initializing Integers
    public unsafe BigInteger()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_init((IntPtr)ptr);
        }
    }

    public unsafe BigInteger(Mpz_t raw)
    {
        Raw = raw;
    }

    public unsafe BigInteger(uint bitCount)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_init2((IntPtr)ptr, bitCount);
        }
    }

    /// <summary>
    /// Change the space for integer to new_alloc limbs. The value in integer is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByLimbs(int limbs)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_realloc((IntPtr)ptr, limbs);
        }
    }

    /// <summary>
    /// Change the space allocated for x to n bits. The value in x is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public unsafe void ReallocByBits(uint bits)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_realloc2((IntPtr)ptr, bits);
        }
    }

    public unsafe void ReallocToFit() => ReallocByLimbs(System.Math.Abs(Raw.Size));
    #endregion

    #region Assignment Functions
    public unsafe void Assign(BigInteger op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(uint op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_ui((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(int op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_si((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(double op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpNative.__gmpz_set_d((IntPtr)ptr, op);
        }
    }

    public unsafe void Assign(BigRational op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set_q((IntPtr)ptr, (IntPtr)pop);
        }
    }

    public unsafe void Assign(BigFloat op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_set_f((IntPtr)ptr, (IntPtr)pop);
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
                int ret = GmpNative.__gmpz_set_str((IntPtr)ptr, (IntPtr)opPtr, opBase);
                if (ret != 0)
                {
                    throw new FormatException($"Failed to parse \"{op}\", base={opBase} to BigInteger, __gmpz_set_str returns {ret}");
                }
            }
        }
    }

    public static unsafe void Swap(BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_swap((IntPtr)pop1, (IntPtr)pop2);
        }
    }
    #endregion

    #region Combined Initialization and Assignment Functions
    public static unsafe BigInteger From(BigInteger op)
    {
        Mpz_t raw = new();
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_init_set((IntPtr)(&raw), (IntPtr)pop);
        }
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(uint op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_ui((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(int op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_si((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    public static unsafe BigInteger From(double op)
    {
        Mpz_t raw = new();
        GmpNative.__gmpz_init_set_d((IntPtr)(&raw), op);
        return new BigInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static BigInteger Parse(string val, int valBase = 0)
    {
        Mpz_t raw = new();
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpNative.__gmpz_init_set_str((IntPtr)(&raw), (IntPtr)pval, valBase);
            if (ret != 0)
            {
                GmpNative.__gmpz_clear((IntPtr)(&raw));
                throw new FormatException($"Failed to parse {val}, base={valBase} to BigInteger, __gmpf_init_set_str returns {ret}");
            }
        }
        return new BigInteger(raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe static bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out BigInteger result, int valBase = 10)
    {
        Mpz_t raw = new();
        Mpz_t* ptr = &raw;
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpNative.__gmpz_init_set_str((IntPtr)ptr, (IntPtr)pval, valBase);
            if (rt != 0)
            {
                GmpNative.__gmpz_clear((IntPtr)ptr);
                result = null;
                return false;
            }
            else
            {
                result = new BigInteger(raw);
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
            GmpNative.__gmpz_clear((IntPtr)ptr);
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

    ~BigInteger()
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
    public static unsafe void AddInplace(BigInteger r, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_add((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static BigInteger Add(BigInteger op1, BigInteger op2)
    {
        BigInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator +(BigInteger a, BigInteger b) => Add(a, b);

    public static unsafe void AddInplace(BigInteger r, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_add_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static BigInteger Add(BigInteger op1, uint op2)
    {
        BigInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator +(BigInteger a, uint b) => Add(a, b);
    public static BigInteger operator +(uint a, BigInteger b) => Add(b, a);

    public static unsafe void SubtractInplace(BigInteger r, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_sub((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static BigInteger Subtract(BigInteger op1, BigInteger op2)
    {
        BigInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator -(BigInteger op1, BigInteger op2) => Subtract(op1, op2);

    public static unsafe void SubtractInplace(BigInteger r, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_sub_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static BigInteger Subtract(BigInteger op1, uint op2)
    {
        BigInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator -(BigInteger op1, uint op2) => Subtract(op1, op2);

    public static unsafe void SubtractInplace(BigInteger r, uint op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_ui_sub((IntPtr)pr, op1, (IntPtr)pop2);
        }
    }

    public static BigInteger Subtract(uint op1, BigInteger op2)
    {
        BigInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator -(uint op1, BigInteger op2) => Subtract(op1, op2);

    public static unsafe void MultipleInplace(BigInteger r, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_mul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    public static BigInteger Multiple(BigInteger op1, BigInteger op2)
    {
        BigInteger r = new();
        MultipleInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator *(BigInteger op1, BigInteger op2) => Multiple(op1, op2);

    public static unsafe void MultipleInplace(BigInteger r, BigInteger op1, int op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_mul_si((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static BigInteger Multiple(BigInteger op1, int op2)
    {
        BigInteger r = new();
        MultipleInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator *(BigInteger op1, int op2) => Multiple(op1, op2);
    public static BigInteger operator *(int op1, BigInteger op2) => Multiple(op2, op1);

    public static unsafe void MultipleInplace(BigInteger r, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_mul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static BigInteger Multiple(BigInteger op1, uint op2)
    {
        BigInteger r = new();
        MultipleInplace(r, op1, op2);
        return r;
    }

    public static BigInteger operator *(BigInteger op1, uint op2) => Multiple(op1, op2);
    public static BigInteger operator *(uint op1, BigInteger op2) => Multiple(op2, op1);

    /// <summary>
    /// r += op1 * op2
    /// </summary>
    public static unsafe void AddMultiply(BigInteger r, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_addmul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// r += op1 * op2
    /// </summary>
    public static unsafe void AddMultiply(BigInteger r, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_addmul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static unsafe void SubtractMultiply(BigInteger r, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpNative.__gmpz_submul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static unsafe void SubtractMultiply(BigInteger r, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_submul_ui((IntPtr)pr, (IntPtr)pop1, op2);
        }
    }

    public static unsafe void Multiple2ExpInplace(BigInteger r, BigInteger op1, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_mul_2exp((IntPtr)pr, (IntPtr)pop1, exp2);
        }
    }

    public static unsafe BigInteger Multiple2Exp(BigInteger op1, uint exp2)
    {
        BigInteger r = new();
        Multiple2ExpInplace(r, op1, exp2);
        return r;
    }

    public void LeftShiftInplace(uint bits) => Multiple2ExpInplace(this, this, bits);

    public static BigInteger operator <<(BigInteger op1, uint exp2) => Multiple2Exp(op1, exp2);

    public static unsafe void NegateInplace(BigInteger r, BigInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_neg((IntPtr)pr, (IntPtr)pop1);
        }
    }

    public static BigInteger Negate(BigInteger op1)
    {
        BigInteger r = new();
        NegateInplace(r, op1);
        return r;
    }

    public static BigInteger operator -(BigInteger op1) => Negate(op1);

    public static unsafe void AbsInplace(BigInteger r, BigInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpNative.__gmpz_abs((IntPtr)pr, (IntPtr)pop1);
        }
    }

    public static BigInteger Abs(BigInteger op1)
    {
        BigInteger r = new();
        AbsInplace(r, op1);
        return r;
    }
    #endregion

    #region Conversion Functions
    public unsafe uint ToUInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_ui((IntPtr)ptr);
        }
    }

    public unsafe int ToInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_si((IntPtr)ptr);
        }
    }

    public unsafe double ToDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpNative.__gmpz_get_d((IntPtr)ptr);
        }
    }

    public static explicit operator uint(BigInteger op) => op.ToUInt32();
    public static explicit operator int(BigInteger op) => op.ToInt32();
    public static explicit operator double(BigInteger op) => op.ToDouble();
    public static explicit operator BigFloat(BigInteger op) => BigFloat.From(op);

    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            int exp = default;
            double val = GmpNative.__gmpz_get_d_2exp((IntPtr)(&exp), (IntPtr)ptr);
            return new ExpDouble(exp, val);
        }
    }

    public override string ToString() => ToString(10);

    public unsafe string ToString(int opBase)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            IntPtr ret = GmpNative.__gmpz_get_str(IntPtr.Zero, opBase, (IntPtr)ptr);
            if (ret == IntPtr.Zero)
            {
                throw new ArgumentException($"Unable to convert BigInteger to string.");
            }

            return Marshal.PtrToStringUTF8(ret)!;
        }
    }
    #endregion

    #region Division Functions
    #region Ceililng
    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    public static unsafe void CeilingDivideInplace(BigInteger q, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_cdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (ceiling)
    /// </summary>
    public static unsafe BigInteger CeilingDivide(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    public static unsafe void CeilingReminderInplace(BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_cdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static unsafe BigInteger CeilingReminder(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        CeilingReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    public static unsafe void CeilingDivRemInplace(BigInteger q, BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_cdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) CeilingDivRem(BigInteger n, BigInteger d)
    {
        BigInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingDivideInplace(BigInteger q, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_cdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static unsafe BigInteger CeilingDivide(BigInteger n, uint d)
    {
        BigInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingReminderInplace(BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_cdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint CeilingDivRemInplace(BigInteger q, BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_cdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) CeilingDivRem(BigInteger n, uint d)
    {
        BigInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (ceiling)</returns>
    public static unsafe uint CeilingReminderToUInt32(BigInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_cdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (ceiling)</returns>
    public static BigInteger CeilingReminder(BigInteger n, uint d)
    {
        BigInteger r = new();
        CeilingReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void CeilingDivide2ExpInplace(BigInteger q, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_cdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (ceiling)
    /// </summary>
    public static unsafe BigInteger CeilingDivide2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        CeilingDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void CeilingReminder2ExpInplace(BigInteger r, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_cdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (ceiling)
    /// </summary>
    public static unsafe BigInteger CeilingReminder2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        CeilingReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Floor
    /// <summary>
    /// q = n / d (Floor)
    /// </summary>
    public static unsafe void FloorDivideInplace(BigInteger q, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_fdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (Floor)
    /// </summary>
    public static unsafe BigInteger FloorDivide(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    public static unsafe void FloorReminderInplace(BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_fdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static unsafe BigInteger FloorReminder(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        FloorReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    public static unsafe void FloorDivRemInplace(BigInteger q, BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_fdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) FloorDivRem(BigInteger n, BigInteger d)
    {
        BigInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorDivideInplace(BigInteger q, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_fdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static unsafe BigInteger FloorDivide(BigInteger n, uint d)
    {
        BigInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorReminderInplace(BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_fdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint FloorDivRemInplace(BigInteger q, BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_fdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) FloorDivRem(BigInteger n, uint d)
    {
        BigInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Floor)</returns>
    public static unsafe uint FloorReminderToUInt32(BigInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_fdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (Floor)</returns>
    public static unsafe BigInteger FloorReminder(BigInteger n, uint d)
    {
        BigInteger r = new();
        FloorReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void FloorDivide2ExpInplace(BigInteger q, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_fdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (Floor)
    /// </summary>
    public static unsafe BigInteger FloorDivide2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        FloorDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void FloorReminder2ExpInplace(BigInteger r, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_fdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (Floor)
    /// </summary>
    public static unsafe BigInteger FloorReminder2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        FloorReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Truncate
    /// <summary>
    /// q = n / d (Truncate)
    /// </summary>
    public static unsafe void DivideInplace(BigInteger q, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_tdiv_q((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n / d (Truncate)
    /// </summary>
    public static unsafe BigInteger Divide(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    public static unsafe void ReminderInplace(BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_tdiv_r((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static unsafe BigInteger Reminder(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        ReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    public static unsafe void DivRemInplace(BigInteger q, BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_tdiv_qr((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) DivRem(BigInteger n, BigInteger d)
    {
        BigInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint DivideInplace(BigInteger q, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_tdiv_q_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static unsafe BigInteger Divide(BigInteger n, uint d)
    {
        BigInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint ReminderInplace(BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_tdiv_r_ui((IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe uint DivRemInplace(BigInteger q, BigInteger r, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_tdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static unsafe (BigInteger q, BigInteger r) DivRem(BigInteger n, uint d)
    {
        BigInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Truncate)</returns>
    public static unsafe uint ReminderToUInt32(BigInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_tdiv_ui((IntPtr)pn, d);
        }
    }

    /// <returns>n mod d (Truncate)</returns>
    public static unsafe BigInteger Reminder(BigInteger n, uint d)
    {
        BigInteger r = new();
        ReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void Divide2ExpInplace(BigInteger q, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_tdiv_q_2exp((IntPtr)pq, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n / (2 ^ d) (Truncate)
    /// </summary>
    public static unsafe BigInteger Divide2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        Divide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static unsafe void Reminder2ExpInplace(BigInteger r, BigInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_tdiv_r_2exp((IntPtr)pr, (IntPtr)pn, exp2);
        }
    }

    /// <summary>
    /// return n mod (2 ^ d) (Truncate)
    /// </summary>
    public static unsafe BigInteger Reminder2Exp(BigInteger n, uint exp2)
    {
        BigInteger q = new();
        Reminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region operators
    public static BigInteger operator /(BigInteger op1, BigInteger op2) => Divide(op1, op2);

    public static BigInteger operator %(BigInteger op1, BigInteger op2) => Reminder(op1, op2);

    public static BigInteger operator /(BigInteger op1, uint op2) => Divide(op1, op2);

    public static BigInteger operator %(BigInteger op1, uint op2) => Reminder(op1, op2);
    #endregion

    #region Others
    /// <summary>
    /// Set r to n mod d. The sign of the divisor is ignored; the result is always non-negative.
    /// </summary>
    public static unsafe void ModInplace(BigInteger r, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_mod((IntPtr)pr, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// return n mod d. The sign of the divisor is ignored; the result is always non-negative.
    /// </summary>
    public static unsafe BigInteger Mod(BigInteger n, BigInteger d)
    {
        BigInteger r = new();
        ModInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// <para>Set r to n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static unsafe uint ModInplace(BigInteger r, BigInteger n, uint d) => FloorReminderInplace(r, n, d);

    /// <summary>
    /// <para>return n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static unsafe BigInteger Mod(BigInteger n, uint d) => FloorReminder(n, d);

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe void DivExactInplace(BigInteger q, BigInteger n, BigInteger d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            GmpNative.__gmpz_divexact((IntPtr)pq, (IntPtr)pn, (IntPtr)pd);
        }
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe BigInteger DivExact(BigInteger n, BigInteger d)
    {
        BigInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe void DivExactInplace(BigInteger q, BigInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpNative.__gmpz_divexact_ui((IntPtr)pq, (IntPtr)pn, d);
        }
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static unsafe BigInteger DivExact(BigInteger n, uint d)
    {
        BigInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// <para>n is congruent to c mod d if there exists an integer q satisfying n = c + q*d.</para>
    /// <para>Unlike the other division functions, d=0 is accepted and following the rule it can be seen that n and c are considered congruent mod 0 only when exactly equal.</para>
    /// </summary>
    /// <returns>true if n = c mod d</returns>
    public static unsafe bool Congruent(BigInteger n, BigInteger c, BigInteger d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pc = &c.Raw)
        fixed (Mpz_t* pd = &d.Raw)
        {
            return GmpNative.__gmpz_congruent_p((IntPtr)pn, (IntPtr)pc, (IntPtr)pd) != 0;
        }
    }

    /// <summary>
    /// <para>n is congruent to c mod d if there exists an integer q satisfying n = c + q*d.</para>
    /// <para>Unlike the other division functions, d=0 is accepted and following the rule it can be seen that n and c are considered congruent mod 0 only when exactly equal.</para>
    /// </summary>
    /// <returns>true if n = c mod d</returns>
    public static unsafe bool Congruent(BigInteger n, uint c, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpNative.__gmpz_congruent_ui_p((IntPtr)pn, c, d) != 0;
        }
    }

    /// <summary>
    /// <para>n is congruent to c mod (2^b) if there exists an integer q satisfying n = c + q*(2^b).</para>
    /// </summary>
    /// <returns>true if n = c mod (2^b)</returns>
    public static unsafe bool Congruent2Exp(BigInteger n, BigInteger c, uint b)
    {
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pc = &c.Raw)
        {
            return GmpNative.__gmpz_congruent_2exp_p((IntPtr)pn, (IntPtr)pc, b) != 0;
        }
    }
    #endregion
    #endregion

    #region Exponentiation Functions
    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    public static unsafe void PowerModInplace(BigInteger r, BigInteger @base, BigInteger exp, BigInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pexp = &exp.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpNative.__gmpz_powm((IntPtr)pr, (IntPtr)pbase, (IntPtr)pexp, (IntPtr)pmod);
        }
    }

    /// <returns>(@base ^ exp) % mod</returns>
    public static BigInteger PowerMod(BigInteger @base, BigInteger exp, BigInteger mod)
    {
        BigInteger r = new();
        PowerModInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    public static unsafe void PowerModInplace(BigInteger r, BigInteger @base, uint exp, BigInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpNative.__gmpz_powm_ui((IntPtr)pr, (IntPtr)pbase, exp, (IntPtr)pmod);
        }
    }

    /// <returns>(@base ^ exp) % mod</returns>
    public static BigInteger PowerMod(BigInteger @base, uint exp, BigInteger mod)
    {
        BigInteger r = new();
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
    public static unsafe void PowerModSecureInplace(BigInteger r, BigInteger @base, BigInteger exp, BigInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pexp = &exp.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpNative.__gmpz_powm_sec((IntPtr)pr, (IntPtr)pbase, (IntPtr)pexp, (IntPtr)pmod);
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
    public static BigInteger PowerModSecure(BigInteger @base, BigInteger exp, BigInteger mod)
    {
        BigInteger r = new();
        PowerModSecureInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// r = base ^ exp
    /// </summary>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static unsafe void PowerInplace(BigInteger r, BigInteger @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        {
            GmpNative.__gmpz_pow_ui((IntPtr)pr, (IntPtr)pbase, exp);
        }
    }

    /// <returns>base ^ exp</returns>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static BigInteger Power(BigInteger @base, uint exp)
    {
        BigInteger r = new();
        PowerInplace(r, @base, exp);
        return r;
    }

    /// <summary>
    /// r = base ^ exp
    /// </summary>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static unsafe void PowerInplace(BigInteger r, uint @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        {
            GmpNative.__gmpz_ui_pow_ui((IntPtr)pr, @base, exp);
        }
    }

    /// <returns>base ^ exp</returns>
    /// <remarks>The case 0^0 yields 1</remarks>
    public static BigInteger Power(uint @base, uint exp)
    {
        BigInteger r = new();
        PowerInplace(r, @base, exp);
        return r;
    }

    public static BigInteger operator ^(BigInteger @base, uint exp) => Power(@base, exp);
    #endregion

    #region Root Extraction Functions
    /// <summary>
    /// r = sqrt(op, n)
    /// </summary>
    /// <returns>true if computation was exact</returns>
    public static unsafe bool RootInplace(BigInteger r, BigInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            return GmpNative.__gmpz_root((IntPtr)pr, (IntPtr)pop, n) != 0;
        }
    }

    /// <returns>sqsrt(op, n)</returns>
    public static BigInteger Root(BigInteger op, uint n)
    {
        BigInteger r = new();
        RootInplace(r, op, n);
        return r;
    }

    /// <summary>
    /// r = sqrt(op, n) + reminder
    /// </summary>\
    public static unsafe void RootReminderInplace(BigInteger r, BigInteger reminder, BigInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* preminder = &reminder.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_rootrem((IntPtr)pr, (IntPtr)preminder, (IntPtr)pop, n);
        }
    }

    /// <returns>(root, reminder)</returns>
    public static (BigInteger root, BigInteger reminder) RootReminder(BigInteger op, uint n)
    {
        BigInteger root = new(), reminder = new();
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
            return GmpNative.__gmpz_perfect_power_p((IntPtr)pop) != 0;
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
            return GmpNative.__gmpz_perfect_square_p((IntPtr)pop) != 0;
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
            return (PrimePossibility)GmpNative.__gmpz_probab_prime_p((IntPtr)ptr, reps);
        }
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public static unsafe void NextPrime(BigInteger rop, BigInteger op)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpNative.__gmpz_nextprime((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public static BigInteger NextPrime(BigInteger op)
    {
        BigInteger r = new();
        NextPrime(r, op);
        return r;
    }

    public static unsafe void Gcd(BigInteger rop, BigInteger op1, BigInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpNative.__gmpz_gcd((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    public static unsafe BigInteger Gcd(BigInteger op1, BigInteger op2)
    {
        BigInteger rop = new();
        Gcd(rop, op1, op2);
        return rop;
    }

    public static unsafe void Gcd(BigInteger rop, BigInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            GmpNative.__gmpz_gcd_ui((IntPtr)pr, (IntPtr)p1, op2);
        }
    }

    public static unsafe BigInteger Gcd(BigInteger op1, uint op2)
    {
        BigInteger rop = new();
        Gcd(rop, op1, op2);
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

    private unsafe Span<nint> GetLimbData() => new Span<nint>((void*)Limbs, Allocated);

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