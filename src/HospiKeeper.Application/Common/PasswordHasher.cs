using System.Security.Cryptography;
using System.Text;

namespace HospiKeeper.Application.Common;

/// <summary>与数据库文档保持一致的 SHA-256 十六进制摘要。</summary>
public static class PasswordHasher
{
    public static string Hash(string plainText)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Verify(string plainText, string hash)
        => string.Equals(Hash(plainText), hash, StringComparison.OrdinalIgnoreCase);
}
