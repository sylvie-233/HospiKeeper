namespace Sylvie233.Caching;

/// <summary>
/// 应用级缓存端口。实现可选择内存或 Redis，业务层只依赖此接口。
/// </summary>
public interface ICacheService
{
    /// <summary>读取缓存；未命中或缓存不可用时返回 <c>null</c>。</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>写入缓存，可指定绝对过期时间。</summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 读取缓存；未命中时执行 factory 并写回。同一进程内对同一 key 做防击穿保护。
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>删除单个 key。</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>按前缀批量删除（用于一组相关缓存统一失效）。</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
