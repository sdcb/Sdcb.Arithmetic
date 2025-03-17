using System;
using System.Text;

namespace Sdcb.Arithmetic.Gmp;

internal static class CStringHelper
{
    /// <summary>
    /// 将普通的 C# 字符串转换为 C 语言风格的以 null 结尾的 UTF-8 字节数组。
    /// </summary>
    /// <param name="str">输入字符串</param>
    /// <returns>包含 UTF-8 编码内容和末尾 null 字节的 byte[]</returns>
    public static byte[] ToCString(string str)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));

        // 计算 string 编码为 UTF-8 所需的字节数
        int byteCount = Encoding.UTF8.GetByteCount(str);

        // 分配数组，多分配一个字节用于存放 '\0'
        byte[] result = new byte[byteCount + 1];

        // 将字符串转换为 UTF-8 编码, 写入数组中
        Encoding.UTF8.GetBytes(str, 0, str.Length, result, 0);

        // 默认分配的数组内容初始为 0, 因此最后一个字节已经是 '\0'
        return result;
    }
}