using System.Collections.Concurrent;
using Sylvie233.Caching.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sylvie233.Caching;

/// <summary>基于 <see cref="IMemoryCache"/> 的实现，额外维护 key 注册表以支持前缀清理。</summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public MemoryCacheService(
        IMemoryCache cache,
        IOptions<CacheOptions> options,
        ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        if (!_cache.TryGetValue(fullKey, out byte[]? bytes) || bytes is null)
        {
            return Task.FromResult<T?>(null);
        }

        try
        {
            return Task.FromResult(CacheSerializer.Deserialize<T>(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Memory cache deserialization failed for key {CacheKey}; entry removed.", fullKey);
            _cache.Remove(fullKey);
            _keys.TryRemove(fullKey, out _);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.DefaultExpiration
        };
        entryOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string text)
            {
                _keys.TryRemove(text, out _);
            }
        });

        _cache.Set(fullKey, CacheSerializer.Serialize(value), entryOptions);
        _keys[fullKey] = 0;
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var fullKey = BuildKey(key);
        var gate = _locks.GetOrAdd(fullKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            var value = await factory(cancellationToken);
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
            return value;
        }
        finally
        {
            gate.Release();
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        _cache.Remove(fullKey);
        _keys.TryRemove(fullKey, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPrefix = BuildKey(prefix);
        foreach (var key in _keys.Keys.Where(key => key.StartsWith(fullPrefix, StringComparison.Ordinal)).ToList())
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private string BuildKey(string key) => $"{_options.InstanceName}{key}";
}
