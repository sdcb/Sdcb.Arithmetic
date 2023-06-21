using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sdcb.Arithmetic.Mpfr.Tests;

public class RawApiTest
{
    private readonly ITestOutputHelper _console;

    public RawApiTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public unsafe void InitClose()
    {
        Mpfr_t t = new();
        MpfrLib.mpfr_init2((IntPtr)(&t), 128);
        //MpfrLib.mpfr_clear((IntPtr)(&t));
        _console.WriteLine("Good!");
    }
}
