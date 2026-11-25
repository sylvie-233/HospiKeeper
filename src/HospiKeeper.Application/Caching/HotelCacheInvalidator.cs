using HospiKeeper.Application.Abstractions.Caching;

namespace HospiKeeper.Application.Caching;

public sealed class HotelCacheInvalidator : IHotelCacheInvalidator
{
    private readonly ICacheService _cache;

    public HotelCacheInvalidator(ICacheService cache) => _cache = cache;

    public async Task InvalidateRoomsAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveByPrefixAsync(CacheKeys.RoomsPrefix, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.RoomTypes, cancellationToken);
        await InvalidateOperationalAsync(cancellationToken);
    }

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
