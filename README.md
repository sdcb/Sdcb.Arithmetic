# Sdcb.Arithmetic [![main](https://github.com/sdcb/Sdcb.Arithmetic/actions/workflows/test-nuget.yml/badge.svg)](https://github.com/sdcb/Sdcb.Arithmetic/actions/workflows/test-nuget.yml) [![QQ](https://img.shields.io/badge/QQ_Group-495782587-52B6EF?style=social&logo=tencent-qq&logoColor=000&logoWidth=20)](http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=mma4msRKd372Z6dWpmBp4JZ9RL4Jrf8X&authKey=gccTx0h0RaH5b8B8jtuPJocU7MgFRUznqbV%2FLgsKdsK8RqZE%2BOhnETQ7nYVTp1W0&noverify=0&group_code=495782587)

`Sdcb.Arithmetic` is a modern `.NET` library that can PInvoke to `gmp` and `mpfr`, that enable both high performance and best .NET convenience.

Known classes in `Sdcb.Arithmetic`:

| Class       | Native name     | Library              |
| ----------- | --------------- | -------------------- |
| GmpInteger  | mpz_t           | Sdcb.Arithmetic.Gmp  |
| GmpFloat    | mpf_t           | Sdcb.Arithmetic.Gmp  |
| GmpRational | mpq_t           | Sdcb.Arithmetic.Gmp  |
| GmpRandom   | gmp_randstate_t | Sdcb.Arithmetic.Gmp  |
| MpfrFloat   | mpfr_t          | Sdcb.Arithmetic.Mpfr |

## NuGet Packages

### libgmp

| Package Id                                   | Version                                                                                                    | License | Notes                     |
| -------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | ------- | ------------------------- |
| Sdcb.Arithmetic.Gmp                          | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp) | MIT     | .NET binding for `libgmp` |
| Sdcb.Arithmetic.Gmp.runtime.win-x64          | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.win-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.win-x64) | LGPL    | native lib in Windows x64 |
| Sdcb.Arithmetic.Gmp.runtime.win-x86          | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.win-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.win-x86) | LGPL    | native lib in Windows x86 |
| Sdcb.Arithmetic.Gmp.runtime.linux-x64        | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-x64) | LGPL    | native lib in Linux x64   |
| Sdcb.Arithmetic.Gmp.runtime.linux-x86        | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-x86) | LGPL    | native lib in Linux x86   |
| Sdcb.Arithmetic.Gmp.runtime.linux-arm        | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-arm.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-arm) | LGPL    | native lib in Linux ARM   |
| Sdcb.Arithmetic.Gmp.runtime.linux-arm64      | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-arm64) | LGPL    | native lib in Linux ARM64 |
| Sdcb.Arithmetic.Gmp.runtime.linux-musl-x64   | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-musl-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-musl-x64) | LGPL    | native lib in Linux Musl x64 |
| Sdcb.Arithmetic.Gmp.runtime.linux-musl-arm64 | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.linux-musl-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.linux-musl-arm64) | LGPL    | native lib in Linux Musl ARM64 |
| Sdcb.Arithmetic.Gmp.runtime.osx-arm64        | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.osx-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.osx-arm64) | LGPL    | native lib in macOS ARM64 |
| Sdcb.Arithmetic.Gmp.runtime.osx-x64          | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.osx-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.osx-x64) | LGPL    | native lib in macOS x64   |
| Sdcb.Arithmetic.Gmp.runtime.android-arm      | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.android-arm.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.android-arm) | LGPL    | native lib in Android ARM |
| Sdcb.Arithmetic.Gmp.runtime.android-arm64    | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.android-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.android-arm64) | LGPL    | native lib in Android ARM64 |
| Sdcb.Arithmetic.Gmp.runtime.android-x86      | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.android-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.android-x86) | LGPL    | native lib in Android x86 |
| Sdcb.Arithmetic.Gmp.runtime.android-x64      | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Gmp.runtime.android-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Gmp.runtime.android-x64) | LGPL    | native lib in Android x64 |

Update: This library also tested and works good in Loongarch64(龙芯)

### mpfr

| Package Id                                   | Version                                                                                                    | License | Notes                      |
| -------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | ------- | -------------------------- |
| Sdcb.Arithmetic.Mpfr                         | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr) | MIT     | .NET binding for `libmpfr` |
| Sdcb.Arithmetic.Mpfr.runtime.win-x64         | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.win-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.win-x64) | LGPL    | native lib in Windows x64  |
| Sdcb.Arithmetic.Mpfr.runtime.win-x86         | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.win-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.win-x86) | LGPL    | native lib in Windows x86  |
| Sdcb.Arithmetic.Mpfr.runtime.linux-x64       | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-x64) | LGPL    | native lib in Linux x64    |
| Sdcb.Arithmetic.Mpfr.runtime.linux-x86       | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-x86) | LGPL    | native lib in Linux x86    |
| Sdcb.Arithmetic.Mpfr.runtime.linux-arm       | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-arm.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-arm) | LGPL    | native lib in Linux ARM    |
| Sdcb.Arithmetic.Mpfr.runtime.linux-arm64     | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-arm64) | LGPL    | native lib in Linux ARM64  |
| Sdcb.Arithmetic.Mpfr.runtime.linux-musl-x64  | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-musl-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-musl-x64) | LGPL    | native lib in Linux Musl x64 |
| Sdcb.Arithmetic.Mpfr.runtime.linux-musl-arm64| [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.linux-musl-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.linux-musl-arm64) | LGPL    | native lib in Linux Musl ARM64 |
| Sdcb.Arithmetic.Mpfr.runtime.osx-arm64       | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.osx-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.osx-arm64) | LGPL    | native lib in macOS ARM64  |
| Sdcb.Arithmetic.Mpfr.runtime.osx-x64         | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.osx-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.osx-x64) | LGPL    | native lib in macOS x64    |
| Sdcb.Arithmetic.Mpfr.runtime.android-arm     | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.android-arm.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.android-arm) | LGPL    | native lib in Android ARM  |
| Sdcb.Arithmetic.Mpfr.runtime.android-arm64   | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.android-arm64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.android-arm64) | LGPL    | native lib in Android ARM64 |
| Sdcb.Arithmetic.Mpfr.runtime.android-x86     | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.android-x86.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.android-x86) | LGPL    | native lib in Android x86  |
| Sdcb.Arithmetic.Mpfr.runtime.android-x64     | [![NuGet](https://img.shields.io/nuget/v/Sdcb.Arithmetic.Mpfr.runtime.android-x64.svg)](https://nuget.org/packages/Sdcb.Arithmetic.Mpfr.runtime.android-x64) | LGPL    | native lib in Android x64  |

### Native dynamic library name mapping

Defined in `GmpNativeLoader.cs` and `MpfrNativeLoader.cs`

| OS      | gmp dynamic lib | mpfr dynamic lib |
| ------- | --------------- | ---------------- |
| Windows | libgmp-10.dll   | libmpfr-6.dll    |
| Linux   | libgmp.so.10    | libmpfr.so.6     |
| MacOS   | libgmp.10.dylib | libmpfr.6.dylib  |
| Android | libgmp.so       | libmpfr.so       |
| Others  | gmp.10          | mpfr.6           |

## Examples

### Calculate 1,000,000 length of π using `Sdcb.Arithmetic.Gmp`:

```csharp
// Install NuGet package: Sdcb.Arithmetic.Gmp
// Install NuGet package: Sdcb.Arithmetic.Gmp.runtime.win-x64(for windows)
using Sdcb.Arithmetic.Gmp;

Console.WriteLine(CalcPI().ToString("N1000000"));

GmpFloat CalcPI(int inputDigits = 1_000_000)
{
    const double DIGITS_PER_TERM = 14.1816474627254776555; // = log(53360^3) / log(10)
    int DIGITS = (int)Math.Max(inputDigits, Math.Ceiling(DIGITS_PER_TERM));
    uint PREC = (uint)(DIGITS * Math.Log2(10));
    int N = (int)(DIGITS / DIGITS_PER_TERM);
    const int A = 13591409;
    const int B = 545140134;
    const int C = 640320;
    const int D = 426880;
    const int E = 10005;
    const double E3_24 = (double)C * C * C / 24;

    using PQT pqt = ComputePQT(0, N);

    GmpFloat pi = new(precision: PREC);
    // pi = D * sqrt((mpf_class)E) * PQT.Q;
    pi.Assign(GmpFloat.From(D, PREC) * GmpFloat.Sqrt((GmpFloat)E, PREC) * (GmpFloat)pqt.Q);
    // pi /= (A * PQT.Q + PQT.T);
    GmpFloat.DivideInplace(pi, pi, GmpFloat.From(A * pqt.Q + pqt.T, PREC));
    return pi;

    PQT ComputePQT(int n1, int n2)
    {
        int m;

        if (n1 + 1 == n2)
        {
            PQT res = new()
            {
                P = GmpInteger.From(2 * n2 - 1)
            };
            GmpInteger.MultiplyInplace(res.P, res.P, 6 * n2 - 1);
            GmpInteger.MultiplyInplace(res.P, res.P, 6 * n2 - 5);

            GmpInteger q = GmpInteger.From(E3_24);
            GmpInteger.MultiplyInplace(q, q, n2);
            GmpInteger.MultiplyInplace(q, q, n2);
            GmpInteger.MultiplyInplace(q, q, n2);
            res.Q = q;

            GmpInteger t = GmpInteger.From(B);
            GmpInteger.MultiplyInplace(t, t, n2);
            GmpInteger.AddInplace(t, t, A);
            GmpInteger.MultiplyInplace(t, t, res.P);
            // res.T = (A + B * n2) * res.P;            
            if ((n2 & 1) == 1) GmpInteger.NegateInplace(t, t);
            res.T = t;

            return res;
        }
        else
        {
            m = (n1 + n2) / 2;
            PQT res1 = ComputePQT(n1, m);
            using PQT res2 = ComputePQT(m, n2);
            GmpInteger p = res1.P * res2.P;
            GmpInteger q = res1.Q * res2.Q;

            // t = res1.T * res2.Q + res1.P * res2.T
            GmpInteger.MultiplyInplace(res1.T, res1.T, res2.Q);
            GmpInteger.MultiplyInplace(res1.P, res1.P, res2.T);
            GmpInteger.AddInplace(res1.T, res1.T, res1.P);
            res1.P.Dispose();
            res1.Q.Dispose();
            return new PQT
            {
                P = p,
                Q = q,
                T = res1.T,
            };
        }
    }
}

public ref struct PQT
{
    public GmpInteger P;
    public GmpInteger Q;
    public GmpInteger T;

    public readonly void Dispose()
    {
        P?.Dispose();
        Q?.Dispose();
        T?.Dispose();
    }
}
```

## Technical notes

### Why choosing struct in class design instead of raw memory IntPtr design?
* (1) `Struct in class` design:
  ```csharp
  class GmpInteger
  {
    public readonly Mpz_t Raw;

    public unsafe void DoWork()
    {
        fixed (Mpz_t* ptr = &Raw)
        {
            GmpLib.__dowork((IntPtr)ptr);
        }
    }
  }

  struct Mpz_t
  {
    public int A, B;
    public IntPtr Limbs;
  }
  ```
* (2) `Raw memory IntPtr` design:
  ```csharp
  class GmpInteger : IDisposable
  {
    public readonly IntPtr Raw;

    public unsafe GmpInteger()
    {
        Raw = Marshal.AllocHGlobal(sizeof(Mpz_t));
    }

    public void DoWork()
    {
        GmpLib.__dowork(Raw);
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Raw);
    }
  }
  ```

  Here is some benchmark I tested for both `DoWork` senario and `initialize-dispose` senario:

  Details:
  * init & dispose combines following actions:
    * allocating struct memory
    * calling `mpz_init`
    * calling `mpz_free`
    * free the memory
    * Measure the operations-per-seconds, **higher ops is better**
  * dowork contains following actions:
    * create a `GmpFloat` to `1.5` with precision=1000(v = 1, a = 1.5)
    * Calling `MultiplyInplace`(v *= a) `10*1024*1024` times
    * Measure the duration, **lower is better**
  
  Here is the tested results in my laptop:
  
  | case/senario      | init & dispose | dowork |
  | ----------------- | -------------- | ------ |
  | Struct in class   | 82,055,792 ops | 1237ms |
  | Raw memory IntPtr | 15,543,619 ops | 1134ms |

  As you can see, raw memory IntPtr design will benifits **~8.33%** faster in `dowork` senario above, but struct in class design will be **5.2x faster** in `init & dispose` senario.

  Finally I choosed the **struct in class** design, here is some existing Raw memory IntPtr design work if you also wants to check or test:
  * branch: https://github.com/sdcb/Sdcb.Arithmetic/tree/feature/gmp-raw-ptr
  * nuget-package: https://www.nuget.org/packages/Sdcb.Arithmetic.Gmp/1.0.10-preview.12

  Struct in class performance results environment:
  * commit: https://github.com/sdcb/Sdcb.Arithmetic/tree/976d4271c487a554be936cf56ed55f2b1314042e
  * nuget-package: https://www.nuget.org/packages/Sdcb.Arithmetic.Gmp/1.0.10-preview.11

  In the future, `Raw memory IntPtr` design can be pick-up if a handy, good performance memory allocator was found.
