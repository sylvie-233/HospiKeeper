using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>退房结算账单（含追加消费快照明细，便于票据展示）。</summary>
public sealed class CheckoutBill
{
    /// <summary>计费方式；Nights 在加钟模式下表示小时数。</summary>
    public BillingMode Mode { get; set; } = BillingMode.Daily;

    public int Id { get; set; }
    public string BillNo { get; set; } = string.Empty;
    public int CheckInId { get; set; }
    public string CheckInNo { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomNo { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime CheckoutTime { get; set; }
    public int Nights { get; set; } = 1;
    public decimal RoomFee { get; set; }
    public decimal ExtraTotal { get; set; }
    public decimal Damage { get; set; }
    public decimal OtherFee { get; set; }
    public decimal DepositReceived { get; set; }
    public decimal DepositUsed { get; set; }
    public decimal Total { get; set; }

    /// <summary>&gt;0 应补收，&lt;0 应退款，=0 押金正好抵扣。</summary>
    public decimal Diff { get; set; }

    public string? Note { get; set; }
    public int? OpUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<BillLine> Extras { get; set; } = new();
}

/// <summary>账单中的消费明细行（快照）。</summary>
public sealed class BillLine
{
    public int Id { get; set; }
    public int CheckoutBillId { get; set; }
    public string Item { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
