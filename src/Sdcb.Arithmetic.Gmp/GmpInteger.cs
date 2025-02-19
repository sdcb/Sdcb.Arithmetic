using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sdcb.Arithmetic.Gmp;

/// <summary>
/// Represents a multi-precision integer that supports arithmetic operations
/// with high performance using the GNU Multiple Precision Arithmetic Library.
/// </summary>
public class GmpInteger : IDisposable, IComparable, IComparable<GmpInteger>, IEquatable<GmpInteger>
{
    /// <summary>
    /// Gets or sets the default precision for the <see cref="GmpInteger"/> operations.
    /// The precision is expressed in bits and is used to determine the number of significant digits
    /// in the results of arithmetic operations.
    /// </summary>
    /// <value>
    /// A uint representing the default precision in bits.
    /// </value>
    public static uint DefaultPrecision
    {
        get => (uint)GmpLib.__gmpf_get_default_prec().Value;
        set => GmpLib.__gmpf_set_default_prec(new CULong(value));
    }

    internal readonly Mpz_t Raw = new();
    private readonly bool _isOwner;

    #region Initializing Integers
    /// <summary>
    /// Initializes a new instance of the <see cref="GmpInteger"/> class using the default constructor with an optional ownership parameter.
    /// </summary>
    /// <param name="isOwner">
    /// A <see cref="bool"/> value that indicates whether the current instance owns the memory for the underlying GMP integer.
    /// Default value is true.
    /// </param>
    public unsafe GmpInteger(bool isOwner = true)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_init((IntPtr)ptr);
        }
        _isOwner = isOwner;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpInteger"/> class from an existing <see cref="Mpz_t"/> structure, with an optional ownership parameter.
    /// </summary>
    /// <param name="raw">
    /// The <see cref="Mpz_t"/> structure representing the underlying GMP integer value.
    /// </param>
    /// <param name="isOwner">
    /// A <see cref="bool"/> value that indicates whether the current instance owns the memory for the underlying GMP integer.
    /// Default value is true.
    /// </param>
    public GmpInteger(Mpz_t raw, bool isOwner = true)
    {
        Raw = raw;
        _isOwner = isOwner;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GmpInteger"/> class and allocates memory for a GMP integer with the specified number of bits, with an optional ownership parameter.
    /// </summary>
    /// <param name="bitCount">
    /// A <see cref="nuint"/> value specifying the number of bits to allocate for the GMP integer.
    /// </param>
    /// <param name="isOwner">
    /// A <see cref="bool"/> value that indicates whether the current instance owns the memory for the underlying GMP integer.
    /// Default value is true.
    /// </param>
    public unsafe GmpInteger(nuint bitCount, bool isOwner = true)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_init2((IntPtr)ptr, new CULong(bitCount));
        }
        _isOwner = isOwner;
    }

    /// <summary>
    /// Reallocate the memory of the <see cref="GmpInteger"/> instance to fit the specified number of limbs.
    /// </summary>
    /// <param name="limbs">The number of limbs to allocate memory for.</param>
    /// <remarks>
    /// This method is used to resize the memory allocated for the <see cref="GmpInteger"/> instance to fit the specified number of limbs.
    /// </remarks>
    public unsafe void ReallocByLimbs(int limbs)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_realloc((IntPtr)ptr, new CLong(limbs));
        }
    }

    /// <summary>
    /// Reallocate the memory of the current <see cref="GmpInteger"/> instance to fit the specified number of bits.
    /// </summary>
    /// <param name="bits">The number of bits to allocate memory for.</param>
    /// <remarks>
    /// This method reallocates the memory of the current <see cref="GmpInteger"/> instance to fit the specified number of bits.
    /// </remarks>
    public unsafe void ReallocByBits(uint bits)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_realloc2((IntPtr)ptr, new CULong(bits));
        }
    }

    /// <summary>
    /// Reallocates the memory block of the current <see cref="GmpInteger"/> instance to fit its current size in limbs.
    /// </summary>
    public unsafe void ReallocToFit() => ReallocByLimbs(System.Math.Abs(Raw.Size));
    #endregion

    #region Assignment Functions

    /// <summary>
    /// Assigns the value of <paramref name="op"/> to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to assign the value from.</param>
    /// <remarks>
    /// This method copies the value of <paramref name="op"/> to this <see cref="GmpInteger"/> instance.
    /// </remarks>
    public unsafe void Assign(GmpInteger op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set((IntPtr)ptr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns an unsigned integer value <paramref name="op"/> to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The unsigned integer value to assign.</param>
    /// <remarks>
    /// This method sets the value of this <see cref="GmpInteger"/> instance to the specified unsigned integer value <paramref name="op"/>.
    /// </remarks>
    public unsafe void Assign(uint op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_ui((IntPtr)ptr, new CULong(op));
        }
    }

    /// <summary>
    /// Assigns the integer value <paramref name="op"/> to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The integer value to assign.</param>
    public unsafe void Assign(int op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_si((IntPtr)ptr, new CLong(op));
        }
    }

    /// <summary>
    /// Assigns the value of a double-precision floating-point number to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The double-precision floating-point number to assign.</param>
    /// <remarks>
    /// The value of <paramref name="op"/> is converted to an integer and assigned to this <see cref="GmpInteger"/> instance.
    /// </remarks>
    public unsafe void Assign(double op)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_set_d((IntPtr)ptr, op);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpRational"/> instance to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpRational"/> instance to assign from.</param>
    public unsafe void Assign(GmpRational op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpq_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set_q((IntPtr)ptr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a <see cref="GmpFloat"/> instance to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to assign from.</param>
    public unsafe void Assign(GmpFloat op)
    {
        fixed (Mpz_t* ptr = &Raw)
        fixed (Mpf_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_set_f((IntPtr)ptr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Assigns the value of a string representation of a number in the specified base to this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The string representation of the number to assign.</param>
    /// <param name="opBase">The base of the number to assign. Default is 0, which means the base is determined by the prefix of the string (e.g. "0x" for hexadecimal).</param>
    /// <exception cref="FormatException">Thrown when the string representation cannot be parsed as a valid number in the specified base.</exception>
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

    /// <summary>
    /// Swaps the values of two <see cref="GmpInteger"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> instance.</param>
    /// <remarks>
    /// This method swaps the values of <paramref name="op1"/> and <paramref name="op2"/> in place.
    /// </remarks>
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

    /// <summary>
    /// Create a new <see cref="GmpInteger"/> instance from an existing <paramref name="op"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to create a new instance from.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the same value as <paramref name="op"/>.</returns>
    public static unsafe GmpInteger From(GmpInteger op)
    {
        Mpz_t raw = new();
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_init_set((IntPtr)(&raw), (IntPtr)pop);
        }
        return new GmpInteger(raw);
    }

    /// <summary>
    /// Creates a new instance of <see cref="GmpInteger"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpInteger"/> that is a copy of this instance.</returns>
    public GmpInteger Clone() => From(this);

    /// <summary>
    /// Create a new <see cref="GmpInteger"/> instance from a <see cref="GmpFloat"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpFloat"/> instance to convert.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public static unsafe GmpInteger From(GmpFloat op)
    {
        GmpInteger rop = new();
        rop.Assign(op);
        return rop;
    }

    /// <summary>Explicitly Convert the specific <see cref="GmpFloat"/> into <see cref="GmpInteger"/>.</summary>
    public static explicit operator GmpInteger(GmpFloat op) => From(op);

    /// <summary>
    /// Create a <see cref="GmpInteger"/> instance from an unsigned integer <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The unsigned integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public static unsafe GmpInteger From(uint op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_ui((IntPtr)(&raw), new CULong(op));
        return new GmpInteger(raw);
    }

    /// <summary>Implicitly Convert the specific uint into <see cref="GmpInteger"/>.</summary>
    public static implicit operator GmpInteger(uint op) => From(op);

    /// <summary>
    /// Create a <see cref="GmpInteger"/> instance from a integer <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The integer value to convert.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public static unsafe GmpInteger From(int op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_si((IntPtr)(&raw), new CLong(op));
        return new GmpInteger(raw);
    }

    /// <summary>Implicitly Convert the specific int into <see cref="GmpInteger"/>.</summary>
    public static implicit operator GmpInteger(int op) => From(op);

    /// <summary>
    /// Create a <see cref="GmpInteger"/> instance from a double-precision floating-point number <paramref name="op"/>.
    /// </summary>
    /// <param name="op">The double-precision floating-point number to convert.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the converted value.</returns>
    public static unsafe GmpInteger From(double op)
    {
        Mpz_t raw = new();
        GmpLib.__gmpz_init_set_d((IntPtr)(&raw), op);
        return new GmpInteger(raw);
    }

    /// <summary>
    /// Explicitly Convert the specific double-precision floating-point number into <see cref="GmpInteger"/>.
    /// </summary>
    public static explicit operator GmpInteger(double op) => From(op);

    /// <summary>
    /// Parses the string representation of a number in the specified base and returns a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="val">The string representation of the number to parse.</param>
    /// <param name="valBase">The base of the number to parse. Default is 0, which means the base is determined by the prefix of <paramref name="val"/> ("0x" for hexadecimal, "0" for octal, and "0b" for binary). If <paramref name="valBase"/> is not 0 and not in the range 2 to 62, an exception is thrown.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the parsed number.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="val"/> is not in the correct format or <paramref name="valBase"/> is not in the valid range.</exception>
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
    /// Tries to parse a string <paramref name="val"/> to a <see cref="GmpInteger"/> instance with the specified base <paramref name="valBase"/>.
    /// </summary>
    /// <param name="val">The string to parse.</param>
    /// <param name="result">When this method returns, contains the <see cref="GmpInteger"/> instance equivalent to the numeric value of <paramref name="val"/>, if the conversion succeeded, or <see langword="null"/> if the conversion failed. The conversion fails if the <paramref name="val"/> parameter is <see langword="null"/>, is not a number in a valid format. This parameter is passed uninitialized.</param>
    /// <param name="valBase">The base of the number in <paramref name="val"/>, which must be 2, 8, 10, or 16. The default is 10.</param>
    /// <returns><see langword="true"/> if <paramref name="val"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="valBase"/> is not 2, 8, 10, or 16.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="val"/> is <see langword="null"/>.</exception>
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
    private bool _disposed;

    /// <summary>
    /// Clears the memory allocated for the <see cref="GmpInteger"/> instance.
    /// </summary>
    private unsafe void Clear()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_clear((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="GmpInteger"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

    /// <summary>Finalizer for <see cref="GmpInteger"/>.</summary>
    ~GmpInteger()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Arithmetic Functions

    /// <summary>
    /// Adds two <see cref="GmpInteger"/> values and stores the result in the first operand <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result of the addition.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand to add.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to add.</param>
    /// <remarks>The result of the addition is stored in the first operand <paramref name="r"/>.</remarks>
    public static unsafe void AddInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_add((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Adds two <see cref="GmpInteger"/> values and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static GmpInteger Add(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    /// <summary>Adds two <see cref="GmpInteger"/> instances (op1 + op2).</summary>
    public static GmpInteger operator +(GmpInteger a, GmpInteger b) => Add(a, b);

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to <paramref name="op1"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance to add.</param>
    /// <param name="op2">The unsigned integer value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="r"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The result is stored in <paramref name="r"/> and <paramref name="op1"/> is not modified.</remarks>
    public static unsafe void AddInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_add_ui((IntPtr)pr, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Adds an unsigned integer <paramref name="op2"/> to a <paramref name="op1"/> <see cref="GmpInteger"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to add to.</param>
    /// <param name="op2">The unsigned integer to add.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the sum of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static GmpInteger Add(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        AddInplace(r, op1, op2);
        return r;
    }

    /// <summary>Adds a <see cref="GmpInteger" /> instance and an unsigned integer (a + b).</summary>
    public static GmpInteger operator +(GmpInteger a, uint b) => Add(a, b);

    /// <summary>Adds an unsigned integer and a <see cref="GmpInteger" /> instance (a + b).</summary>
    public static GmpInteger operator +(uint a, GmpInteger b) => Add(b, a);

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance to subtract from.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> instance to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the input parameters is null.</exception>
    /// <remarks>The result is stored in <paramref name="r"/> and <paramref name="op1"/> and <paramref name="op2"/> are not modified.</remarks>
    public static unsafe void SubtractInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_sub((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Subtract two <see cref="GmpInteger"/> values and return the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the subtraction.</returns>
    public static GmpInteger Subtract(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    /// <summary>Subtracts two <see cref="GmpInteger" /> instances and returns the result as a new <see cref="GmpInteger" />.</summary>
    public static GmpInteger operator -(GmpInteger op1, GmpInteger op2) => Subtract(op1, op2);

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result of the subtraction.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> to subtract from.</param>
    /// <param name="op2">The unsigned integer to subtract.</param>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="r"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The operation is performed in place, modifying the value of <paramref name="r"/>.</remarks>
    public static unsafe void SubtractInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_sub_ui((IntPtr)pr, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Subtracts an unsigned integer <paramref name="op2"/> from a <paramref name="op1"/> <see cref="GmpInteger"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to subtract from.</param>
    /// <param name="op2">The unsigned integer value to subtract.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the subtraction.</returns>
    public static GmpInteger Subtract(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    /// <summary>Performs subtraction of a <see cref="GmpInteger"/> and an <see cref="uint"/> by invoking the Subtract method.</summary>
    public static GmpInteger operator -(GmpInteger op1, uint op2) => Subtract(op1, op2);

    /// <summary>
    /// Subtracts <paramref name="op2"/> from <paramref name="op1"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The unsigned integer value to subtract from.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to subtract.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="r"/> or <paramref name="op2"/> is null.</exception>
    /// <remarks>The result is stored in <paramref name="r"/> and <paramref name="op2"/> is not modified.</remarks>
    public static unsafe void SubtractInplace(GmpInteger r, uint op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_ui_sub((IntPtr)pr, new CULong(op1), (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Subtract <paramref name="op2"/> from <paramref name="op1"/> and return the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the subtraction.</returns>
    public static GmpInteger Subtract(uint op1, GmpInteger op2)
    {
        GmpInteger r = new();
        SubtractInplace(r, op1, op2);
        return r;
    }

    /// <summary>Subtracts a <see cref="GmpInteger"/> from a <see cref="uint"/> value, resulting in a new <see cref="GmpInteger"/>.</summary>
    public static GmpInteger operator -(uint op1, GmpInteger op2) => Subtract(op1, op2);

    /// <summary>
    /// Multiplies two <see cref="GmpInteger"/> values and stores the result in the first operand.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result of the multiplication.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand to multiply.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to multiply.</param>
    /// <remarks>The result of the multiplication is stored in <paramref name="r"/> and the original values of <paramref name="op1"/> and <paramref name="op2"/> are not modified.</remarks>
    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        fixed (Mpz_t* pop2 = &op2.Raw)
        {
            GmpLib.__gmpz_mul((IntPtr)pr, (IntPtr)pop1, (IntPtr)pop2);
        }
    }

    /// <summary>
    /// Multiplies two <see cref="GmpInteger"/> values and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the multiplication.</returns>
    /// <remarks>The original operands are not modified.</remarks>
    public static GmpInteger Multiply(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    /// <summary>Multiplies two <see cref="GmpInteger"/> instances together (op1 * op2).</summary>
    public static GmpInteger operator *(GmpInteger op1, GmpInteger op2) => Multiply(op1, op2);

    /// <summary>
    /// Multiplies an integer <paramref name="op1"/> by an integer <paramref name="op2"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance to multiply.</param>
    /// <param name="op2">The second integer to multiply.</param>
    /// <remarks>The result is stored in <paramref name="r"/> and <paramref name="op1"/> is not modified.</remarks>
    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, int op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_si((IntPtr)pr, (IntPtr)pop1, new CLong(op2));
        }
    }

    /// <summary>
    /// Multiplies a <see cref="GmpInteger"/> <paramref name="op1"/> by an integer <paramref name="op2"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to multiply.</param>
    /// <param name="op2">The integer to multiply by.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the multiplication.</returns>
    public static GmpInteger Multiply(GmpInteger op1, int op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    /// <summary>Multiplies a <see cref="GmpInteger" /> instance by an integer (op1 * op2).</summary>
    public static GmpInteger operator *(GmpInteger op1, int op2) => Multiply(op1, op2);

    /// <summary>Multiplies an integer by a <see cref="GmpInteger" /> instance (op1 * op2).</summary>
    public static GmpInteger operator *(int op1, GmpInteger op2) => Multiply(op2, op1);

    /// <summary>
    /// Multiplies an <paramref name="op1"/> <see cref="GmpInteger"/> by an unsigned integer <paramref name="op2"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result of the multiplication.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> to be multiplied.</param>
    /// <param name="op2">The unsigned integer to multiply with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="r"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The result of the multiplication is stored in <paramref name="r"/>. The value of <paramref name="op1"/> is not changed.</remarks>
    public static unsafe void MultiplyInplace(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_ui((IntPtr)pr, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Multiplies a <see cref="GmpInteger"/> <paramref name="op1"/> by an unsigned integer <paramref name="op2"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to be multiplied.</param>
    /// <param name="op2">The unsigned integer to multiply by.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the multiplication.</returns>
    public static GmpInteger Multiply(GmpInteger op1, uint op2)
    {
        GmpInteger r = new();
        MultiplyInplace(r, op1, op2);
        return r;
    }

    /// <summary>Performs multiplication of a <see cref="GmpInteger" /> instance by an unsigned integer (op1 * op2).</summary>
    public static GmpInteger operator *(GmpInteger op1, uint op2) => Multiply(op1, op2);

    /// <summary>Performs multiplication of an unsigned integer by a <see cref="GmpInteger" /> instance (op1 * op2).</summary>
    public static GmpInteger operator *(uint op1, GmpInteger op2) => Multiply(op2, op1);

    /// <summary>
    /// Adds the product of two <see cref="GmpInteger"/> values and stores the result in the first operand.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> value to store the result.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <remarks>The result of the operation is equivalent to <c>r += op1 * op2</c>.</remarks>
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
    /// Adds the product of <paramref name="op1"/> and <paramref name="op2"/> to <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to add the product to.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance to multiply.</param>
    /// <param name="op2">The second operand to multiply.</param>
    /// <remarks>The result is stored in <paramref name="r"/>.</remarks>
    public static unsafe void AddMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_addmul_ui((IntPtr)pr, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Subtracts the product of two <see cref="GmpInteger"/> values from the first operand and stores the result in the first operand.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> value to store the result.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <remarks>The result of the operation is computed as <c>r - op1 * op2</c>.</remarks>
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
    /// Subtracts the product of <paramref name="op1"/> and <paramref name="op2"/> from <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to subtract from.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> to multiply with <paramref name="op2"/> and subtract from <paramref name="r"/>.</param>
    /// <param name="op2">The unsigned integer to multiply with <paramref name="op1"/> and subtract from <paramref name="r"/>.</param>
    /// <remarks>The result is stored in <paramref name="r"/>.</remarks>
    public static unsafe void SubtractMultiply(GmpInteger r, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_submul_ui((IntPtr)pr, (IntPtr)pop1, new CULong(op2));
        }
    }

    /// <summary>
    /// Multiply the <paramref name="op1"/> by 2 raised to the power of <paramref name="exp2"/> and store the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to be multiplied.</param>
    /// <param name="exp2">The exponent of 2.</param>
    /// <remarks>The result will be stored in <paramref name="r"/> and <paramref name="op1"/> will not be modified.</remarks>
    public static unsafe void Multiply2ExpInplace(GmpInteger r, GmpInteger op1, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_mul_2exp((IntPtr)pr, (IntPtr)pop1, new CULong(exp2));
        }
    }

    /// <summary>
    /// Multiplies a <see cref="GmpInteger"/> instance <paramref name="op1"/> by 2 raised to the power of <paramref name="exp2"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to multiply.</param>
    /// <param name="exp2">The exponent of 2 to raise.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the multiplication.</returns>
    /// <remarks>The original <paramref name="op1"/> instance is not modified.</remarks>
    public static unsafe GmpInteger Multiply2Exp(GmpInteger op1, uint exp2)
    {
        GmpInteger r = new();
        Multiply2ExpInplace(r, op1, exp2);
        return r;
    }

    /// <summary>
    /// Shifts the current <see cref="GmpInteger"/> instance to the left by the specified number of bits.
    /// </summary>
    /// <param name="bits">The number of bits to shift the current instance to the left.</param>
    /// <remarks>
    /// This method multiplies the current instance by 2 raised to the power of <paramref name="bits"/>.
    /// </remarks>
    public void LeftShift(uint bits) => Multiply2ExpInplace(this, this, bits);

    /// <summary>Performs left shift of the specified <see cref="GmpInteger"/> operand by the given number of bits.</summary>
    public static GmpInteger operator <<(GmpInteger op1, uint exp2) => Multiply2Exp(op1, exp2);

    /// <summary>
    /// Negates the value of <paramref name="op1"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="r"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The value of <paramref name="op1"/> is not changed.</remarks>
    public static unsafe void NegateInplace(GmpInteger r, GmpInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_neg((IntPtr)pr, (IntPtr)pop1);
        }
    }

    /// <summary>
    /// Negates the specified <paramref name="op1"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to negate.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the negated value of <paramref name="op1"/>.</returns>
    public static GmpInteger Negate(GmpInteger op1)
    {
        GmpInteger r = new();
        NegateInplace(r, op1);
        return r;
    }

    /// <summary>Negates a <see cref="GmpInteger" /> instance (unary minus, -op1).</summary>
    public static GmpInteger operator -(GmpInteger op1) => Negate(op1);

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpInteger"/> in place.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> to compute the absolute value of.</param>
    /// <remarks>
    /// The absolute value of a number is its value without regard to its sign. For example, the absolute value of both 2 and -2 is 2.
    /// </remarks>
    public static unsafe void AbsInplace(GmpInteger r, GmpInteger op1)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop1 = &op1.Raw)
        {
            GmpLib.__gmpz_abs((IntPtr)pr, (IntPtr)pop1);
        }
    }

    /// <summary>
    /// Computes the absolute value of a <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to compute the absolute value of.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the absolute value of <paramref name="op1"/>.</returns>
    public static GmpInteger Abs(GmpInteger op1)
    {
        GmpInteger r = new();
        AbsInplace(r, op1);
        return r;
    }
    #endregion

    #region Conversion Functions

    /// <summary>
    /// Converts the current <see cref="GmpInteger"/> instance to an unsigned 32-bit integer.
    /// </summary>
    /// <returns>An unsigned 32-bit integer equivalent to the value of the current <see cref="GmpInteger"/> instance.</returns>
    public unsafe uint ToUInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (uint)GmpLib.__gmpz_get_ui((IntPtr)ptr).Value;
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpInteger"/> instance to a 32-bit signed integer.
    /// </summary>
    /// <returns>A 32-bit signed integer representation of the current <see cref="GmpInteger"/> instance.</returns>
    public unsafe int ToInt32()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (int)GmpLib.__gmpz_get_si((IntPtr)ptr).Value;
        }
    }

    /// <summary>
    /// Converts the current <see cref="GmpInteger"/> instance to a double-precision floating-point number.
    /// </summary>
    /// <returns>A double-precision floating-point number that is equivalent to the current <see cref="GmpInteger"/> instance.</returns>
    public unsafe double ToDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_get_d((IntPtr)ptr);
        }
    }

    /// <summary>
    /// Converts a <see cref="GmpInteger"/> instance to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator uint(GmpInteger op) => op.ToUInt32();

    /// <summary>
    /// Converts a <see cref="GmpInteger"/> instance to a signed 32-bit integer.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator int(GmpInteger op) => op.ToInt32();

    /// <summary>
    /// Converts a <see cref="GmpInteger"/> instance to a double-precision floating-point number.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to convert.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator double(GmpInteger op) => op.ToDouble();

    /// <summary>
    /// Converts the current <see cref="GmpInteger"/> instance to an <see cref="ExpDouble"/> instance.
    /// </summary>
    /// <returns>An <see cref="ExpDouble"/> instance representing the converted value.</returns>
    /// <remarks>
    /// The conversion is done by extracting the double value and exponent from the current <see cref="GmpInteger"/> instance.
    /// </remarks>
    public unsafe ExpDouble ToExpDouble()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            int exp = default;
            double val = GmpLib.__gmpz_get_d_2exp((IntPtr)(&exp), (IntPtr)ptr);
            return new ExpDouble(exp, val);
        }
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="GmpFloat"/> object using base 10.
    /// </summary>
    /// <returns>A string representation of the current <see cref="GmpFloat"/> object using base 10.</returns>
    public override string ToString() => ToString(10);

    /// <summary>
    /// Converts the current <see cref="GmpInteger"/> to a string representation in the specified base.
    /// </summary>
    /// <param name="opBase">The base of the output string, which must be between 2 and 62.</param>
    /// <returns>A string representation of the current <see cref="GmpInteger"/> in the specified base.</returns>
    /// <exception cref="ArgumentException">Thrown when unable to convert <see cref="GmpInteger"/> to string.</exception>
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
    /// Calculates the ceiling division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> instance to store the result of the ceiling division.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> instance to divide by.</param>
    /// <remarks>
    /// The ceiling division of two integers is the smallest integer that is greater than or equal to the exact division of the two integers.
    /// </remarks>
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
    /// Calculates the ceiling division of two <see cref="GmpInteger"/>s and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The numerator.</param>
    /// <param name="d">The denominator.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the ceiling division of <paramref name="n"/> and <paramref name="d"/>.</returns>
    public static unsafe GmpInteger CeilingDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Calculates the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// The result is rounded towards positive infinity.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <remarks>
    /// This method modifies the value of <paramref name="r"/> in place.
    /// </remarks>
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
    /// Computes the ceiling of the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the ceiling of the remainder of the division of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger CeilingReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        CeilingReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Computes the ceiling division of <paramref name="n"/> by <paramref name="d"/> and stores the quotient in <paramref name="q"/> and the remainder in <paramref name="r"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the quotient.</param>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
    /// <remarks>
    /// The ceiling division of two integers <c>n</c> and <c>d</c> is defined as the smallest integer <c>q</c> such that <c>n &lt;= q*d</c> and the remainder <c>r</c> is non-negative. 
    /// </remarks>
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
    /// Computes the quotient and remainder of the division of <paramref name="n"/> by <paramref name="d"/>,
    /// where the quotient is rounded towards positive infinity.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A tuple containing the quotient and remainder of the division.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Divides <paramref name="n"/> by <paramref name="d"/> and rounds up to the nearest integer, storing the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint CeilingDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_cdiv_q_ui((IntPtr)pq, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Computes the ceiling division of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the ceiling division of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger CeilingDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        CeilingDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Calculates the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// If the remainder is non-zero, it is rounded up to the nearest multiple of <paramref name="d"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division of <paramref name="n"/> by <paramref name="d"/> rounded up to the nearest multiple of <paramref name="d"/>.</returns>
    public static unsafe uint CeilingReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_cdiv_r_ui((IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <paramref name="n"/> by an unsigned integer <paramref name="d"/> and returns the quotient and remainder as out parameters <paramref name="q"/> and <paramref name="r"/> respectively, rounded towards positive infinity.
    /// </summary>
    /// <param name="q">The quotient of the division.</param>
    /// <param name="r">The remainder of the division.</param>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint CeilingDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_cdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Calculates the quotient and remainder of the division of a <paramref name="n"/> by a positive integer <paramref name="d"/>,
    /// where the quotient is rounded towards positive infinity.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The positive integer divisor.</param>
    /// <returns>A tuple of two <see cref="GmpInteger"/> values representing the quotient and remainder of the division.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) CeilingDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        CeilingDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Calculates the ceiling of the division of <paramref name="n"/> by <paramref name="d"/> and returns the remainder as an unsigned 32-bit integer.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division as an unsigned 32-bit integer.</returns>
    public static unsafe nuint CeilingReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_cdiv_ui((IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Computes the ceiling of the remainder of <paramref name="n"/> divided by <paramref name="d"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the ceiling of the remainder of <paramref name="n"/> divided by <paramref name="d"/>.</returns>
    public static GmpInteger CeilingReminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        CeilingReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// Divides <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/> and rounds up to the nearest integer, and stores the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="exp2">The power of 2 to raise and divide <paramref name="n"/>.</param>
    /// <remarks>
    /// This method modifies <paramref name="q"/> in place.
    /// </remarks>
    public static unsafe void CeilingDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_cdiv_q_2exp((IntPtr)pq, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Computes the ceiling division of a <see cref="GmpInteger"/> by 2 raised to the power of <paramref name="exp2"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to divide.</param>
    /// <param name="exp2">The power of 2 to raise.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the ceiling division result.</returns>
    /// <remarks>
    /// The ceiling division of <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/> is defined as the smallest integer <c>q</c> such that <c>n &lt;= q * 2^<paramref name="exp2"/></c>.
    /// </remarks>
    public static unsafe GmpInteger CeilingDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        CeilingDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// Computes the remainder of <paramref name="n"/> divided by 2 to the power of <paramref name="exp2"/> and stores the result in <paramref name="r"/>.
    /// The result is rounded towards positive infinity.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to compute the remainder of.</param>
    /// <param name="exp2">The power of 2 to divide <paramref name="n"/> by.</param>
    /// <remarks>
    /// This method modifies <paramref name="r"/> in place.
    /// </remarks>
    public static unsafe void CeilingReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_cdiv_r_2exp((IntPtr)pr, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Calculates the ceiling of the remainder of <paramref name="n"/> divided by 2 raised to the power of <paramref name="exp2"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="exp2">The power of 2 divisor.</param>
    /// <returns>The ceiling of the remainder of <paramref name="n"/> divided by 2 raised to the power of <paramref name="exp2"/>.</returns>
    public static unsafe GmpInteger CeilingReminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        CeilingReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Floor

    /// <summary>
    /// Divides the numerator <paramref name="n"/> by the denominator <paramref name="d"/> and stores the result in <paramref name="q"/>.
    /// The result is rounded towards zero (truncated).
    /// </summary>
    /// <param name="q">The quotient of the division.</param>
    /// <param name="n">The numerator of the division.</param>
    /// <param name="d">The denominator of the division.</param>
    /// <remarks>
    /// This method modifies the value of <paramref name="q"/> in place.
    /// </remarks>
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
    /// Performs floor division of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The numerator <see cref="GmpInteger"/> instance.</param>
    /// <param name="d">The denominator <see cref="GmpInteger"/> instance.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the floor division result of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger FloorDivide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Computes the floor reminder of <paramref name="n"/> divided by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The dividend <see cref="GmpInteger"/>.</param>
    /// <param name="d">The divisor <see cref="GmpInteger"/>.</param>
    /// <remarks>
    /// The floor reminder is the difference between the dividend and the product of the divisor and the quotient rounded towards zero.
    /// </remarks>
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
    /// Computes the floor of the quotient and the remainder of the division of <paramref name="n"/> by <paramref name="d"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the floor of the quotient of <paramref name="n"/> divided by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger FloorReminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        FloorReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Computes the quotient and remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the results in <paramref name="q"/> and <paramref name="r"/> respectively.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the quotient of the division.</param>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder of the division.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <remarks>
    /// This method modifies the values of <paramref name="q"/> and <paramref name="r"/> in place.
    /// </remarks>
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
    /// Computes the quotient and remainder of the division of <paramref name="n"/> by <paramref name="d"/> and returns them as a tuple.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A tuple containing the quotient and remainder of the division.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Divides <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint FloorDivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_fdiv_q_ui((IntPtr)pq, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Performs floor division of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the floor division result.</returns>
    public static unsafe GmpInteger FloorDivide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        FloorDivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Calculates the floor remainder of <paramref name="n"/> divided by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The floor remainder of <paramref name="n"/> divided by <paramref name="d"/>.</returns>
    public static unsafe uint FloorReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_fdiv_r_ui((IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <paramref name="n"/> by an unsigned integer <paramref name="d"/> and returns the quotient and remainder as out parameters <paramref name="q"/> and <paramref name="r"/> respectively.
    /// </summary>
    /// <param name="q">The quotient of the division.</param>
    /// <param name="r">The remainder of the division.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer divisor.</param>
    /// <returns>The unsigned integer remainder of the division.</returns>
    public static unsafe uint FloorDivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_fdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <see cref="GmpInteger"/> by an unsigned integer <paramref name="d"/> and returns the quotient and remainder as a tuple.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer divisor.</param>
    /// <returns>A tuple containing the quotient and remainder as <see cref="GmpInteger"/> instances.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) FloorDivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        FloorDivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Calculates the floor division remainder of <paramref name="n"/> by <paramref name="d"/> and returns the result as an unsigned 32-bit integer.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to calculate the remainder of.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The floor division remainder of <paramref name="n"/> by <paramref name="d"/> as an unsigned 32-bit integer.</returns>
    public static unsafe uint FloorReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_fdiv_ui((IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Computes the floor division remainder of <paramref name="n"/> by <paramref name="d"/> and stores the result in a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the floor division remainder of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger FloorReminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        FloorReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// Divides <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/> and stores the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="exp2">The power of 2 to divide by.</param>
    /// <remarks>The operation is performed in place, modifying the value of <paramref name="q"/>.</remarks>
    public static unsafe void FloorDivide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_fdiv_q_2exp((IntPtr)pq, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Divide a <see cref="GmpInteger"/> by 2 raised to the power of <paramref name="exp2"/> and return the floor result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="exp2">The power of 2 to divide by.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the floor result of the division.</returns>
    public static unsafe GmpInteger FloorDivide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        FloorDivide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// Computes the floor remainder of <paramref name="n"/> divided by 2 to the power of <paramref name="exp2"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to compute the remainder of.</param>
    /// <param name="exp2">The power of 2 to divide <paramref name="n"/> by.</param>
    /// <remarks>
    /// This method modifies the value of <paramref name="r"/> in place.
    /// </remarks>
    public static unsafe void FloorReminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_fdiv_r_2exp((IntPtr)pr, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Computes the floor division of <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/> and stores the remainder in returned value.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="exp2">The power of 2 to raise.</param>
    /// <returns>The floor division of <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/>.</returns>
    public static unsafe GmpInteger FloorReminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        FloorReminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region Truncate

    /// <summary>
    /// Divides the numerator <paramref name="n"/> by the denominator <paramref name="d"/> and stores the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The quotient of the division.</param>
    /// <param name="n">The numerator of the division.</param>
    /// <param name="d">The denominator of the division.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
    /// <remarks>The value of <paramref name="q"/> will be modified to store the result of the division.</remarks>
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
    /// Divides <paramref name="n"/> by <paramref name="d"/> and returns the quotient as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the quotient of the division.</returns>
    public static unsafe GmpInteger Divide(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> instance to divide by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="r"/>, <paramref name="n"/>, or <paramref name="d"/> is null.</exception>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
    /// <remarks>The value of <paramref name="r"/> will be modified to the remainder of the division of <paramref name="n"/> by <paramref name="d"/>.</remarks>
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
    /// Computes the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and returns a new <see cref="GmpInteger"/> instance representing the result.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the remainder of the division.</returns>
    public static unsafe GmpInteger Reminder(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        ReminderInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Divides the <paramref name="n"/> by <paramref name="d"/> and stores the quotient in <paramref name="q"/> and the remainder in <paramref name="r"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the quotient.</param>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
    /// <remarks>
    /// This method modifies the values of <paramref name="q"/> and <paramref name="r"/> in place.
    /// </remarks>
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
    /// Divides two <see cref="GmpInteger"/> values and returns the quotient and remainder as a tuple.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <returns>A tuple containing the quotient and remainder of the division.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Divides a <paramref name="n"/> by an unsigned integer <paramref name="d"/> and stores the quotient in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the quotient.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint DivideInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_tdiv_q_ui((IntPtr)pq, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <see cref="GmpInteger"/> <paramref name="n"/> by an unsigned integer <paramref name="d"/> and returns the quotient as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer to divide by.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the quotient of the division.</returns>
    public static unsafe GmpInteger Divide(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        DivideInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Calculates the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint ReminderInplace(GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_tdiv_r_ui((IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <paramref name="n"/> by an unsigned integer <paramref name="d"/> and stores the quotient in <paramref name="q"/> and the remainder in <paramref name="r"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the quotient.</param>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer divisor.</param>
    /// <returns>The remainder of the division.</returns>
    public static unsafe uint DivRemInplace(GmpInteger q, GmpInteger r, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_tdiv_qr_ui((IntPtr)pq, (IntPtr)pr, (IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Divides a <see cref="GmpInteger"/> by a <paramref name="d"/> and returns the quotient and remainder as a tuple.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A tuple containing the quotient and remainder as <see cref="GmpInteger"/> instances.</returns>
    public static unsafe (GmpInteger q, GmpInteger r) DivRem(GmpInteger n, uint d)
    {
        GmpInteger q = new(), r = new();
        DivRemInplace(q, r, n, d);
        return (q, r);
    }

    /// <summary>
    /// Computes the remainder of the division of a <see cref="GmpInteger"/> by a <see cref="uint"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="uint"/> divisor.</param>
    /// <returns>The remainder of the division of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe uint ReminderToUInt32(GmpInteger n, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return (uint)GmpLib.__gmpz_tdiv_ui((IntPtr)pn, new CULong(d)).Value;
        }
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the remainder of the division.</returns>
    public static unsafe GmpInteger Reminder(GmpInteger n, uint d)
    {
        GmpInteger r = new();
        ReminderInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// Divide a <paramref name="n"/> by 2 raised to the power of <paramref name="exp2"/> and store the result in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="exp2">The power of 2 to raise.</param>
    /// <remarks>The result is stored in <paramref name="q"/> and <paramref name="n"/> is not modified.</remarks>
    public static unsafe void Divide2ExpInplace(GmpInteger q, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_tdiv_q_2exp((IntPtr)pq, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Divides a <see cref="GmpInteger"/> by 2 raised to the power of <paramref name="exp2"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to divide.</param>
    /// <param name="exp2">The power of 2 to raise to.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the division.</returns>
    /// <remarks>The original <paramref name="n"/> is not modified.</remarks>
    public static unsafe GmpInteger Divide2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        Divide2ExpInplace(q, n, exp2);
        return q;
    }

    /// <summary>
    /// Calculates the remainder of <paramref name="n"/> divided by 2 to the power of <paramref name="exp2"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the remainder.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to be divided.</param>
    /// <param name="exp2">The power of 2 to divide <paramref name="n"/> by.</param>
    /// <remarks>
    /// This method modifies the value of <paramref name="r"/> in place.
    /// </remarks>
    public static unsafe void Reminder2ExpInplace(GmpInteger r, GmpInteger n, uint exp2)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_tdiv_r_2exp((IntPtr)pr, (IntPtr)pn, new CULong(exp2));
        }
    }

    /// <summary>
    /// Computes the remainder of <paramref name="n"/> divided by 2 raised to the power of <paramref name="exp2"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to compute the remainder of.</param>
    /// <param name="exp2">The power of 2 to divide <paramref name="n"/> by.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the remainder of <paramref name="n"/> divided by 2 raised to the power of <paramref name="exp2"/>.</returns>
    public static unsafe GmpInteger Reminder2Exp(GmpInteger n, uint exp2)
    {
        GmpInteger q = new();
        Reminder2ExpInplace(q, n, exp2);
        return q;
    }
    #endregion

    #region operators
    /// <summary>Divides two <see cref="GmpInteger" /> instances, returning the quotient (op1 / op2).</summary>
    public static GmpInteger operator /(GmpInteger op1, GmpInteger op2) => Divide(op1, op2);

    /// <summary>Calculates the remainder when two <see cref="GmpInteger" /> instances are divided (op1 % op2).</summary>
    public static GmpInteger operator %(GmpInteger op1, GmpInteger op2) => Reminder(op1, op2);

    /// <summary>Divides a <see cref="GmpInteger" /> instance by an unsigned integer (op1 / op2).</summary>
    public static GmpInteger operator /(GmpInteger op1, uint op2) => Divide(op1, op2);

    /// <summary>Calculates the remainder when a <see cref="GmpInteger" /> instance is divided by an unsigned integer (op1 % op2).</summary>
    public static GmpInteger operator %(GmpInteger op1, uint op2) => Reminder(op1, op2);
    #endregion

    #region Others

    /// <summary>
    /// Computes the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the remainder of the division.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="r"/>, <paramref name="n"/> or <paramref name="d"/> is null.</exception>
    /// <remarks>The value of <paramref name="r"/> will be the smallest non-negative integer congruent to <paramref name="n"/> modulo <paramref name="d"/>. The sign of <paramref name="r"/> will be the same as the sign of <paramref name="n"/>. </remarks>
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
    /// Computes the modulo of <paramref name="n"/> by <paramref name="d"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the modulo operation.</returns>
    public static unsafe GmpInteger Mod(GmpInteger n, GmpInteger d)
    {
        GmpInteger r = new();
        ModInplace(r, n, d);
        return r;
    }

    /// <summary>
    /// Computes the remainder of the division of <paramref name="n"/> by <paramref name="d"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance to compute the remainder of.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>The remainder of the division of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe uint ModInplace(GmpInteger r, GmpInteger n, uint d) => FloorReminderInplace(r, n, d);

    /// <summary>
    /// Computes the modulo of a <see cref="GmpInteger"/> instance <paramref name="n"/> by a positive integer <paramref name="d"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> instance to compute the modulo of.</param>
    /// <param name="d">The positive integer to compute the modulo with.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the modulo of <paramref name="n"/> by <paramref name="d"/>.</returns>
    public static unsafe GmpInteger Mod(GmpInteger n, uint d) => FloorReminder(n, d);

    /// <summary>
    /// Divides <paramref name="n"/> by <paramref name="d"/> and stores the exact quotient in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the exact quotient.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The <see cref="GmpInteger"/> to divide by.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
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
    /// Divides <paramref name="n"/> by <paramref name="d"/> and returns the quotient as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The dividend.</param>
    /// <param name="d">The divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the quotient of the division.</returns>
    public static unsafe GmpInteger DivExact(GmpInteger n, GmpInteger d)
    {
        GmpInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Divides <paramref name="n"/> by <paramref name="d"/> and stores the exact quotient in <paramref name="q"/>.
    /// </summary>
    /// <param name="q">The <see cref="GmpInteger"/> to store the exact quotient.</param>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The divisor.</param>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="d"/> is zero.</exception>
    /// <remarks>
    /// This method computes the quotient <paramref name="q"/> such that <paramref name="n"/> = <paramref name="q"/> * <paramref name="d"/>.
    /// If <paramref name="d"/> does not divide <paramref name="n"/> exactly, an exception is thrown.
    /// </remarks>
    public static unsafe void DivExactInplace(GmpInteger q, GmpInteger n, uint d)
    {
        fixed (Mpz_t* pq = &q.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_divexact_ui((IntPtr)pq, (IntPtr)pn, new CULong(d));
        }
    }

    /// <summary>
    /// Divides a <see cref="GmpInteger"/> <paramref name="n"/> by an unsigned integer <paramref name="d"/> and returns the quotient as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to be divided.</param>
    /// <param name="d">The unsigned integer divisor.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the quotient of the division.</returns>
    public static unsafe GmpInteger DivExact(GmpInteger n, uint d)
    {
        GmpInteger q = new();
        DivExactInplace(q, n, d);
        return q;
    }

    /// <summary>
    /// Determines whether the integer <paramref name="n"/> is congruent to <paramref name="c"/> modulo <paramref name="d"/>.
    /// </summary>
    /// <param name="n">The integer to check for congruence.</param>
    /// <param name="c">The integer to compare with.</param>
    /// <param name="d">The modulus.</param>
    /// <returns><c>true</c> if <paramref name="n"/> is congruent to <paramref name="c"/> modulo <paramref name="d"/>; otherwise, <c>false</c>.</returns>
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
    /// Determines whether the <paramref name="n"/> is congruent to <paramref name="c"/> modulo <paramref name="d"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to check for congruence.</param>
    /// <param name="c">The integer to compare with.</param>
    /// <param name="d">The modulus to compare with.</param>
    /// <returns><c>true</c> if <paramref name="n"/> is congruent to <paramref name="c"/> modulo <paramref name="d"/>; otherwise, <c>false</c>.</returns>
    public static unsafe bool Congruent(GmpInteger n, uint c, uint d)
    {
        fixed (Mpz_t* pn = &n.Raw)
        {
            return GmpLib.__gmpz_congruent_ui_p((IntPtr)pn, new CULong(c), new CULong(d)) != 0;
        }
    }

    /// <summary>
    /// Determines whether the <paramref name="n"/> is congruent to <paramref name="c"/> modulo 2 to the power of <paramref name="b"/>.
    /// </summary>
    /// <param name="n">The <see cref="GmpInteger"/> to check for congruence.</param>
    /// <param name="c">The <see cref="GmpInteger"/> to compare with.</param>
    /// <param name="b">The power of 2 to use as the modulus.</param>
    /// <returns><c>true</c> if <paramref name="n"/> is congruent to <paramref name="c"/> modulo 2 to the power of <paramref name="b"/>; otherwise, <c>false</c>.</returns>
    public static unsafe bool Congruent2Exp(GmpInteger n, GmpInteger c, uint b)
    {
        fixed (Mpz_t* pn = &n.Raw)
        fixed (Mpz_t* pc = &c.Raw)
        {
            return GmpLib.__gmpz_congruent_2exp_p((IntPtr)pn, (IntPtr)pc, new CULong(b)) != 0;
        }
    }
    #endregion
    #endregion

    #region Exponentiation Functions

    /// <summary>
    /// Computes the modular exponentiation of a base integer <paramref name="base"/> raised to the power of an exponent integer <paramref name="exp"/> modulo a modulus integer <paramref name="mod"/> and stores the result in the provided <paramref name="r"/> integer.
    /// </summary>
    /// <param name="r">The integer to store the result of the modular exponentiation.</param>
    /// <param name="base">The base integer.</param>
    /// <param name="exp">The exponent integer.</param>
    /// <param name="mod">The modulus integer.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the input integers is null.</exception>
    /// <remarks>
    /// This method modifies the value of <paramref name="r"/> in place.
    /// </remarks>
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

    /// <summary>
    /// Computes the modular exponentiation of a <paramref name="base"/> raised to the power of an <paramref name="exp"/> modulo a <paramref name="mod"/>.
    /// </summary>
    /// <param name="base">The base integer.</param>
    /// <param name="exp">The exponent integer.</param>
    /// <param name="mod">The modulo integer.</param>
    /// <returns>The result of the modular exponentiation.</returns>
    public static GmpInteger PowerMod(GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// Computes the modular exponentiation of a <paramref name="base"/> raised to the power of an <paramref name="exp"/> and stores the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="base">The <see cref="GmpInteger"/> instance representing the base.</param>
    /// <param name="exp">The exponent to raise the <paramref name="base"/> to.</param>
    /// <param name="mod">The <see cref="GmpInteger"/> instance representing the modulus.</param>
    /// <remarks>
    /// This method computes the modular exponentiation of a <paramref name="base"/> raised to the power of an <paramref name="exp"/> and stores the result in <paramref name="r"/>.
    /// The result is computed as <c>r = base^exp mod mod</c>.
    /// </remarks>
    public static unsafe void PowerModInplace(GmpInteger r, GmpInteger @base, uint exp, GmpInteger mod)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        fixed (Mpz_t* pmod = &mod.Raw)
        {
            GmpLib.__gmpz_powm_ui((IntPtr)pr, (IntPtr)pbase, new CULong(exp), (IntPtr)pmod);
        }
    }

    /// <summary>
    /// Computes the modular exponentiation of a <paramref name="base"/> raised to the power of an unsigned integer <paramref name="exp"/> modulo a <paramref name="mod"/>.
    /// </summary>
    /// <param name="base">The base <see cref="GmpInteger"/> to raise to the power of <paramref name="exp"/>.</param>
    /// <param name="exp">The unsigned integer exponent to raise the <paramref name="base"/> to.</param>
    /// <param name="mod">The modulus <see cref="GmpInteger"/> to apply to the result.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the modular exponentiation.</returns>
    public static GmpInteger PowerMod(GmpInteger @base, uint exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// Computes the modular exponentiation of a base integer <paramref name="base"/> raised to the power of an exponent integer <paramref name="exp"/> modulo a modulus integer <paramref name="mod"/>.
    /// </summary>
    /// <param name="r">The result of the modular exponentiation.</param>
    /// <param name="base">The base integer.</param>
    /// <param name="exp">The exponent integer.</param>
    /// <param name="mod">The modulus integer.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the input parameters is null.</exception>
    /// <remarks>
    /// This method uses the secure variant of the modular exponentiation algorithm, which is resistant to some side-channel attacks.
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

    /// <summary>
    /// Computes the modular exponentiation of a base integer <paramref name="base"/> raised to the power of an exponent integer <paramref name="exp"/> with respect to a modulus integer <paramref name="mod"/>.
    /// </summary>
    /// <param name="base">The base integer.</param>
    /// <param name="exp">The exponent integer.</param>
    /// <param name="mod">The modulus integer.</param>
    /// <returns>The result of modular exponentiation.</returns>
    public static GmpInteger PowerModSecure(GmpInteger @base, GmpInteger exp, GmpInteger mod)
    {
        GmpInteger r = new();
        PowerModSecureInplace(r, @base, exp, mod);
        return r;
    }

    /// <summary>
    /// Computes the power of a <paramref name="base"/> to the specified <paramref name="exp"/> and stores the result in the <paramref name="r"/> parameter.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> to store the result.</param>
    /// <param name="base">The <see cref="GmpInteger"/> base.</param>
    /// <param name="exp">The exponent to raise the <paramref name="base"/> to.</param>
    /// <remarks>The result is stored in the <paramref name="r"/> parameter.</remarks>
    public static unsafe void PowerInplace(GmpInteger r, GmpInteger @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pbase = &@base.Raw)
        {
            GmpLib.__gmpz_pow_ui((IntPtr)pr, (IntPtr)pbase, new CULong(exp));
        }
    }

    /// <summary>
    /// Computes the power of a <paramref name="base"/> to the specified <paramref name="exp"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="base">The base value.</param>
    /// <param name="exp">The exponent value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the power operation.</returns>
    public static GmpInteger Power(GmpInteger @base, uint exp)
    {
        GmpInteger r = new();
        PowerInplace(r, @base, exp);
        return r;
    }

    /// <summary>
    /// Raises an unsigned integer <paramref name="base"/> to the power of another unsigned integer <paramref name="exp"/> and stores the result in the <paramref name="r"/> instance.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="base">The base value to raise.</param>
    /// <param name="exp">The exponent value to raise.</param>
    /// <remarks>The result is stored in the <paramref name="r"/> instance.</remarks>
    public static unsafe void PowerInplace(GmpInteger r, uint @base, uint exp)
    {
        fixed (Mpz_t* pr = &r.Raw)
        {
            GmpLib.__gmpz_ui_pow_ui((IntPtr)pr, new CULong(@base), new CULong(exp));
        }
    }

    /// <summary>
    /// Computes the power of a given unsigned integer <paramref name="base"/> raised to a given unsigned integer <paramref name="exp"/>.
    /// </summary>
    /// <param name="base">The base value.</param>
    /// <param name="exp">The exponent value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the power operation.</returns>
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
    /// Compute the <paramref name="n"/>-th root of <paramref name="op"/> and store the result in <paramref name="r"/>.
    /// </summary>
    /// <param name="r">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpInteger"/> instance to compute the root.</param>
    /// <param name="n">The root degree.</param>
    /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
    public static unsafe bool RootInplace(GmpInteger r, GmpInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            return GmpLib.__gmpz_root((IntPtr)pr, (IntPtr)pop,  new CULong(n)) != 0;
        }
    }

    /// <summary>
    /// Computes the <paramref name="n"/>th root of a <see cref="GmpInteger"/> <paramref name="op"/> and returns a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> to compute the root of.</param>
    /// <param name="n">The root degree.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the computed root.</returns>
    public static GmpInteger Root(GmpInteger op, uint n)
    {
        GmpInteger r = new();
        RootInplace(r, op, n);
        return r;
    }

    /// <summary>
    /// Calculates the integer n-th root of <paramref name="op"/> and its remainder, and stores the results in <paramref name="r"/> and <paramref name="reminder"/> respectively.
    /// </summary>
    /// <param name="r">The output integer n-th root.</param>
    /// <param name="reminder">The output remainder.</param>
    /// <param name="op">The input integer to calculate the n-th root.</param>
    /// <param name="n">The root degree.</param>
    /// <remarks>
    /// This method calculates the integer n-th root of <paramref name="op"/> and its remainder, and stores the results in <paramref name="r"/> and <paramref name="reminder"/> respectively.
    /// </remarks>
    public static unsafe void RootReminderInplace(GmpInteger r, GmpInteger reminder, GmpInteger op, uint n)
    {
        fixed (Mpz_t* pr = &r.Raw)
        fixed (Mpz_t* preminder = &reminder.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_rootrem((IntPtr)pr, (IntPtr)preminder, (IntPtr)pop, new CULong(n));
        }
    }

    /// <summary>
    /// Computes the integer square root and remainder of <paramref name="op"/> divided by <paramref name="n"/>.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> to compute the square root and remainder of.</param>
    /// <param name="n">The divisor.</param>
    /// <returns>A tuple containing the integer square root and remainder of <paramref name="op"/> divided by <paramref name="n"/>.</returns>
    public static (GmpInteger root, GmpInteger reminder) RootReminder(GmpInteger op, uint n)
    {
        GmpInteger root = new(), reminder = new();
        RootReminderInplace(root, reminder, op, n);
        return (root, reminder);
    }

    /// <summary>
    /// Determines whether this <see cref="GmpInteger"/> is a perfect power of some integer.
    /// </summary>
    /// <returns><c>true</c> if this <see cref="GmpInteger"/> is a perfect power of some integer; otherwise, <c>false</c>.</returns>
    public unsafe bool HasPerfectPower()
    {
        fixed (Mpz_t* pop = &Raw)
        {
            return GmpLib.__gmpz_perfect_power_p((IntPtr)pop) != 0;
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="GmpInteger"/> instance is a perfect square.
    /// </summary>
    /// <returns><c>true</c> if the current instance is a perfect square; otherwise, <c>false</c>.</returns>
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
    /// Determines whether the current <see cref="GmpInteger"/> instance is a probable prime number with a certain number of Miller-Rabin tests.
    /// </summary>
    /// <param name="reps">The number of Miller-Rabin tests to perform. Default is 15.</param>
    /// <returns>A <see cref="PrimePossibility"/> value indicating whether the current instance is a probable prime number.</returns>
    public unsafe PrimePossibility ProbablePrime(int reps = 15)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (PrimePossibility)GmpLib.__gmpz_probab_prime_p((IntPtr)ptr, reps);
        }
    }

    /// <summary>
    /// Finds the next prime number greater than <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="GmpInteger"/> instance to find the next prime number greater than.</param>
    /// <remarks>The value of <paramref name="rop"/> will be modified to store the result.</remarks>
    public static unsafe void NextPrimeInplace(GmpInteger rop, GmpInteger op)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        {
            GmpLib.__gmpz_nextprime((IntPtr)pr, (IntPtr)pop);
        }
    }

    /// <summary>
    /// Computes the next prime number greater than the specified <paramref name="op"/> and returns a new instance of <see cref="GmpInteger"/> representing the result.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> to find the next prime number greater than.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the next prime number greater than <paramref name="op"/>.</returns>
    public static GmpInteger NextPrime(GmpInteger op)
    {
        GmpInteger r = new();
        NextPrimeInplace(r, op);
        return r;
    }

    /// <summary>
    /// Finds the next prime number greater than the current <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the next prime number greater than the current instance.</returns>
    public GmpInteger NextPrime()
    {
        GmpInteger r = new();
        NextPrimeInplace(r, this);
        return r;
    }

    /// <summary>
    /// Computes the greatest common divisor of two <see cref="GmpInteger"/>s <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compute the GCD with.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compute the GCD with.</param>
    /// <remarks>The result is stored in <paramref name="rop"/> and both <paramref name="op1"/> and <paramref name="op2"/> are unchanged.</remarks>
    public static unsafe void GcdInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_gcd((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Computes the greatest common divisor (GCD) of two <see cref="GmpInteger"/> values <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the GCD of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpInteger Gcd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        GcdInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Compute the greatest common divisor of op1 and op2. 
    /// </summary>
    /// <param name="rop">Parameter to store the result</param>
    /// <param name="op1">First parameter to compute gcd against</param>
    /// <param name="op2">Second parameter to compute gcd against</param>
    /// <returns>
    /// <para>If the result is small enough to fit in an unsigned, it is returned. </para>
    /// <para>If the result does not fit, 0 is returned, and the result is equal to the argument op1. </para>
    /// <para>Note that the result will always fit if op2 is non-zero</para>
    /// </returns>
    public static unsafe uint GcdInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return (uint)GmpLib.__gmpz_gcd_ui((IntPtr)pr, (IntPtr)p1, new CULong(op2)).Value;
        }
    }

    /// <summary>
    /// Computes the greatest common divisor (GCD) of a <see cref="GmpInteger"/> <paramref name="op1"/> and an unsigned integer <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the GCD of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpInteger Gcd(GmpInteger op1, uint op2)
    {
        GmpInteger rop = new();
        GcdInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Computes the greatest common divisor (GCD) of two integers <paramref name="a"/> and <paramref name="b"/> and the Bezout coefficients <paramref name="s"/> and <paramref name="t"/> such that <paramref name="s"/> * <paramref name="a"/> + <paramref name="t"/> * <paramref name="b"/> = gcd(<paramref name="a"/>, <paramref name="b"/>).
    /// </summary>
    /// <param name="g">The GCD of <paramref name="a"/> and <paramref name="b"/>.</param>
    /// <param name="s">The Bezout coefficient of <paramref name="a"/>.</param>
    /// <param name="t">The Bezout coefficient of <paramref name="b"/>.</param>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters is null.</exception>
    /// <remarks>
    /// This method modifies the values of <paramref name="g"/>, <paramref name="s"/>, and <paramref name="t"/> in place.
    /// </remarks>
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
    /// Computes the greatest common divisor (GCD) of two <see cref="GmpInteger"/> values <paramref name="a"/> and <paramref name="b"/> using the extended Euclidean algorithm.
    /// </summary>
    /// <param name="a">The first <see cref="GmpInteger"/> value.</param>
    /// <param name="b">The second <see cref="GmpInteger"/> value.</param>
    /// <returns>A tuple of three <see cref="GmpInteger"/> values (g, s, t) such that g = gcd(a, b) and g = a * s + b * t.</returns>
    public static unsafe (GmpInteger g, GmpInteger s, GmpInteger t) Gcd2(GmpInteger a, GmpInteger b)
    {
        GmpInteger g = new();
        GmpInteger s = new();
        GmpInteger t = new();
        Gcd2Inplace(g, s, t, a, b);
        return (g, s, t);
    }

    /// <summary>
    /// Calculates the least common multiple (LCM) of two <see cref="GmpInteger"/> values <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <remarks>The values of <paramref name="op1"/> and <paramref name="op2"/> are not changed.</remarks>
    public static unsafe void LcmInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_lcm((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Computes the least common multiple (LCM) of two <see cref="GmpInteger"/> values <paramref name="op1"/> and <paramref name="op2"/>.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the LCM of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpInteger Lcm(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        LcmInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Calculates the least common multiple of <paramref name="op1"/> and <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance.</param>
    /// <param name="op2">The second unsigned integer value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rop"/> or <paramref name="op1"/> is null.</exception>
    /// <remarks>The result is stored in <paramref name="rop"/> and <paramref name="op1"/> and <paramref name="op2"/> are not modified.</remarks>
    public static unsafe void LcmInplace(GmpInteger rop, GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            GmpLib.__gmpz_lcm_ui((IntPtr)pr, (IntPtr)p1, new CULong(op2));
        }
    }

    /// <summary>
    /// Computes the least common multiple (LCM) of <paramref name="op1"/> and <paramref name="op2"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the LCM of <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe GmpInteger Lcm(GmpInteger op1, uint op2)
    {
        GmpInteger rop = new();
        LcmInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>
    /// Inverts <paramref name="op1"/> modulo <paramref name="op2"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to be inverted.</param>
    /// <param name="op2">The <see cref="GmpInteger"/> instance to be used as modulo.</param>
    /// <returns><c>true</c> if the inversion is successful, <c>false</c> otherwise.</returns>
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
    /// Calculates the inverse of <paramref name="op1"/> modulo <paramref name="op2"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to calculate the inverse of.</param>
    /// <param name="op2">The modulus <see cref="GmpInteger"/>.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the inverse of <paramref name="op1"/> modulo <paramref name="op2"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when unable to find the inverse of <paramref name="op1"/> and <paramref name="op2"/>.</exception>
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
    /// Computes the Jacobi symbol (a/b) for given integers <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The Jacobi symbol (a/b).</returns>
    public static unsafe int Jacobi(GmpInteger a, GmpInteger b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_jacobi((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compute the Legendre symbol of <paramref name="a"/> and <paramref name="p"/> using the Jacobi symbol implementation.
    /// </summary>
    /// <param name="a">The integer value.</param>
    /// <param name="p">The prime value.</param>
    /// <returns>The Legendre symbol of <paramref name="a"/> and <paramref name="p"/>.</returns>
    public static unsafe int Legendre(GmpInteger a, GmpInteger p) => Jacobi(a, p);

    /// <summary>
    /// Compute the Kronecker symbol of two integers <paramref name="a"/> and <paramref name="b"/> using the Jacobi symbol.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static unsafe int Kronecker(GmpInteger a, GmpInteger b) => Jacobi(a, b);

    /// <summary>
    /// Compute the Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The <see cref="GmpInteger"/> instance.</param>
    /// <param name="b">The integer value.</param>
    /// <returns>The Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static unsafe int Kronecker(GmpInteger a, int b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        {
            return GmpLib.__gmpz_kronecker_si((IntPtr)p1, new CLong(b));
        }
    }

    /// <summary>
    /// Compute the Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The <see cref="GmpInteger"/> instance.</param>
    /// <param name="b">The unsigned integer value.</param>
    /// <returns>The Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static unsafe int Kronecker(GmpInteger a, uint b)
    {
        fixed (Mpz_t* p1 = &a.Raw)
        {
            return GmpLib.__gmpz_kronecker_ui((IntPtr)p1, new CULong(b));
        }
    }

    /// <summary>
    /// Compute the Kronecker symbol of two integers <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer as a <see cref="GmpInteger"/> instance.</param>
    /// <returns>The Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static unsafe int Kronecker(int a, GmpInteger b)
    {
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_si_kronecker(new CLong(a), (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compute the Kronecker symbol of two integers <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns>The Kronecker symbol of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static unsafe int Kronecker(uint a, GmpInteger b)
    {
        fixed (Mpz_t* p2 = &b.Raw)
        {
            return GmpLib.__gmpz_ui_kronecker(new CULong(a), (IntPtr)p2);
        }
    }

    /// <summary>
    /// Removes the factor <paramref name="f"/> from <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="op">The <see cref="GmpInteger"/> instance to remove the factor from.</param>
    /// <param name="f">The <see cref="GmpInteger"/> instance representing the factor to remove.</param>
    /// <returns>The number of times the factor was removed from <paramref name="op"/>.</returns>
    public static unsafe uint RemoveFactorInplace(GmpInteger rop, GmpInteger op, GmpInteger f)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pop = &op.Raw)
        fixed (Mpz_t* pf = &f.Raw)
        {
            return (uint)GmpLib.__gmpz_remove((IntPtr)pr, (IntPtr)pop, (IntPtr)pf).Value;
        }
    }

    /// <summary>
    /// Removes the factor <paramref name="f"/> from <paramref name="op"/> and returns the result as a new <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> to remove the factor from.</param>
    /// <param name="f">The factor to remove.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of removing the factor.</returns>
    public static GmpInteger RemoveFactor(GmpInteger op, GmpInteger f)
    {
        GmpInteger rop = new();
        RemoveFactorInplace(rop, op, f);
        return rop;
    }

    /// <summary>
    /// Remove a factor <paramref name="f"/> from this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="f">The factor to remove.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of removing <paramref name="f"/> from this instance.</returns>
    public GmpInteger RemoveFactor(GmpInteger f) => RemoveFactor(this, f);

    /// <summary>
    /// Calculates the factorial of a given positive integer <paramref name="n"/> and stores the result in the provided <paramref name="rop"/> instance.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result of the factorial calculation.</param>
    /// <param name="n">The positive integer to calculate the factorial of.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is negative.</exception>
    /// <remarks>
    /// The result of the factorial calculation can be very large, so make sure that the <paramref name="rop"/> instance has enough memory allocated to store the result.
    /// </remarks>
    public static unsafe void FactorialInplace(GmpInteger rop, uint n)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_fac_ui((IntPtr)pr, new CULong(n));
        }
    }

    /// <summary>
    /// Calculates the factorial of a given non-negative integer <paramref name="n"/>.
    /// </summary>
    /// <param name="n">The non-negative integer to calculate the factorial of.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the factorial of <paramref name="n"/>.</returns>
    public static GmpInteger Factorial(uint n)
    {
        GmpInteger rop = new();
        FactorialInplace(rop, n);
        return rop;
    }

    /// <summary>
    /// Calculates the double factorial of a non-negative integer <paramref name="n"/> and stores the result in-place in the <paramref name="rop"/> parameter.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="n">The non-negative integer to calculate the double factorial of.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="n"/> is negative.</exception>
    public static unsafe void Factorial2Inplace(GmpInteger rop, uint n)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_2fac_ui((IntPtr)pr, new CULong(n));
        }
    }

    /// <summary>
    /// Calculates the factorial of a given non-negative integer <paramref name="n"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The non-negative integer to calculate the factorial of.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the factorial of <paramref name="n"/>.</returns>
    public static unsafe GmpInteger Factorial2(uint n)
    {
        GmpInteger rop = new();
        Factorial2Inplace(rop, n);
        return rop;
    }

    /// <summary>
    /// Calculate the m-factorial of n in place and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The integer value to calculate the factorial.</param>
    /// <param name="m">The integer value to specify the modulus.</param>
    /// <remarks>
    /// This method calculates the m-factorial of n, which is the product of all positive integers less than or equal to n that are congruent to m modulo n.
    /// The result is stored in <paramref name="rop"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="rop"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="n"/> is less than 1 or <paramref name="m"/> is greater than or equal to <paramref name="n"/>.</exception>
    /// <exception cref="ArithmeticException">An error occurred while calculating the factorial.</exception>
    public static unsafe void FactorialMInplace(GmpInteger rop, uint n, uint m)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_mfac_uiui((IntPtr)pr, new CULong(n), new CULong(m));
        }
    }

    /// <summary>
    /// Calculates the factorial of <paramref name="n"/> modulo <paramref name="m"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The number to calculate the factorial of.</param>
    /// <param name="m">The modulo value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the factorial of <paramref name="n"/> modulo <paramref name="m"/>.</returns>
    public static unsafe GmpInteger FactorialM(uint n, uint m)
    {
        GmpInteger rop = new();
        FactorialMInplace(rop, n, m);
        return rop;
    }

    /// <summary>
    /// Calculates the binomial coefficient of <paramref name="n"/> and <paramref name="k"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The <see cref="GmpInteger"/> instance representing the value of n.</param>
    /// <param name="k">The value of k.</param>
    /// <remarks>
    /// The binomial coefficient of n and k is defined as n! / (k! * (n - k)!).
    /// </remarks>
    public static unsafe void BinomialCoefficientInplace(GmpInteger rop, GmpInteger n, uint k)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* pn = &n.Raw)
        {
            GmpLib.__gmpz_bin_ui((IntPtr)pr, (IntPtr)pn, new CULong(k));
        }
    }

    /// <summary>
    /// Calculates the binomial coefficient of <paramref name="n"/> and <paramref name="k"/> and returns the result as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The first parameter of the binomial coefficient.</param>
    /// <param name="k">The second parameter of the binomial coefficient.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the binomial coefficient of <paramref name="n"/> and <paramref name="k"/>.</returns>
    public static unsafe GmpInteger BinomialCoefficient(GmpInteger n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// Compute the binomial coefficient of <paramref name="n"/> choose <paramref name="k"/> and store the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="n">The number of items.</param>
    /// <param name="k">The number of items to choose.</param>
    /// <remarks>
    /// The result is computed as <paramref name="n"/>! / (<paramref name="k"/>! * (<paramref name="n"/> - <paramref name="k"/>)!).
    /// </remarks>
    public static unsafe void BinomialCoefficientInplace(GmpInteger rop, uint n, uint k)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        {
            GmpLib.__gmpz_bin_uiui((IntPtr)pr, new CULong(n), new CULong(k));
        }
    }

    /// <summary>
    /// Calculates the binomial coefficient of two non-negative integers <paramref name="n"/> and <paramref name="k"/>.
    /// </summary>
    /// <param name="n">The first non-negative integer.</param>
    /// <param name="k">The second non-negative integer.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the binomial coefficient of <paramref name="n"/> and <paramref name="k"/>.</returns>
    public static unsafe GmpInteger BinomialCoefficient(uint n, uint k)
    {
        GmpInteger rop = new();
        BinomialCoefficientInplace(rop, n, k);
        return rop;
    }

    /// <summary>
    /// Calculate the Fibonacci number at position <paramref name="n"/> and store the result in-place in the <paramref name="fn"/> parameter.
    /// </summary>
    /// <param name="fn">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="n">The position of the Fibonacci number to calculate.</param>
    /// <remarks>
    /// The result will be stored in the <paramref name="fn"/> parameter, which will be modified in-place.
    /// </remarks>
    public static unsafe void FibonacciInplace(GmpInteger fn, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        {
            GmpLib.__gmpz_fib_ui((IntPtr)pr, new CULong(n));
        }
    }

    /// <summary>
    /// Calculates the nth Fibonacci number and returns it as a new instance of <see cref="GmpInteger"/>.
    /// </summary>
    /// <param name="n">The index of the Fibonacci number to calculate.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the calculated Fibonacci number.</returns>
    public static unsafe GmpInteger Fibonacci(uint n)
    {
        GmpInteger fn = new();
        FibonacciInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// Calculates the Fibonacci number at index <paramref name="n"/> using the two initial values <paramref name="fn"/> and <paramref name="fnsub1"/>.
    /// The result is stored in <paramref name="fn"/> and <paramref name="fnsub1"/>.
    /// </summary>
    /// <param name="fn">The first initial value of the Fibonacci sequence.</param>
    /// <param name="fnsub1">The second initial value of the Fibonacci sequence.</param>
    /// <param name="n">The index of the Fibonacci number to calculate.</param>
    /// <remarks>
    /// This method modifies the values of <paramref name="fn"/> and <paramref name="fnsub1"/> in place to store the result.
    /// </remarks>
    public static unsafe void Fibonacci2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        fixed (Mpz_t* pr1 = &fnsub1.Raw)
        {
            GmpLib.__gmpz_fib2_ui((IntPtr)pr, (IntPtr)pr1, new CULong(n));
        }
    }

    /// <summary>
    /// Calculates the Fibonacci sequence up to the <paramref name="n"/>th number and returns a tuple containing the <paramref name="n"/>th and <paramref name="n"/>-1th numbers.
    /// </summary>
    /// <param name="n">The index of the Fibonacci number to calculate.</param>
    /// <returns>A tuple containing the <paramref name="n"/>th and <paramref name="n"/>-1th Fibonacci numbers.</returns>
    public static unsafe (GmpInteger fn, GmpInteger fnsub1) Fibonacci2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        Fibonacci2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }

    /// <summary>
    /// Computes the <paramref name="n"/>th Lucas number in place and stores the result in the <paramref name="fn"/> parameter.
    /// </summary>
    /// <param name="fn">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="n">The index of the Lucas number to compute.</param>
    /// <remarks>
    /// The Lucas numbers form a sequence similar to the Fibonacci sequence, but with different starting values and recurrence relation.
    /// </remarks>
    public static unsafe void LucasNumInplace(GmpInteger fn, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        {
            GmpLib.__gmpz_lucnum_ui((IntPtr)pr, new CULong(n));
        }
    }

    /// <summary>
    /// Computes the Lucas number at index <paramref name="n"/>.
    /// </summary>
    /// <param name="n">The index of the Lucas number to compute.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the Lucas number at index <paramref name="n"/>.</returns>
    public static unsafe GmpInteger LucasNum(uint n)
    {
        GmpInteger fn = new();
        LucasNumInplace(fn, n);
        return fn;
    }

    /// <summary>
    /// Calculates the second Lucas number at index <paramref name="n"/> and stores the result in <paramref name="fn"/>.
    /// </summary>
    /// <param name="fn">The <see cref="GmpInteger"/> instance to store the result.</param>
    /// <param name="fnsub1">The <see cref="GmpInteger"/> instance representing the (n-1)th Lucas number.</param>
    /// <param name="n">The index of the Lucas number to calculate.</param>
    /// <remarks>
    /// The second Lucas number is defined as:
    /// L_2(n) = L(n-1) + L(n+1)
    /// where L_k is the kth Lucas number.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fn"/> or <paramref name="fnsub1"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="n"/> is less than 0.</exception>
    public static unsafe void LucasNum2Inplace(GmpInteger fn, GmpInteger fnsub1, uint n)
    {
        fixed (Mpz_t* pr = &fn.Raw)
        fixed (Mpz_t* pr1 = &fnsub1.Raw)
        {
            GmpLib.__gmpz_lucnum2_ui((IntPtr)pr, (IntPtr)pr1, new CULong(n));
        }
    }

    /// <summary>
    /// Computes the Lucas Numbers of the second kind for the given index <paramref name="n"/> and returns a tuple containing the result and the previous number in the sequence.
    /// </summary>
    /// <param name="n">The index of the Lucas Number to compute.</param>
    /// <returns>A tuple containing the computed Lucas Number and the previous number in the sequence.</returns>
    public static unsafe (GmpInteger fn, GmpInteger fnsub1) LucasNum2(uint n)
    {
        GmpInteger fn = new();
        GmpInteger fnsub1 = new();
        LucasNum2Inplace(fn, fnsub1, n);
        return (fn, fnsub1);
    }
    #endregion

    #region Comparison Functions

    /// <summary>
    /// Compares two <see cref="GmpInteger"/> values and returns an integer that indicates their relationship to one another in the sort order.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// Value Meaning
    /// Less than zero: <paramref name="op1"/> is less than <paramref name="op2"/>.
    /// Zero: <paramref name="op1"/> equals <paramref name="op2"/>.
    /// Greater than zero: <paramref name="op1"/> is greater than <paramref name="op2"/>.
    /// </returns>
    public static unsafe int Compare(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_cmp((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compares the current <see cref="GmpInteger"/> object with the specified object and returns an integer that indicates whether the current object is less than, equal to, or greater than the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>A signed integer that indicates the relative values of this instance and <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a valid type.</exception>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        GmpInteger z => Compare(this, z),
        double d => Compare(this, d),
        int i => Compare(this, i),
        uint ui => Compare(this, ui),
        GmpFloat f => -GmpFloat.Compare(f, this),
        _ => throw new ArgumentException($"obj must be GmpInteger, int, uint, double, GmpFloat"),
    };

    /// <summary>
    /// Compares this <see cref="GmpInteger"/> instance to another <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="GmpInteger"/> instance to compare with this instance.</param>
    /// <returns>A signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
    /// Less than zero: This instance is less than <paramref name="other"/>.
    /// Zero: This instance is equal to <paramref name="other"/>.
    /// Greater than zero: This instance is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo([AllowNull] GmpInteger other) => other switch
    {
        null => 1,
        _ => Compare(this, other)
    };

    /// <summary>
    /// Determines whether the current <see cref="GmpInteger"/> object is equal to another <see cref="GmpInteger"/> object.
    /// </summary>
    /// <param name="other">The <see cref="GmpInteger"/> to compare with the current <see cref="GmpInteger"/> object.</param>
    /// <returns><c>true</c> if the specified <see cref="GmpInteger"/> is equal to the current <see cref="GmpInteger"/>; otherwise, <c>false</c>.</returns>
    public bool Equals([AllowNull] GmpInteger other) => other switch
    {
        null => false,
        _ => Compare(this, other) == 0,
    };

    /// <summary>Determines whether two <see cref="GmpInteger" /> instances are equal (op1 == op2).</summary>
    public static bool operator ==(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) == 0;

    /// <summary>Determines whether two <see cref="GmpInteger" /> instances are not equal (op1 != op2).</summary>
    public static bool operator !=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) != 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than another instance (op1 > op2).</summary>
    public static bool operator >(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) > 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than another instance (op1 &lt; op2).</summary>
    public static bool operator <(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) < 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than or equal to another instance (op1 >= op2).</summary>
    public static bool operator >=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) >= 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than or equal to another instance (op1 &lt;= op2).</summary>
    public static bool operator <=(GmpInteger op1, GmpInteger op2) => Compare(op1, op2) <= 0;

    /// <summary>
    /// Compares a <see cref="GmpInteger"/> instance with a double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> instance to compare.</param>
    /// <param name="op2">The double-precision floating-point number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    ///     <listheader>
    ///         <term>Value</term>
    ///         <description>Meaning</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Less than zero</term>
    ///         <description><paramref name="op1"/> is less than <paramref name="op2"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term>Zero</term>
    ///         <description><paramref name="op1"/> equals <paramref name="op2"/>.</description>
    ///     </item>
    ///     <item>
    ///         <term>Greater than zero</term>
    ///         <description><paramref name="op1"/> is greater than <paramref name="op2"/>.</description>
    ///     </item>
    /// </list>
    /// </returns>
    public static unsafe int Compare(GmpInteger op1, double op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_d((IntPtr)p1, op2);
        }
    }

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is equal to a double (op1 == op2).</summary>
    public static bool operator ==(GmpInteger op1, double op2) => Compare(op1, op2) == 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is not equal to a double (op1 != op2).</summary>
    public static bool operator !=(GmpInteger op1, double op2) => Compare(op1, op2) != 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than a double (op1 > op2).</summary>
    public static bool operator >(GmpInteger op1, double op2) => Compare(op1, op2) > 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than a double (op1 &lt; op2).</summary>
    public static bool operator <(GmpInteger op1, double op2) => Compare(op1, op2) < 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than or equal to a double (op1 >= op2).</summary>
    public static bool operator >=(GmpInteger op1, double op2) => Compare(op1, op2) >= 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than or equal to a double (op1 &lt;= op2).</summary>
    public static bool operator <=(GmpInteger op1, double op2) => Compare(op1, op2) <= 0;

    /// <summary>Determines whether a double is equal to a <see cref="GmpInteger" /> instance (op1 == op2).</summary>
    public static bool operator ==(double op1, GmpInteger op2) => Compare(op2, op1) == 0;

    /// <summary>Determines whether a double is not equal to a <see cref="GmpInteger" /> instance (op1 != op2).</summary>
    public static bool operator !=(double op1, GmpInteger op2) => Compare(op2, op1) != 0;

    /// <summary>Determines whether a double is greater than a <see cref="GmpInteger" /> instance (op1 > op2).</summary>
    public static bool operator >(double op1, GmpInteger op2) => Compare(op2, op1) < 0;

    /// <summary>Determines whether a double is less than a <see cref="GmpInteger" /> instance (op1 &lt; op2).</summary>
    public static bool operator <(double op1, GmpInteger op2) => Compare(op2, op1) > 0;

    /// <summary>Determines whether a double is greater than or equal to a <see cref="GmpInteger" /> instance (op1 >= op2).</summary>
    public static bool operator >=(double op1, GmpInteger op2) => Compare(op2, op1) <= 0;

    /// <summary>Determines whether a double is less than or equal to a <see cref="GmpInteger" /> instance (op1 &lt;= op2).</summary>
    public static bool operator <=(double op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compare this GmpInteger to an int value.
    /// </summary>
    /// <param name="op1">The first GmpInteger to compare.</param>
    /// <param name="op2">The second int to compare.</param>
    /// <returns>A value indicating the relative values of this instance and a specified object.</returns>
    public static unsafe int Compare(GmpInteger op1, int op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_si((IntPtr)p1, new CLong(op2));
        }
    }

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is equal to an integer (op1 == op2).</summary>
    public static bool operator ==(GmpInteger op1, int op2) => Compare(op1, op2) == 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is not equal to an integer (op1 != op2).</summary>
    public static bool operator !=(GmpInteger op1, int op2) => Compare(op1, op2) != 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than an integer (op1 > op2).</summary>
    public static bool operator >(GmpInteger op1, int op2) => Compare(op1, op2) > 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than an integer (op1 &lt; op2).</summary>
    public static bool operator <(GmpInteger op1, int op2) => Compare(op1, op2) < 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than or equal to an integer (op1 >= op2).</summary>
    public static bool operator >=(GmpInteger op1, int op2) => Compare(op1, op2) >= 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than or equal to an integer (op1 &lt;= op2).</summary>
    public static bool operator <=(GmpInteger op1, int op2) => Compare(op1, op2) <= 0;

    /// <summary>Determines whether an integer is equal to a <see cref="GmpInteger" /> instance (op1 == op2).</summary>
    public static bool operator ==(int op1, GmpInteger op2) => Compare(op2, op1) == 0;

    /// <summary>Determines whether an integer is not equal to a <see cref="GmpInteger" /> instance (op1 != op2).</summary>
    public static bool operator !=(int op1, GmpInteger op2) => Compare(op2, op1) != 0;

    /// <summary>Determines whether an integer is greater than a <see cref="GmpInteger" /> instance (op1 > op2).</summary>
    public static bool operator >(int op1, GmpInteger op2) => Compare(op2, op1) < 0;

    /// <summary>Determines whether an integer is less than a <see cref="GmpInteger" /> instance (op1 &lt; op2).</summary>
    public static bool operator <(int op1, GmpInteger op2) => Compare(op2, op1) > 0;

    /// <summary>Determines whether an integer is greater than or equal to a <see cref="GmpInteger" /> instance (op1 >= op2).</summary>
    public static bool operator >=(int op1, GmpInteger op2) => Compare(op2, op1) <= 0;

    /// <summary>Determines whether an integer is less than or equal to a <see cref="GmpInteger" /> instance (op1 &lt;= op2).</summary>
    public static bool operator <=(int op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Compares a <see cref="GmpInteger"/> instance with an unsigned integer.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance to compare.</param>
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
    public static unsafe int Compare(GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmp_ui((IntPtr)p1, new CULong(op2));
        }
    }

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is equal to a uint (op1 == op2).</summary>
    public static bool operator ==(GmpInteger op1, uint op2) => Compare(op1, op2) == 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is not equal to a uint (op1 != op2).</summary>
    public static bool operator !=(GmpInteger op1, uint op2) => Compare(op1, op2) != 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than a uint (op1 > op2).</summary>
    public static bool operator >(GmpInteger op1, uint op2) => Compare(op1, op2) > 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than a uint (op1 &lt; op2).</summary>
    public static bool operator <(GmpInteger op1, uint op2) => Compare(op1, op2) < 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is greater than or equal to a uint (op1 >= op2).</summary>
    public static bool operator >=(GmpInteger op1, uint op2) => Compare(op1, op2) >= 0;

    /// <summary>Determines whether a <see cref="GmpInteger" /> instance is less than or equal to a uint (op1 &lt;= op2).</summary>
    public static bool operator <=(GmpInteger op1, uint op2) => Compare(op1, op2) <= 0;

    /// <summary>Determines whether a uint is equal to a <see cref="GmpInteger" /> instance (op1 == op2).</summary>
    public static bool operator ==(uint op1, GmpInteger op2) => Compare(op2, op1) == 0;

    /// <summary>Determines whether a uint is not equal to a <see cref="GmpInteger" /> instance (op1 != op2).</summary>
    public static bool operator !=(uint op1, GmpInteger op2) => Compare(op2, op1) != 0;

    /// <summary>Determines whether a uint is greater than a <see cref="GmpInteger" /> instance (op1 > op2).</summary>
    public static bool operator >(uint op1, GmpInteger op2) => Compare(op2, op1) < 0;

    /// <summary>Determines whether a uint is less than a <see cref="GmpInteger" /> instance (op1 &lt; op2).</summary>
    public static bool operator <(uint op1, GmpInteger op2) => Compare(op2, op1) > 0;

    /// <summary>Determines whether a uint is greater than or equal to a <see cref="GmpInteger" /> instance (op1 >= op2).</summary>
    public static bool operator >=(uint op1, GmpInteger op2) => Compare(op2, op1) <= 0;

    /// <summary>Determines whether a uint is less than or equal to a <see cref="GmpInteger" /> instance (op1 &lt;= op2).</summary>
    public static bool operator <=(uint op1, GmpInteger op2) => Compare(op2, op1) >= 0;

    /// <summary>
    /// Determines whether the current <see cref="GmpInteger"/> object is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns the hash code for this <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <returns>An integer hash code.</returns>
    public override int GetHashCode() => Raw.GetHashCode();

    /// <summary>
    /// Compares the absolute values of two <see cref="GmpInteger"/> objects.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>. 
    /// If the absolute value of <paramref name="op1"/> is greater than the absolute value of <paramref name="op2"/>, 
    /// the method returns 1. If the absolute value of <paramref name="op1"/> is less than the absolute value of <paramref name="op2"/>, 
    /// the method returns -1. If the absolute value of <paramref name="op1"/> equals the absolute value of <paramref name="op2"/>, 
    /// the method returns 0.</returns>
    public static unsafe int CompareAbs(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return GmpLib.__gmpz_cmpabs((IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Compares the absolute value of a <see cref="GmpInteger"/> with a double-precision floating-point number.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The double-precision floating-point number to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>, as shown in the following table.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>Less than zero</term>
    /// <description><paramref name="op1"/> is less than the absolute value of <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Zero</term>
    /// <description><paramref name="op1"/> is equal to the absolute value of <paramref name="op2"/>.</description>
    /// </item>
    /// <item>
    /// <term>Greater than zero</term>
    /// <description><paramref name="op1"/> is greater than the absolute value of <paramref name="op2"/>.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static unsafe int CompareAbs(GmpInteger op1, double op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmpabs_d((IntPtr)p1, op2);
        }
    }

    /// <summary>
    /// Compares the absolute value of a <see cref="GmpInteger"/> with an unsigned integer.
    /// </summary>
    /// <param name="op1">The <see cref="GmpInteger"/> to compare.</param>
    /// <param name="op2">The unsigned integer to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="op1"/> and <paramref name="op2"/>. If the absolute value of <paramref name="op1"/> is greater than <paramref name="op2"/>, the return value is greater than 0. If the absolute value of <paramref name="op1"/> is equal to <paramref name="op2"/>, the return value is 0. If the absolute value of <paramref name="op1"/> is less than <paramref name="op2"/>, the return value is less than 0.</returns>
    /// <remarks>This method compares the absolute value of <paramref name="op1"/> with an unsigned integer <paramref name="op2"/>.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="op1"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="op1"/> is negative.</exception>
    public static unsafe int CompareAbs(GmpInteger op1, uint op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        {
            return GmpLib.__gmpz_cmpabs_ui((IntPtr)p1, new CULong(op2));
        }
    }

    /// <summary>The sign of the <see cref="GmpInteger"/>.</summary>
    public int Sign => Raw.Size < 0 ? -1 : Raw.Size > 0 ? 1 : 0;
    #endregion

    #region Logical and Bit Manipulation Functions

    /// <summary>
    /// Performs a bitwise AND operation between two <see cref="GmpInteger"/> operands and stores the result in the first operand.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> operand to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand to perform the operation.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to perform the operation.</param>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="rop"/> will be modified.</remarks>
    public static unsafe void BitwiseAndInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_and((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Computes the bitwise AND of two <see cref="GmpInteger"/> values.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> value.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> value.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the bitwise AND operation.</returns>
    public static GmpInteger BitwiseAnd(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseAndInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>Performs a bitwise AND operation on two <see cref="GmpInteger" /> instances (op1 &amp; op2).</summary>
    public static GmpInteger operator &(GmpInteger op1, GmpInteger op2) => BitwiseAnd(op1, op2);

    /// <summary>
    /// Performs a bitwise OR operation between two <see cref="GmpInteger"/> operands and stores the result in the first operand.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> operand to store the result of the operation.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand to use in the operation.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand to use in the operation.</param>
    /// <remarks>The operation is performed in-place, meaning the value of <paramref name="rop"/> will be modified.</remarks>
    public static unsafe void BitwiseOrInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_ior((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Computes the bitwise OR of two <see cref="GmpInteger"/> values.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the bitwise OR operation.</returns>
    public static GmpInteger BitwiseOr(GmpInteger op1, GmpInteger op2)
    {
        GmpInteger rop = new();
        BitwiseOrInplace(rop, op1, op2);
        return rop;
    }

    /// <summary>Performs a bitwise OR operation on two <see cref="GmpInteger"/> instances and returns the result as a new <see cref="GmpInteger"/>.</summary>
    public static GmpInteger operator |(GmpInteger op1, GmpInteger op2) => BitwiseOr(op1, op2);

    /// <summary>
    /// Computes the bitwise XOR of two <see cref="GmpInteger"/> values and stores the result in the first operand.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> value to store the result in.</param>
    /// <param name="op1">The first <see cref="GmpInteger"/> operand.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> operand.</param>
    /// <remarks>The result is computed as <paramref name="rop"/> = <paramref name="op1"/> XOR <paramref name="op2"/>.</remarks>
    public static unsafe void BitwiseXorInplace(GmpInteger rop, GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            GmpLib.__gmpz_xor((IntPtr)pr, (IntPtr)p1, (IntPtr)p2);
        }
    }

    /// <summary>
    /// Computes the bitwise XOR of two <see cref="GmpInteger"/> operands.
    /// </summary>
    /// <param name="op1">The first operand.</param>
    /// <param name="op2">The second operand.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the result of the bitwise XOR operation.</returns>
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
    /// Computes the one's complement of the <paramref name="op"/> and stores the result in <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> instance to store the result in.</param>
    /// <param name="op">The <see cref="GmpInteger"/> instance to compute the one's complement of.</param>
    /// <remarks>
    /// The one's complement of a binary number is the value obtained by inverting all the bits (swapping 0s for 1s and vice versa).
    /// </remarks>
    public static unsafe void ComplementInplace(GmpInteger rop, GmpInteger op)
    {
        fixed (Mpz_t* pr = &rop.Raw)
        fixed (Mpz_t* p = &op.Raw)
        {
            GmpLib.__gmpz_com((IntPtr)pr, (IntPtr)p);
        }
    }

    /// <summary>
    /// Computes the one's complement of a <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="op">The <see cref="GmpInteger"/> instance to complement.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> representing the one's complement of <paramref name="op"/>.</returns>
    public static GmpInteger Complement(GmpInteger op)
    {
        GmpInteger rop = new();
        ComplementInplace(rop, op);
        return rop;
    }

    /// <summary>
    /// Returns the number of set bits (population count) in the binary representation of the current <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <returns>The number of set bits in the binary representation of the current <see cref="GmpInteger"/> instance.</returns>
    public unsafe uint PopulationCount()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (uint)GmpLib.__gmpz_popcount((IntPtr)ptr).Value;
        }
    }

    /// <summary>
    /// Calculates the Hamming distance between two <see cref="GmpInteger"/> instances.
    /// </summary>
    /// <param name="op1">The first <see cref="GmpInteger"/> instance.</param>
    /// <param name="op2">The second <see cref="GmpInteger"/> instance.</param>
    /// <returns>The Hamming distance between <paramref name="op1"/> and <paramref name="op2"/>.</returns>
    public static unsafe uint HammingDistance(GmpInteger op1, GmpInteger op2)
    {
        fixed (Mpz_t* p1 = &op1.Raw)
        fixed (Mpz_t* p2 = &op2.Raw)
        {
            return (uint)GmpLib.__gmpz_hamdist((IntPtr)p1, (IntPtr)p2).Value;
        }
    }

    /// <summary>
    /// Finds the index of the first 0 bit in the binary representation of the current <see cref="GmpInteger"/> instance, starting from <paramref name="startingBit"/> position.
    /// </summary>
    /// <param name="startingBit">The bit position to start searching from. Default is 0.</param>
    /// <returns>The index of the first 0 bit, or -1 if no 0 bit is found.</returns>
    public unsafe uint FirstIndexOf0(uint startingBit = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (uint)GmpLib.__gmpz_scan0((IntPtr)ptr, new CULong(startingBit)).Value;
        }
    }

    /// <summary>
    /// Finds the index of the first bit with value 1 in the <see cref="GmpInteger"/> instance, starting from the specified <paramref name="startingBit"/> index.
    /// </summary>
    /// <param name="startingBit">The index to start searching from. Default is 0.</param>
    /// <returns>The index of the first bit with value 1, or -1 if no such bit is found.</returns>
    public unsafe uint FirstIndexOf1(uint startingBit = 0)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return (uint)GmpLib.__gmpz_scan1((IntPtr)ptr, new CULong(startingBit)).Value;
        }
    }

    /// <summary>
    /// Set the bit at the specified <paramref name="bitIndex"/> to 1.
    /// </summary>
    /// <param name="bitIndex">The index of the bit to set to 1.</param>
    /// <remarks>
    /// If the bit at the specified <paramref name="bitIndex"/> is already 1, this method has no effect.
    /// </remarks>
    public unsafe void SetBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_setbit((IntPtr)ptr, new CULong(bitIndex));
        }
    }

    /// <summary>
    /// Clears the bit at the specified <paramref name="bitIndex"/> in the current <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="bitIndex">The zero-based index of the bit to clear.</param>
    /// <remarks>
    /// The bit at the specified <paramref name="bitIndex"/> is set to 0.
    /// </remarks>
    public unsafe void ClearBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_clrbit((IntPtr)ptr, new CULong(bitIndex));
        }
    }

    /// <summary>
    /// Complements the bit at the specified <paramref name="bitIndex"/> in the <see cref="GmpInteger"/> instance.
    /// </summary>
    /// <param name="bitIndex">The index of the bit to complement.</param>
    /// <remarks>
    /// The bit at the specified <paramref name="bitIndex"/> is flipped, i.e. 0 becomes 1 and 1 becomes 0.
    /// </remarks>
    public unsafe void ComplementBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__gmpz_combit((IntPtr)ptr, new CULong(bitIndex));
        }
    }

    /// <summary>
    /// Tests whether the bit at the specified <paramref name="bitIndex"/> in the current <see cref="GmpInteger"/> is set or not.
    /// </summary>
    /// <param name="bitIndex">The zero-based index of the bit to test.</param>
    /// <returns>1 if the bit is set; otherwise, 0.</returns>
    public unsafe int TestBit(uint bitIndex)
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            return GmpLib.__gmpz_tstbit((IntPtr)ptr, new CULong(bitIndex));
        }
    }

    /// <summary>
    /// Generates a random <see cref="GmpInteger"/> with a maximum number of limbs specified by <paramref name="maxLimbCount"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> to store the generated random number.</param>
    /// <param name="maxLimbCount">The maximum number of limbs of the generated random number.</param>
    /// <remarks>
    /// This method is obsolete, use <see cref="GmpRandom"/> instead.
    /// </remarks>
    [Obsolete("use GmpRandom")]
    public static unsafe void RandomInplace(GmpInteger rop, int maxLimbCount)
    {
        fixed (Mpz_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpz_random((IntPtr)ptr, new CLong(maxLimbCount));
        }
    }

    /// <summary>
    /// Creates a random <see cref="GmpInteger"/> instance with a maximum limb count of <paramref name="maxLimbCount"/>.
    /// </summary>
    /// <param name="maxLimbCount">The maximum number of limbs the resulting <see cref="GmpInteger"/> can have.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> with random value.</returns>
    /// <remarks>This method is obsolete, use <see cref="GmpRandom"/> instead.</remarks>
    [Obsolete("use GmpRandom")]
    public static unsafe GmpInteger Random(int maxLimbCount)
    {
        GmpInteger rop = new();
        RandomInplace(rop, maxLimbCount);
        return rop;
    }

    /// <summary>
    /// Generates a random <see cref="GmpInteger"/> with <paramref name="maxLimbCount"/> limbs and assigns it to <paramref name="rop"/>.
    /// </summary>
    /// <param name="rop">The <see cref="GmpInteger"/> to assign the generated random value to.</param>
    /// <param name="maxLimbCount">The maximum number of limbs the generated random value can have.</param>
    /// <remarks>
    /// This method is obsolete, use <see cref="GmpRandom"/> instead.
    /// </remarks>
    [Obsolete("use GmpRandom")]
    public static unsafe void Random2Inplace(GmpInteger rop, int maxLimbCount)
    {
        fixed (Mpz_t* ptr = &rop.Raw)
        {
            GmpLib.__gmpz_random((IntPtr)ptr, new CLong(maxLimbCount));
        }
    }

    /// <summary>
    /// Creates a random <see cref="GmpInteger"/> instance with a maximum limb count of <paramref name="maxLimbCount"/>.
    /// </summary>
    /// <param name="maxLimbCount">The maximum limb count of the generated <see cref="GmpInteger"/>.</param>
    /// <returns>A new instance of <see cref="GmpInteger"/> with random value.</returns>
    /// <remarks>This method is obsolete, use <see cref="GmpRandom"/> instead.</remarks>
    [Obsolete("use GmpRandom")]
    public static unsafe GmpInteger Random2(int maxLimbCount)
    {
        GmpInteger rop = new();
        RandomInplace(rop, maxLimbCount);
        return rop;
    }
    #endregion
}
