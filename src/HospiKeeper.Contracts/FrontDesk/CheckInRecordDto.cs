using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>同住人的对外视图。</summary>
public sealed record CompanionDto(
    int Id,
    int CheckInId,
    string Name,
    string? Phone,
    string? IdCard);

/// <summary>追加消费的对外视图。</summary>
public sealed record ExtraChargeDto(
    int Id,
    int CheckInId,
    string Item,
    decimal Amount,
    DateTime ChargeTime,
    int? OpUserId,
    string OperatorName);

/// <summary>入住记录的对外视图，包含同住人与消费明细。</summary>
public sealed record CheckInRecordDto(
    int Id,
    string CheckInNo,
    int RoomId,
    string RoomNo,
    string RoomTypeName,
    string GuestName,
    string GuestPhone,
    string GuestIdCard,
    DateTime CheckInTime,
    DateTime PlanOutTime,
    DateTime? ActualOutTime,
    decimal Price,
    decimal? OriginPrice,
    decimal Deposit,
    int ExtendHours,
    CheckInStatus Status,
    string? Note,
    int? OpUserId,
    string OperatorName,
    DateTime CreatedAt,
    IReadOnlyList<CompanionDto> Companions,
    IReadOnlyList<ExtraChargeDto> ExtraCharges,
    decimal ExtraTotal);
