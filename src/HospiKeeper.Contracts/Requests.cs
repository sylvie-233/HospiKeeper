using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

public sealed record CompanionInput(string Name, string? Phone, string? IdCard);

public sealed record CheckInRequest(
    int RoomId,
    string GuestName,
    string GuestPhone,
    string GuestIdCard,
    DateTime CheckInTime,
    DateTime PlanOutTime,
    decimal Price,
    decimal Deposit,
    string? Note,
    int? ReservationId = null,
    List<CompanionInput>? Companions = null);

public sealed record ReservationRequest(
    string GuestName,
    string GuestPhone,
    int RoomId,
    DateTime ArriveTime,
    string? Note);

public sealed record CheckoutRequest(
    int CheckInId,
    DateTime ActualOutTime,
    decimal Damage,
    string? Note,
    BillingMode Mode);

public sealed record ExtraChargeRequest(int CheckInId, string Item, decimal Amount);

public sealed record ExtraChargeInput(string Item, decimal Amount);

public sealed record RoomInput(
    string RoomNo,
    int Floor,
    int TypeId,
    int Beds,
    decimal Price,
    decimal MinPrice,
    decimal Deposit,
    bool HasWindow,
    bool HasBath,
    string? Note);

public sealed record RoomTypeInput(string Name, decimal BasePrice, string? Note);

public sealed record AccountInput(
    string Username,
    string RealName,
    UserRole Role,
    AccountStatus Status,
    string? Password);

public sealed record RecordQuery(string? RoomNo, string? Keyword, DateTime? From, DateTime? To);
