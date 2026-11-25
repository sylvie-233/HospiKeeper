using HospiKeeper.Application.Abstractions.Caching;
using Sylvie233.Caching;

namespace HospiKeeper.Application.Caching;

/// <summary>按业务动作集中失效缓存，避免在服务中出现零散的 key 清理代码。</summary>
public sealed class HotelCacheInvalidator : IHotelCacheInvalidator
{
    private readonly ICacheService _cache;

    public HotelCacheInvalidator(ICacheService cache) => _cache = cache;

    /// <summary>房间 / 房型变更：清理房间、房型及所有运营类缓存。</summary>
    public async Task InvalidateRoomsAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveByPrefixAsync(CacheKeys.RoomsPrefix, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomTypes, cancellationToken);
        await InvalidateOperationalAsync(cancellationToken);
    }

    /// <summary>入住 / 退房 / 预定变更：清理房间、在住、预定及运营类缓存。</summary>
    public async Task InvalidateFrontDeskAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveByPrefixAsync(CacheKeys.RoomsPrefix, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.StaysAll, cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.ReservationsPrefix, cancellationToken);
        await InvalidateOperationalAsync(cancellationToken);
    }

    private async Task InvalidateOperationalAsync(CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.Board, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Counts, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.ReportSnapshot, cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.ReportRangePrefix, cancellationToken);
    }
}
