using System.Collections.Concurrent;
using System.Security.Cryptography;
using HospiKeeper.Application.Common;

namespace HospiKeeper.Api.Security;

/// <summary>
/// 轻量会话令牌（内存）。桌面端单机使用；生产多实例可替换为 JWT / Redis。
/// </summary>
public sealed class ApiTokenStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(12);
    private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new(StringComparer.Ordinal);
    private readonly IClock _clock;

    public ApiTokenStore(IClock clock) => _clock = clock;

    /// <summary>签发 32 字节随机令牌并写入内存会话。</summary>
    public (string Token, DateTime ExpiresAt) Issue(int userId)
    {
        PurgeExpired();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = _clock.Now.Add(Lifetime);
        _tokens[token] = new TokenEntry(userId, expiresAt);
        return (token, expiresAt);
    }

    /// <summary>校验令牌是否存在且未过期。</summary>
    public bool TryValidate(string token, out int userId)
    {
        userId = 0;
        if (!_tokens.TryGetValue(token, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= _clock.Now)
        {
            _tokens.TryRemove(token, out _);
            return false;
        }

        userId = entry.UserId;
        return true;
    }

    /// <summary>主动吊销令牌（退出登录或账号失效）。</summary>
    public void Revoke(string token) => _tokens.TryRemove(token, out _);

    private void PurgeExpired()
    {
        var now = _clock.Now;
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
