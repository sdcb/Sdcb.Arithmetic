using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sdcb.Arithmetic.Gmp;

public class GmpInteger : IDisposable
{
    public static uint DefaultPrecision
    {
        get => GmpLib.__gmpf_get_default_prec();
        set => GmpLib.__gmpf_set_default_prec(value);
    }

    public IntPtr Raw;
    private bool _disposed;
    private readonly bool _isOwner;

    #region Initializing Integers
    public GmpInteger()
    {
        Raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init(Raw);
        _isOwner = true;
    }

    public GmpInteger(IntPtr raw, bool isOwner)
    {
        Raw = raw;
        _isOwner = isOwner;
    }

    public GmpInteger(uint bitCount)
    {
        Raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init2(Raw, bitCount);
        _isOwner = true;
    }

    /// <summary>
    /// Change the space for integer to new_alloc limbs. The value in integer is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public void ReallocByLimbs(int limbs)
    {
        GmpLib.__gmpz_realloc(Raw, limbs);
    }

    /// <summary>
    /// Change the space allocated for x to n bits. The value in x is preserved if it fits, or is set to 0 if not.
    /// </summary>
    public void ReallocByBits(uint bits)
    {
        GmpLib.__gmpz_realloc2(Raw, bits);
    }

    public unsafe void ReallocToFit() => ReallocByLimbs(Math.Abs(((Mpz_t*)Raw)->Size));
    #endregion

    #region Assignment Functions
    public void Assign(GmpInteger op)
    {
        GmpLib.__gmpz_set(Raw, op.Raw);
    }

    public void Assign(uint op)
    {
        GmpLib.__gmpz_set_ui(Raw, op);
    }

    public void Assign(int op)
    {
        GmpLib.__gmpz_set_si(Raw, op);
    }

    public void Assign(double op)
    {
        GmpLib.__gmpz_set_d(Raw, op);
    }

    public void Assign(GmpRational op)
    {
        GmpLib.__gmpz_set_q(Raw, op.Raw);
    }

    public void Assign(GmpFloat op)
    {
        GmpLib.__gmpz_set_f(Raw, op.Raw);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public unsafe void Assign(string op, int opBase = 0)
    {
        byte[] opBytes = Encoding.UTF8.GetBytes(op);
        fixed (byte* opPtr = opBytes)
        {
            int ret = GmpLib.__gmpz_set_str(Raw, (IntPtr)opPtr, opBase);
            if (ret != 0)
            {
                throw new FormatException($"Failed to parse \"{op}\", base={opBase} to BigInteger, __gmpz_set_str returns {ret}");
            }
        }
    }

    public static void Swap(GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_swap(op1.Raw, op2.Raw);
    }
    #endregion

    #region Combined Initialization and Assignment Functions
    public static GmpInteger From(GmpInteger op)
    {
        IntPtr raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init_set(raw, op.Raw);
        return new GmpInteger(raw, isOwner: true);
    }

    public static GmpInteger From(uint op)
    {
        IntPtr raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init_set_ui(raw, op);
        return new GmpInteger(raw, isOwner: true);
    }

    public static GmpInteger From(int op)
    {
        IntPtr raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init_set_si(raw, op);
        return new GmpInteger(raw, isOwner: true);
    }

    public static GmpInteger From(double op)
    {
        IntPtr raw = Mpz_t.Alloc();
        GmpLib.__gmpz_init_set_d(raw, op);
        return new GmpInteger(raw, isOwner: true);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public static unsafe GmpInteger Parse(string val, int valBase = 0)
    {
        IntPtr raw = Mpz_t.Alloc();
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int ret = GmpLib.__gmpz_init_set_str(raw, (IntPtr)pval, valBase);
            if (ret != 0)
            {
                GmpLib.__gmpz_clear(raw);
                Marshal.FreeHGlobal(raw);
                throw new FormatException($"Failed to parse {val}, base={valBase} to BigInteger, __gmpf_init_set_str returns {ret}");
            }
        }
        return new GmpInteger(raw, isOwner: true);
    }

    /// <summary>
    /// The base may vary from 2 to 62, or if base is 0, then the leading characters are used: 0x and 0X for hexadecimal, 0b and 0B for binary, 0 for octal, or decimal otherwise.
    /// </summary>
    public static unsafe bool TryParse(string val, [MaybeNullWhen(returnValue: false)] out GmpInteger result, int valBase = 10)
    {
        IntPtr raw = Mpz_t.Alloc();
        byte[] valBytes = Encoding.UTF8.GetBytes(val);
        fixed (byte* pval = valBytes)
        {
            int rt = GmpLib.__gmpz_init_set_str(raw, (IntPtr)pval, valBase);
            if (rt != 0)
            {
                GmpLib.__gmpz_clear(raw);
                Marshal.FreeHGlobal(raw);
                result = null;
                return false;
            }
            else
            {
                result = new GmpInteger(raw, isOwner: true);
                return true;
            }
        }
    }
    #endregion

    #region Dispose and Clear

    private void Clear()
    {
        GmpLib.__gmpz_clear(Raw);
        Marshal.FreeHGlobal(Raw);
        Raw = IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }

            if (_isOwner) Clear();
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
    public static void AddInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_add(r.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger Add(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator +(GmpInteger a, GmpInteger b) => Add(a, b);

    public static void AddInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_add_ui(r.Raw, op1.Raw, op2);
    }

    public static GmpInteger Add(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator +(GmpInteger a, uint b) => Add(a, b);
    public static GmpInteger operator +(uint a, GmpInteger b) => Add(b, a);

    public static void SubtractInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_sub(r.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger Subtract(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1, GmpInteger op2) => Subtract(op1, op2);

    public static void SubtractInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_sub_ui(r.Raw, op1.Raw, op2);
    }

    public static GmpInteger Subtract(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1, uint op2) => Subtract(op1, op2);

    public static void SubtractInplace(GmpInteger r, uint op1, GmpInteger op2)
    {
        GmpLib.__gmpz_ui_sub(r.Raw, op1, op2.Raw);
    }

    public static GmpInteger Subtract(uint op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator -(uint op1, GmpInteger op2) => Subtract(op1, op2);

    public static void MultiplyInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_mul(r.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger Multiply(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator *(GmpInteger op1, GmpInteger op2) => Multiply(op1, op2);

    public static void MultiplyInplace(GmpInteger r, GmpInteger op1, int op2)
    {
        GmpLib.__gmpz_mul_si(r.Raw, op1.Raw, op2);
    }

    public static GmpInteger Multiply(GmpInteger op1, int op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    public static GmpInteger operator *(GmpInteger op1, int op2) => Multiply(op1, op2);
    public static GmpInteger operator *(int op1, GmpInteger op2) => Multiply(op2, op1);

    public static void MultiplyInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_mul_ui(r.Raw, op1.Raw, op2);
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
    public static void AddMultiply(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_addmul(r.Raw, op1.Raw, op2.Raw);
    }

    /// <summary>
    /// r += op1 * op2
    /// </summary>
    public static void AddMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_addmul_ui(r.Raw, op1.Raw, op2);
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static void SubtractMultiply(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_submul(r.Raw, op1.Raw, op2.Raw);
    }

    /// <summary>
    /// r -= op1 * op2
    /// </summary>
    public static void SubtractMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_submul_ui(r.Raw, op1.Raw, op2);
    }

    public static void Multiply2ExpInplace(GmpInteger r, GmpInteger op1, uint exp2)
    {
        GmpLib.__gmpz_mul_2exp(r.Raw, op1.Raw, exp2);
    }

    public static GmpInteger Multiply2Exp(GmpInteger op1, uint exp2)
    {
        GmpInteger r = new();
        Multiply2ExpInplace(r, op1, exp2);
        return r;
    }

    public void LeftShift(uint bits) => Multiply2ExpInplace(this, this, bits);

    public static GmpInteger operator <<(GmpInteger op1, uint exp2) => Multiply2Exp(op1, exp2);

    public static void NegateInplace(GmpInteger r, GmpInteger op1)
    {
        GmpLib.__gmpz_neg(r.Raw, op1.Raw);
    }

    public static GmpInteger Negate(GmpInteger op1)
    {
        GmpInteger r = new();
        NegateInplace(r, op1);
        return r;
    }

    public static GmpInteger operator -(GmpInteger op1) => Negate(op1);

    public static void AbsInplace(GmpInteger r, GmpInteger op1)
    {
        GmpLib.__gmpz_abs(r.Raw, op1.Raw);
    }

    public static GmpInteger Abs(GmpInteger op1)
    {
        GmpInteger r = new();
        AbsInplace(r, op1);
        return r;
    }
    #endregion

    #region Conversion Functions
    public uint ToUInt32()
    {
        return GmpLib.__gmpz_get_ui(Raw);
    }

    public int ToInt32()
    {
        return GmpLib.__gmpz_get_si(Raw);
    }

    public double ToDouble()
    {
        return GmpLib.__gmpz_get_d(Raw);
    }

    public static explicit operator uint(GmpInteger op) => op.ToUInt32();
    public static explicit operator int(GmpInteger op) => op.ToInt32();
    public static explicit operator double(GmpInteger op) => op.ToDouble();
    public static explicit operator GmpFloat(GmpInteger op) => GmpFloat.From(op);

    public unsafe ExpDouble ToExpDouble()
    {
        int exp = default;
        double val = GmpLib.__gmpz_get_d_2exp((IntPtr)(&exp), Raw);
        return new ExpDouble(exp, val);
    }

    public override string ToString() => ToString(10);

    public string ToString(int opBase)
    {
        IntPtr ret = GmpLib.__gmpz_get_str(IntPtr.Zero, opBase, Raw);
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
    #endregion

    #region Division Functions
    #region Ceililng
    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    public static void CeilingDivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_cdiv_q(q.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n / d (ceiling)
    /// </summary>
    public static GmpInteger CeilingDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    public static void CeilingReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_cdiv_r(r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static GmpInteger CeilingReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    public static void CeilingDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_cdiv_qr(q.Raw, r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint CeilingDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_cdiv_q_ui(q.Raw, n.Raw, d);
    }

    /// <summary>
    /// return n mod d (ceiling)
    /// </summary>
    public static GmpInteger CeilingDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint CeilingReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_cdiv_r_ui(r.Raw, n.Raw, d);
    }

    /// <summary>
    /// q = n / d + r (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint CeilingDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_cdiv_qr_ui(q.Raw, r.Raw, n.Raw, d);
    }

    /// <summary>
    /// return (n / d, n mod d) (ceiling)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (ceiling)</returns>
    public static uint CeilingReminderToUInt32(GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_cdiv_ui(n.Raw, d);
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
    public static void CeilingDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_cdiv_q_2exp(q.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n / (2 ^ d) (ceiling)
    /// </summary>
    public static GmpInteger CeilingDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        CeilingDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (ceiling)
    /// </summary>
    /// <returns>the remainder</returns>
    public static void CeilingReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_cdiv_r_2exp(r.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n mod (2 ^ d) (ceiling)
    /// </summary>
    public static GmpInteger CeilingReminder2Exp(GmpInteger n, uint exp2)
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
    public static void FloorDivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_fdiv_q(q.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n / d (Floor)
    /// </summary>
    public static GmpInteger FloorDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    public static void FloorReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_fdiv_r(r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static GmpInteger FloorReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    public static void FloorDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_fdiv_qr(q.Raw, r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint FloorDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_fdiv_q_ui(q.Raw, n.Raw, d);
    }

    /// <summary>
    /// return n mod d (Floor)
    /// </summary>
    public static GmpInteger FloorDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint FloorReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_fdiv_r_ui(r.Raw, n.Raw, d);
    }

    /// <summary>
    /// q = n / d + r (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint FloorDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_fdiv_qr_ui(q.Raw, r.Raw, n.Raw, d);
    }

    /// <summary>
    /// return (n / d, n mod d) (Floor)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Floor)</returns>
    public static uint FloorReminderToUInt32(GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_fdiv_ui(n.Raw, d);
    }

    /// <returns>n mod d (Floor)</returns>
    public static GmpInteger FloorReminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        FloorReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static void FloorDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_fdiv_q_2exp(q.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n / (2 ^ d) (Floor)
    /// </summary>
    public static GmpInteger FloorDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        FloorDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Floor)
    /// </summary>
    /// <returns>the remainder</returns>
    public static void FloorReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_fdiv_r_2exp(r.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n mod (2 ^ d) (Floor)
    /// </summary>
    public static GmpInteger FloorReminder2Exp(GmpInteger n, uint exp2)
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
    public static void DivideInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_tdiv_q(q.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n / d (Truncate)
    /// </summary>
    public static GmpInteger Divide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    public static void ReminderInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_tdiv_r(r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static GmpInteger Reminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        ReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    public static void DivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_tdiv_qr(q.Raw, r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// q = n / d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint DivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_tdiv_q_ui(q.Raw, n.Raw, d);
    }

    /// <summary>
    /// return n mod d (Truncate)
    /// </summary>
    public static GmpInteger Divide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// r = n mod d (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint ReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_tdiv_r_ui(r.Raw, n.Raw, d);
    }

    /// <summary>
    /// q = n / d + r (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static uint DivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_tdiv_qr_ui(q.Raw, r.Raw, n.Raw, d);
    }

    /// <summary>
    /// return (n / d, n mod d) (Truncate)
    /// </summary>
    public static (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <returns>n mod d (Truncate)</returns>
    public static uint ReminderToUInt32(GmpInteger n, uint d)
    {
        return GmpLib.__gmpz_tdiv_ui(n.Raw, d);
    }

    /// <returns>n mod d (Truncate)</returns>
    public static GmpInteger Reminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        ReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// q = n / (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static void Divide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_tdiv_q_2exp(q.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n / (2 ^ d) (Truncate)
    /// </summary>
    public static GmpInteger Divide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        Divide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// r = n mod (2 ^ d) (Truncate)
    /// </summary>
    /// <returns>the remainder</returns>
    public static void Reminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        GmpLib.__gmpz_tdiv_r_2exp(r.Raw, n.Raw, exp2);
    }

    /// <summary>
    /// return n mod (2 ^ d) (Truncate)
    /// </summary>
    public static GmpInteger Reminder2Exp(GmpInteger n, uint exp2)
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
    public static void ModInplace(GmpInteger r, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_mod(r.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// return n mod d. The sign of the divisor is ignored; the result is always non-negative.
    /// </summary>
    public static GmpInteger Mod(GmpInteger n, GmpInteger d)
    {
        GmpInteger r = new();
        ModInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// <para>Set r to n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static uint ModInplace(GmpInteger r, GmpInteger n, uint d) => FloorReminderInplace(r, n, d);

    /// <summary>
    /// <para>return n mod d. The sign of the divisor is ignored; the result is always non-negative.</para>
    /// <para>is identical to mpz_fdiv_r_ui above, returning the remainder as well as setting r</para>
    /// </summary>
    public static GmpInteger Mod(GmpInteger n, uint d) => FloorReminder(n, d);

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static void DivExactInplace(GmpInteger q, GmpInteger n, GmpInteger d)
    {
        GmpLib.__gmpz_divexact(q.Raw, n.Raw, d.Raw);
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static GmpInteger DivExact(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// <para>Set q to n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static void DivExactInplace(GmpInteger q, GmpInteger n, uint d)
    {
        GmpLib.__gmpz_divexact_ui(q.Raw, n.Raw, d);
    }

    /// <summary>
    /// <para>return n/d. These functions produce correct results only when it is known in advance that d divides n.</para>
    /// <para>Much faster than the other division functions, and are the best choice when exact division is known to occur, for example reducing a rational to lowest terms.</para>
    /// </summary>
    public static GmpInteger DivExact(GmpInteger n, uint d)
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
    public static bool Congruent(GmpInteger n, GmpInteger c, GmpInteger d)
    {
        return GmpLib.__gmpz_congruent_p(n.Raw, c.Raw, d.Raw) != 0;
    }

    /// <summary>
    /// <para>n is congruent to c mod d if there exists an integer q satisfying n = c + q*d.</para>
    /// <para>Unlike the other division functions, d=0 is accepted and following the rule it can be seen that n and c are considered congruent mod 0 only when exactly equal.</para>
    /// </summary>
    /// <returns>true if n = c mod d</returns>
    public static bool Congruent(GmpInteger n, uint c, uint d)
    {
        return GmpLib.__gmpz_congruent_ui_p(n.Raw, c, d) != 0;
    }

    /// <summary>
    /// <para>n is congruent to c mod (2^b) if there exists an integer q satisfying n = c + q*(2^b).</para>
    /// </summary>
    /// <returns>true if n = c mod (2^b)</returns>
    public static bool Congruent2Exp(GmpInteger n, GmpInteger c, uint b)
    {
        return GmpLib.__gmpz_congruent_2exp_p(n.Raw, c.Raw, b) != 0;
    }
    #endregion
    #endregion

    #region Exponentiation Functions
    /// <summary>
    /// r = (@base ^ exp) % mod
    /// </summary>
    public static void PowerModInplace(GmpInteger r, GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpLib.__gmpz_powm(r.Raw, @base.Raw, exp.Raw, mod.Raw);
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
    public static void PowerModInplace(GmpInteger r, GmpInteger @base, uint exp, GmpInteger mod)
    {
        GmpLib.__gmpz_powm_ui(r.Raw, @base.Raw, exp, mod.Raw);
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
    public static void PowerModSecureInplace(GmpInteger r, GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpLib.__gmpz_powm_sec(r.Raw, @base.Raw, exp.Raw, mod.Raw);
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
    public static void PowerInplace(GmpInteger r, GmpInteger @base, uint exp)
    {
        GmpLib.__gmpz_pow_ui(r.Raw, @base.Raw, exp);
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
    public static void PowerInplace(GmpInteger r, uint @base, uint exp)
    {
        GmpLib.__gmpz_ui_pow_ui(r.Raw, @base, exp);
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
    public static bool RootInplace(GmpInteger r, GmpInteger op, uint n)
    {
        return GmpLib.__gmpz_root(r.Raw, op.Raw, n) != 0;
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
    public static void RootReminderInplace(GmpInteger r, GmpInteger reminder, GmpInteger op, uint n)
    {
        GmpLib.__gmpz_rootrem(r.Raw, reminder.Raw, op.Raw, n);
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
    public bool HasPerfectPower()
    {
        return GmpLib.__gmpz_perfect_power_p(Raw) != 0;
    }

    /// <returns>
    /// <para>true if op is a perfect square, i.e., if the square root of op is an integer.</para>
    /// <para>Under this definition both 0 and 1 are considered to be perfect squares.</para>
    /// </returns>
    public bool HasPerfectSquare()
    {
        return GmpLib.__gmpz_perfect_square_p(Raw) != 0;
    }
    #endregion

    #region Number Theoretic Functions
    /// <summary>
    /// This function performs some trial divisions, a Baillie-PSW probable prime test, then reps-24 Miller-Rabin probabilistic primality tests. A higher reps value will reduce the chances of a non-prime being identified as “probably prime”. A composite number will be identified as a prime with an asymptotic probability of less than 4^(-reps).
    /// </summary>
    /// <param name="reps">Reasonable values of reps are between 15 and 50.</param>
    /// <returns></returns>
    public PrimePossibility ProbablePrime(int reps = 15)
    {
        return (PrimePossibility)GmpLib.__gmpz_probab_prime_p(Raw, reps);
    }

    /// <summary>
    /// Set rop to the next prime greater than op.
    /// </summary>
    /// <remarks>This function uses a probabilistic algorithm to identify primes. For practical purposes it’s adequate, the chance of a composite passing will be extremely small.</remarks>
    public static void NextPrimeInplace(GmpInteger rop, GmpInteger op)
    {
        GmpLib.__gmpz_nextprime(rop.Raw, op.Raw);
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

    public static void GcdInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_gcd(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger Gcd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        GcdInplace(rop, op1, op2);
        return rop;
    }

    public static void GcdInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_gcd_ui(rop.Raw, op1.Raw, op2);
    }

    public static GmpInteger Gcd(GmpInteger op1, uint op2)
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
    public static void Gcd2Inplace(GmpInteger g, GmpInteger s, GmpInteger t, GmpInteger a, GmpInteger b)
    {
        GmpLib.__gmpz_gcdext(g.Raw, s.Raw, t.Raw, a.Raw, b.Raw);
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
    public static (GmpInteger g, GmpInteger s, GmpInteger t) Gcd2(GmpInteger a, GmpInteger b)
    {
        GmpInteger g = new();
        GmpInteger s = new();
        GmpInteger t = new();
        Gcd2Inplace(g, s, t, a, b);
        return (g, s, t);
    }

    public static void LcmInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_lcm(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger Lcm(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        LcmInplace(rop, op1, op2);
        return rop;
    }

    public static void LcmInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        GmpLib.__gmpz_lcm_ui(rop.Raw, op1.Raw, op2);
    }

    public static GmpInteger Lcm(GmpInteger op1, uint op2)
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
    public static bool InvertInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        return GmpLib.__gmpz_invert(rop.Raw, op1.Raw, op2.Raw) != 0;
    }

    /// <summary>
    /// Set rop to the modular inverse of op1 mod op2, i.e. b, where op1 * b mod op2 = 1
    /// </summary>
    public static GmpInteger Invert(GmpInteger op1, GmpInteger op2)
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
    public static int Jacobi(GmpInteger a, GmpInteger b)
    {
        return GmpLib.__gmpz_jacobi(a.Raw, b.Raw);
    }

    /// <summary>
    /// <para>Calculate the Legendre symbol (a/p). </para>
    /// <para>This is defined only for p an odd positive prime, and for such p it’s identical to the Jacobi symbol.</para>
    /// </summary>
    public static int Legendre(GmpInteger a, GmpInteger p) => Jacobi(a, p);

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static int Kronecker(GmpInteger a, GmpInteger b) => Jacobi(a, b);

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static int Kronecker(GmpInteger a, int b)
    {
        return GmpLib.__gmpz_kronecker_si(a.Raw, b);
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static int Kronecker(GmpInteger a, uint b)
    {
        return GmpLib.__gmpz_kronecker_ui(a.Raw, b);
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static int Kronecker(int a, GmpInteger b)
    {
        return GmpLib.__gmpz_si_kronecker(a, b.Raw);
    }

    /// <summary>
    /// <para>Calculate the Jacobi symbol (a/b) with the Kronecker extension (a/2)=(2/a) when a odd, or (a/2)=0 when a even.</para>
    /// <para>When b is odd the Jacobi symbol and Kronecker symbol are identical, so mpz_kronecker_ui etc can be used for mixed precision Jacobi symbols too.</para>
    /// </summary>
    public static int Kronecker(uint a, GmpInteger b)
    {
        return GmpLib.__gmpz_ui_kronecker(a, b.Raw);
    }

    /// <summary>
    /// Remove all occurrences of the factor f from op and store the result in rop.
    /// </summary>
    /// <returns>The return value is how many such occurrences were removed.</returns>
    public static uint RemoveFactorInplace(GmpInteger rop, GmpInteger op, GmpInteger f)
    {
        return GmpLib.__gmpz_remove(rop.Raw, op.Raw, f.Raw);
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
    public static void FactorialInplace(GmpInteger rop, uint n)
    {
        GmpLib.__gmpz_fac_ui(rop.Raw, n);
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
    public static void Factorial2Inplace(GmpInteger rop, uint n)
    {
        GmpLib.__gmpz_2fac_ui(rop.Raw, n);
    }

    /// <summary>
    /// computes the double-factorial n!!
    /// </summary>
    public static GmpInteger Factorial2(uint n)
    {
        GmpInteger rop = new();
        Factorial2Inplace(rop, n);
        return rop;
    }

    /// <summary>
    /// computes the m-multi-factorial n!^(m)
    /// </summary>
    public static void FactorialMInplace(GmpInteger rop, uint n, uint m)
    {
        GmpLib.__gmpz_mfac_uiui(rop.Raw, n, m);
    }

    /// <summary>
    /// computes the m-multi-factorial n!^(m)
    /// </summary>
    public static GmpInteger FactorialM(uint n, uint m)
    {
        GmpInteger rop = new();
        FactorialMInplace(rop, n, m);
        return rop;
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static void BinomialCoefficientInplace(GmpInteger rop, GmpInteger n, uint k)
    {
        GmpLib.__gmpz_bin_ui(rop.Raw, n.Raw, k);
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static GmpInteger BinomialCoefficient(GmpInteger n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static void BinomialCoefficientInplace(GmpInteger rop, uint n, uint k)
    {
        GmpLib.__gmpz_bin_uiui(rop.Raw, n, k);
    }

    /// <summary>
    /// Compute the binomial coefficient n over k and store the result in rop.
    /// </summary>
    public static GmpInteger BinomialCoefficient(uint n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static void FibonacciInplace(GmpInteger fn, uint n)
    {
        GmpLib.__gmpz_fib_ui(fn.Raw, n);
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static GmpInteger Fibonacci(uint n)
    {
        GmpInteger fn = new();
        FibonacciInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static void Fibonacci2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        GmpLib.__gmpz_fib2_ui(fn.Raw, fnsub1.Raw, n);
    }

    /// <summary>
    /// 1,1,2,3,5,8,13,21,34,55
    /// </summary>
    public static (GmpInteger fn, GmpInteger fnsub1) Fibonacci2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        Fibonacci2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static void LucasNumInplace(GmpInteger fn, uint n)
    {
        GmpLib.__gmpz_lucnum_ui(fn.Raw, n);
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static GmpInteger LucasNum(uint n)
    {
        GmpInteger fn = new();
        LucasNumInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static void LucasNum2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        GmpLib.__gmpz_lucnum2_ui(fn.Raw, fnsub1.Raw, n);
    }

    /// <summary>
    /// 1,3,4,7,11,18,29,47,76,123
    /// </summary>
    public static (GmpInteger fn, GmpInteger fnsub1) LucasNum2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        LucasNum2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }
    #endregion

    #region Comparison Functions
    public static int Compare(GmpInteger op1, GmpInteger op2)
    {
        return GmpLib.__gmpz_cmp(op1.Raw, op2.Raw);
    }

    public static bool operator ==(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) == 0;
    public static bool operator !=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) != 0;
    public static bool operator >(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) > 0;
    public static bool operator <(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) < 0;
    public static bool operator >=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) >= 0;
    public static bool operator <=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) <= 0;

    public static int Compare(GmpInteger op1, double op2)
    {
        return GmpLib.__gmpz_cmp_d(op1.Raw, op2);
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

    public static int Compare(GmpInteger op1, int op2)
    {
        return GmpLib.__gmpz_cmp_si(op1.Raw, op2);
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

    public static int Compare(GmpInteger op1, uint op2)
    {
        return GmpLib.__gmpz_cmp_ui(op1.Raw, op2);
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

    public override unsafe int GetHashCode() => ((Mpz_t*)Raw)->GetHashCode();

    public static int CompareAbs(GmpInteger op1, GmpInteger op2)
    {
        return GmpLib.__gmpz_cmpabs(op1.Raw, op2.Raw);
    }

    public static int CompareAbs(GmpInteger op1, double op2)
    {
        return GmpLib.__gmpz_cmpabs_d(op1.Raw, op2);
    }

    public static int CompareAbs(GmpInteger op1, uint op2)
    {
        return GmpLib.__gmpz_cmpabs_ui(op1.Raw, op2);
    }

    public unsafe int Sign => ((Mpz_t*)Raw)->Size switch
    {
        < 0 => -1,
        0 => 0,
        > 0 => 1
    };
    #endregion

    #region Logical and Bit Manipulation Functions
    public static void BitwiseAndInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_and(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger BitwiseAnd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseAndInplace(rop, op1, op2);
        return rop;
    }

    public static GmpInteger operator &(GmpInteger op1, GmpInteger op2) => BitwiseAnd(op1, op2);

    public static void BitwiseOrInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_ior(rop.Raw, op1.Raw, op2.Raw);
    }

    public static GmpInteger BitwiseOr(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseOrInplace(rop, op1, op2);
        return rop;
    }

    public static GmpInteger operator |(GmpInteger op1, GmpInteger op2) => BitwiseOr(op1, op2);

    public static void BitwiseXorInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        GmpLib.__gmpz_xor(rop.Raw, op1.Raw, op2.Raw);
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
    public static void ComplementInplace(GmpInteger rop, GmpInteger op)
    {
        GmpLib.__gmpz_com(rop.Raw, op.Raw);
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
    public uint PopulationCount()
    {
        return GmpLib.__gmpz_popcount(Raw);
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
    public static uint HammingDistance(GmpInteger op1, GmpInteger op2)
    {
        return GmpLib.__gmpz_hamdist(op1.Raw, op2.Raw);
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
    public uint FirstIndexOf0(uint startingBit = 0)
    {
        return GmpLib.__gmpz_scan0(Raw, startingBit);
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
    public uint FirstIndexOf1(uint startingBit = 0)
    {
        return GmpLib.__gmpz_scan1(Raw, startingBit);
    }

    public void SetBit(uint bitIndex)
    {
        GmpLib.__gmpz_setbit(Raw, bitIndex);
    }

    public void ClearBit(uint bitIndex)
    {
        GmpLib.__gmpz_clrbit(Raw, bitIndex);
    }

    public void ComplementBit(uint bitIndex)
    {
        GmpLib.__gmpz_combit(Raw, bitIndex);
    }

    public int TestBit(uint bitIndex)
    {
        return GmpLib.__gmpz_tstbit(Raw, bitIndex);
    }
    #endregion

    #region Obsoleted Random
    /// <summary>
    /// Generate a random integer of at most max_size limbs.
    /// The generated random number doesn’t satisfy any particular requirements of randomness.
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static void RandomInplace(GmpInteger rop, int maxLimbCount)
    {
        GmpLib.__gmpz_random(rop.Raw, maxLimbCount);
    }

    /// <summary>
    /// Generate a random integer of at most max_size limbs.
    /// The generated random number doesn’t satisfy any particular requirements of randomness.
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static GmpInteger Random(int maxLimbCount)
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
    public static void Random2Inplace(GmpInteger rop, int maxLimbCount)
    {
        GmpLib.__gmpz_random(rop.Raw, maxLimbCount);
    }

    /// <summary>
    /// Generate a random integer of at most max_size limbs, 
    /// with long strings of zeros and ones in the binary representation. 
    /// Useful for testing functions and algorithms, 
    /// since this kind of random numbers have proven to be more likely to trigger corner-case bugs. 
    /// Negative random numbers are generated when max_size is negative.
    /// </summary>
    [Obsolete("use GmpRandom")]
    public static GmpInteger Random2(int maxLimbCount)
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

internal record struct Mpz_t
{
    public int Allocated;
    public int Size;
    /// <summary>
    /// nint*
    /// </summary>
    public IntPtr Limbs;

    public static unsafe int RawSize => Marshal.SizeOf<Mpz_t>();
    public static unsafe IntPtr Alloc() => Marshal.AllocHGlobal(sizeof(Mpz_t));

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