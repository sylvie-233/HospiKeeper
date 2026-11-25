using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>房间的对外视图。</summary>
public sealed record RoomDto(
    int Id,
    string RoomNo,
    int Floor,
    int TypeId,
    string TypeName,
    int Beds,
    decimal Price,
    decimal MinPrice,
    decimal Deposit,
    bool HasWindow,
    bool HasBath,
    RoomStatus Status,
    string? Note,
    DateTime CreatedAt,
    bool IsSellable);

/// <summary>房型的对外视图。</summary>
public sealed record RoomTypeDto(
    int Id,
    string Name,
    decimal BasePrice,
    string? Note,
    DateTime CreatedAt);
