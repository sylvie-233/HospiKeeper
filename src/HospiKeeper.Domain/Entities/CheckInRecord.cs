using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>入住单：散客入住、在住、历史记录。</summary>
public sealed class CheckInRecord
{
    public int Id { get; set; }
    public string CheckInNo { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string GuestIdCard { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime PlanOutTime { get; set; }
    public DateTime? ActualOutTime { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginPrice { get; set; }
    public decimal Deposit { get; set; }
    public int ExtendHours { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.Living;
    public string? Note { get; set; }
    public int? OpUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Companion> Companions { get; set; } = new();
    public List<ExtraCharge> ExtraCharges { get; set; } = new();

    /// <summary>入住期间追加消费合计。</summary>
    public decimal ExtraTotal => ExtraCharges.Sum(x => x.Amount);
}
