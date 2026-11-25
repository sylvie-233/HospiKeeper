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

    /// <summary>游客在线预订填写的身份证号（选填）。</summary>
    public string? GuestIdCard { get; set; }

    /// <summary>到店分配的房间；游客在线预订提交时为 <c>null</c>。</summary>
    public int? RoomId { get; set; }

    /// <summary>房号；游客在线预订提交时为 <c>null</c>，转入住时回填。</summary>
    public string? RoomNo { get; set; }

    /// <summary>预订锁定的房型。</summary>
    public int TypeId { get; set; }

    /// <summary>房型名称冗余，便于列表直接展示。</summary>
    public string RoomTypeName { get; set; } = string.Empty;

    /// <summary>入住日期（仅日期部分有效）。</summary>
    public DateTime CheckInDate { get; set; }

    /// <summary>退房日期（仅日期部分有效，必须晚于入住日期）。</summary>
    public DateTime CheckOutDate { get; set; }

    /// <summary>预订渠道：前台代客 / 游客 Web。</summary>
    public ReservationChannel Channel { get; set; } = ReservationChannel.WalkIn;

    /// <summary>下单时的房型单价快照；游客 Web 预订用于展示预估房费。</summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>预计到店时间；Web 预订默认入住日 14:00。</summary>
    public DateTime ArriveTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public int? CheckInId { get; set; }
    public string? CheckInNo { get; set; }

    /// <summary>转入住后的入住单导航属性，仅用于 EF 关系维护，不参与 JSON 序列化。</summary>
    [JsonIgnore]
    public CheckInRecord? CheckIn { get; set; }
    public string? Note { get; set; }
    public int? OpUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
