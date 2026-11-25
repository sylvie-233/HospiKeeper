using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>账单中的消费明细行（快照）。</summary>
public sealed record BillLineDto(int Id, int CheckoutBillId, string Item, decimal Amount);

/// <summary>退房结算账单的对外视图。</summary>
public sealed record CheckoutBillDto(
    BillingMode Mode,
    int Id,
    string BillNo,
    int CheckInId,
    string CheckInNo,
    int RoomId,
    string RoomNo,
    string RoomTypeName,
    string GuestName,
    DateTime CheckInTime,
    DateTime CheckoutTime,
    int Nights,
    decimal RoomFee,
    decimal ExtraTotal,
    decimal Damage,
    decimal OtherFee,
    decimal DepositReceived,
    decimal DepositUsed,
    decimal Total,
    decimal Diff,
    string? Note,
    int? OpUserId,
    string OperatorName,
    DateTime CreatedAt,
    IReadOnlyList<BillLineDto> Extras);
