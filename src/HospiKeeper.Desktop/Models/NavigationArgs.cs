namespace HospiKeeper.Desktop.Models;

/// <summary>从看板 / 预定页跳转到入住登记时携带的预填信息。</summary>
public sealed record CheckInPrefill(int? RoomId = null, int? ReservationId = null);

/// <summary>跳转到在住列表时按房号过滤。</summary>
public sealed record StayFilter(string? Keyword);
