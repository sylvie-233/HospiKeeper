using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Caching;

/// <summary>业务缓存 key 的唯一来源。</summary>
public static class CacheKeys
{
    /// <summary>房态看板。</summary>
    public const string Board = "board";

    /// <summary>前台房态汇总。</summary>
    public const string Counts = "counts";

    /// <summary>在住列表。</summary>
    public const string StaysAll = "stays:all";

    /// <summary>预定列表前缀（按状态区分）。</summary>
    public const string ReservationsPrefix = "reservations:";

    /// <summary>房间相关缓存前缀（列表 / 空闲房 / 楼层 / 详情）。</summary>
    public const string RoomsPrefix = "rooms:";

    /// <summary>空闲房间。</summary>
    public const string FreeRooms = "rooms:free";

    /// <summary>楼层列表。</summary>
    public const string Floors = "rooms:floors";

    /// <summary>房型列表。</summary>
    public const string RoomTypes = "roomtypes:all";

    /// <summary>报表概览。</summary>
    public const string ReportSnapshot = "reports:snapshot";

    /// <summary>区间营收前缀。</summary>
    public const string ReportRangePrefix = "reports:range:";

    /// <summary>系统静态配置（Redis 持久 key，无 TTL）。</summary>
    public const string SiteSettings = "config:site";

    /// <summary>游客端首页统计。</summary>
    public const string GuestSummary = "guest:summary";

    /// <summary>游客端房型可订量前缀。</summary>
    public const string GuestAvailabilityPrefix = "guest:availability:";

    /// <summary>按筛选条件生成房间列表 key。</summary>
    public static string Rooms(int? floor, int? typeId, RoomStatus? status)
        => $"rooms:{floor?.ToString() ?? "all"}:{typeId?.ToString() ?? "all"}:{status?.ToString() ?? "all"}";

    /// <summary>按状态生成预定列表 key。</summary>
    public static string Reservations(ReservationStatus? status)
        => $"{ReservationsPrefix}{status?.ToString() ?? "all"}";

    /// <summary>按日期区间生成营收统计 key。</summary>
    public static string ReportRange(DateTime start, DateTime end)
        => $"{ReportRangePrefix}{start:yyyyMMdd}-{end:yyyyMMdd}";

    /// <summary>按日期区间生成游客房型可订量 key。</summary>
    public static string GuestAvailability(DateTime checkIn, DateTime checkOut)
        => $"{GuestAvailabilityPrefix}{checkIn:yyyyMMdd}-{checkOut:yyyyMMdd}";
}
