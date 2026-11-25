namespace HospiKeeper.Domain.Entities;

/// <summary>入住期间追加消费（矿泉水、泡面等杂费）。</summary>
public sealed class ExtraCharge
{
    public int Id { get; set; }
    public int CheckInId { get; set; }
    public string Item { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ChargeTime { get; set; } = DateTime.Now;
    public int? OpUserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
}
