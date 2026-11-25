using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>预定的对外视图（不含 EF 导航属性）。</summary>
public sealed record ReservationDto(
    int Id,
    string ResNo,
    string GuestName,
    string GuestPhone,
    int RoomId,
    string RoomNo,
    string RoomTypeName,
    DateTime ArriveTime,
    ReservationStatus Status,
    int? CheckInId,
    string? CheckInNo,
    string? Note,
    int? OpUserId,
    string OperatorName,
    DateTime CreatedAt);
