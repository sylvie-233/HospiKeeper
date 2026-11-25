using System.Security.Cryptography;
using System.Text;

namespace HospiKeeper.Application.Common;

/// <summary>与数据库文档保持一致的 SHA-256 十六进制摘要。</summary>
public static class PasswordHasher
{
    /// <summary>计算密码摘要（SHA-256 小写十六进制）。</summary>
    public static string Hash(string plainText)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>以固定时间比较语义校验密码，忽略大小写。</summary>
    public static bool Verify(string plainText, string hash)
        => string.Equals(Hash(plainText), hash, StringComparison.OrdinalIgnoreCase);
}
