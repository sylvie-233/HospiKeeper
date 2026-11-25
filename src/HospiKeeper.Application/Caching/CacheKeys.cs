using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Caching;

/// <summary>业务缓存 key 的唯一来源。</summary>
public static class CacheKeys
{
    public const string Board = "board";
    public const string Counts = "counts";
    public const string StaysAll = "stays:all";
    public const string ReservationsPrefix = "reservations:";
    public const string RoomsPrefix = "rooms:";
    public const string FreeRooms = "rooms:free";
    public const string Floors = "rooms:floors";
    public const string RoomTypes = "roomtypes:all";
    public const string ReportSnapshot = "reports:snapshot";
    public const string ReportRangePrefix = "reports:range:";

    public static string Rooms(int? floor, int? typeId, RoomStatus? status)
        => $"rooms:{floor?.ToString() ?? "all"}:{typeId?.ToString() ?? "all"}:{status?.ToString() ?? "all"}";

    public static string Reservations(ReservationStatus? status)
        => $"{ReservationsPrefix}{status?.ToString() ?? "all"}";

    public static string ReportRange(DateTime start, DateTime end)
        => $"{ReportRangePrefix}{start:yyyyMMdd}-{end:yyyyMMdd}";
}
