namespace HospiKeeper.Application.Caching;

/// <summary>按数据实时性要求集中定义缓存时长。</summary>
public static class CacheDurations
{
    /// <summary>房态看板缓存时长。</summary>
    public static readonly TimeSpan Board = TimeSpan.FromSeconds(15);
    /// <summary>房态汇总缓存时长。</summary>
    public static readonly TimeSpan Counts = TimeSpan.FromSeconds(15);
    /// <summary>在住列表缓存时长。</summary>
    public static readonly TimeSpan Stays = TimeSpan.FromSeconds(15);
    /// <summary>预定列表缓存时长。</summary>
    public static readonly TimeSpan Reservations = TimeSpan.FromSeconds(30);
    /// <summary>房间列表缓存时长。</summary>
    public static readonly TimeSpan Rooms = TimeSpan.FromSeconds(60);
    /// <summary>房型列表缓存时长。</summary>
    public static readonly TimeSpan RoomTypes = TimeSpan.FromSeconds(60);
    /// <summary>报表缓存时长。</summary>
    public static readonly TimeSpan Reports = TimeSpan.FromSeconds(60);

    /// <summary>游客端房型可订量缓存时长。</summary>
    public static readonly TimeSpan GuestAvailability = TimeSpan.FromSeconds(15);

    /// <summary>游客端首页统计缓存时长。</summary>
    public static readonly TimeSpan GuestSummary = TimeSpan.FromSeconds(60);
}
