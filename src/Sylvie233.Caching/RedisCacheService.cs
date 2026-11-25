using System.Collections.Concurrent;
using Sylvie233.Caching.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Sylvie233.Caching;

/// <summary>基于 StackExchange.Redis 的实现；连接异常按缓存未命中降级处理。</summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IDatabase _database;
    private readonly CacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public RedisCacheService(
        IConnectionMultiplexer multiplexer,
        IOptions<CacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _multiplexer = multiplexer;
        _options = options.Value;
        _logger = logger;
        _database = multiplexer.GetDatabase(_options.RedisDatabase);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        try
        {
            var value = await _database.StringGetAsync(fullKey);
            if (!value.HasValue)
            {
                return null;
            }

            var bytes = (byte[]?)value;
            return bytes is null ? null : CacheSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Redis cache read failed for key {CacheKey}; treated as a miss.", fullKey);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        try
        {
            await _database.StringSetAsync(
                fullKey,
                CacheSerializer.Serialize(value),
                absoluteExpiration ?? _options.DefaultExpiration);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Redis cache write failed for key {CacheKey}.", fullKey);
        }
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

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = BuildKey(key);
        try
        {
            await _database.KeyDeleteAsync(fullKey);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Redis cache delete failed for key {CacheKey}.", fullKey);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pattern = $"{BuildKey(prefix)}*";

        try
        {
            foreach (var endpoint in _multiplexer.GetEndPoints())
            {
                var server = _multiplexer.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica)
                {
                    continue;
                }

                var batch = new List<RedisKey>(250);
                await foreach (var key in server.KeysAsync(_options.RedisDatabase, pattern, 250))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    batch.Add(key);
                    if (batch.Count >= 250)
                    {
                        await _database.KeyDeleteAsync(batch.ToArray());
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await _database.KeyDeleteAsync(batch.ToArray());
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Redis cache prefix delete failed for pattern {Pattern}.", pattern);
        }
    }

    private string BuildKey(string key) => $"{_options.InstanceName}{key}";
}
