using System.Text.Json.Serialization;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>预定单。</summary>
public sealed class Reservation
{
    public int Id { get; set; }
    public string ResNo { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime ArriveTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public int? CheckInId { get; set; }
    public string? CheckInNo { get; set; }

    [JsonIgnore]
    public CheckInRecord? CheckIn { get; set; }
    public string? Note { get; set; }
    public int? OpUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
