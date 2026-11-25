using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace HospiKeeper.Api.Security;

/// <summary>
/// 轻量会话令牌（内存）。桌面端单机使用；生产多实例可替换为 JWT / Redis。
/// </summary>
public sealed class ApiTokenStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(12);
    private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new(StringComparer.Ordinal);

    public (string Token, DateTime ExpiresAt) Issue(int userId)
    {
        PurgeExpired();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTime.Now.Add(Lifetime);
        _tokens[token] = new TokenEntry(userId, expiresAt);
        return (token, expiresAt);
    }

    public bool TryValidate(string token, out int userId)
    {
        userId = 0;
        if (!_tokens.TryGetValue(token, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= DateTime.Now)
        {
            _tokens.TryRemove(token, out _);
            return false;
        }

        userId = entry.UserId;
        return true;
    }

    public void Revoke(string token) => _tokens.TryRemove(token, out _);

    private void PurgeExpired()
    {
        var now = DateTime.Now;
        foreach (var pair in _tokens)
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _tokens.TryRemove(pair.Key, out _);
            }
        }
    }

    private sealed record TokenEntry(int UserId, DateTime ExpiresAt);
}
