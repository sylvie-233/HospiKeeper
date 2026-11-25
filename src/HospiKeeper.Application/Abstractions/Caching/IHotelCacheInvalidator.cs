namespace HospiKeeper.Application.Abstractions.Caching;

/// <summary>集中维护酒店业务缓存失效策略，避免业务代码散落缓存 key。</summary>
public interface IHotelCacheInvalidator
{
    /// <summary>房间 / 房型变更：房态看板、房间列表、统计报表均可能变化。</summary>
    Task InvalidateRoomsAsync(CancellationToken cancellationToken = default);

    /// <summary>入住 / 退房 / 预定 / 追加消费等前台业务变更。</summary>
    Task InvalidateFrontDeskAsync(CancellationToken cancellationToken = default);
}
