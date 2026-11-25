using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>预定的对外视图（不含 EF 导航属性）。</summary>
public sealed record ReservationDto(
    int Id,
    string ResNo,
    string GuestName,
    string GuestPhone,
    string? GuestIdCard,
    int? RoomId,
    string? RoomNo,
    int TypeId,
    string RoomTypeName,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    ReservationChannel Channel,
    decimal? UnitPrice,
    DateTime ArriveTime,
    ReservationStatus Status,
    int? CheckInId,
    string? CheckInNo,
    string? Note,
    int? OpUserId,
    string OperatorName,
    DateTime CreatedAt);
