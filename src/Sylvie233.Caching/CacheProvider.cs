namespace Sylvie233.Caching;

/// <summary>缓存实现类型。</summary>
public enum CacheProvider
{
    /// <summary>进程内内存缓存，适合单实例调试。</summary>
    Memory,

    /// <summary>Redis 缓存，适合多实例共享。</summary>
    Redis
}
