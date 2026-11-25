namespace Sylvie233.Caching;

/// <summary>缓存模块配置：实现类型、key 前缀、默认 TTL 与 Redis 连接。</summary>
public sealed class CacheOptions
{
    /// <summary>Memory 适合单进程调试；Redis 适合多实例共享缓存。</summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>键前缀，用于隔离不同环境 / 应用。</summary>
    public string InstanceName { get; set; } = "hospikeeper:";

    /// <summary>未显式指定 TTL 时的默认过期秒数。</summary>
    public int DefaultExpirationSeconds { get; set; } = 60;

    /// <summary>Redis 连接串，例如 <c>localhost:6379</c>。</summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>Redis 逻辑库编号。</summary>
    public int RedisDatabase { get; set; }

    internal TimeSpan DefaultExpiration =>
        TimeSpan.FromSeconds(DefaultExpirationSeconds <= 0 ? 60 : DefaultExpirationSeconds);
}
