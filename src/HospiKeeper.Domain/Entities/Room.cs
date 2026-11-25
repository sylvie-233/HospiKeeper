using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>房间（房态看板核心）。</summary>
public sealed class Room
{
    public int Id { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public int Floor { get; set; } = 1;
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Beds { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal MinPrice { get; set; }
    public decimal Deposit { get; set; }
    public bool HasWindow { get; set; } = true;
    public bool HasBath { get; set; } = true;
    public RoomStatus Status { get; set; } = RoomStatus.Free;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>停用与维修房不计入可售房分母。</summary>
    public bool IsSellable => Status is not RoomStatus.Disabled and not RoomStatus.Repair;
}
