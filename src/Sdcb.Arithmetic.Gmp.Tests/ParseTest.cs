using System.Text;

namespace Sdcb.Arithmetic.Gmp.Tests;

public class ParseTest
{
    [Fact]
    public void Parse100RandomNumbers_GmpFloat()
    {
        var random = new Random();
        for (int i = 0; i < 100; i++)
        {
            int length = random.Next(1, 100);
            var sb = new StringBuilder();
            sb.Append(random.Next(1, 10));
            for (int j = 1; j < length; j++)
            {
                sb.Append(random.Next(0, 10));
            }

            var str = sb.ToString();

            using GmpFloat gf = GmpFloat.Parse(str);
            Assert.Equal(str, gf.ToString());
        }
    }

    [Fact]
    public void Parse100RandomNumbers_GmpInteger()
    {
        var random = new Random();
        for (int i = 0; i < 100; i++)
        {
            int length = random.Next(1, 100);
            var sb = new StringBuilder();
            sb.Append(random.Next(1, 10));
            for (int j = 1; j < length; j++)
            {
                sb.Append(random.Next(0, 10));
            }

            var str = sb.ToString();

            using GmpInteger gi = GmpInteger.Parse(str);
            Assert.Equal(str, gi.ToString());
        }
    }
}
