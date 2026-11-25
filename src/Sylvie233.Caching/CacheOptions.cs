namespace Sylvie233.Caching;

public sealed class CacheOptions
{
    /// <summary>Memory 适合单进程调试；Redis 适合多实例共享缓存。</summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>键前缀，用于隔离不同环境 / 应用。</summary>
    public string InstanceName { get; set; } = "hospikeeper:";

    public int DefaultExpirationSeconds { get; set; } = 60;

    public string RedisConnectionString { get; set; } = "localhost:6379";

    public int RedisDatabase { get; set; }

    internal TimeSpan DefaultExpiration =>
        TimeSpan.FromSeconds(DefaultExpirationSeconds <= 0 ? 60 : DefaultExpirationSeconds);
}
