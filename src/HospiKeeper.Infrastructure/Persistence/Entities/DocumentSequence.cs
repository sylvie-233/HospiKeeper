namespace HospiKeeper.Infrastructure.Persistence.Entities;

/// <summary>按「单据前缀 + 日期」维护的业务单号序列。</summary>
public sealed class DocumentSequence
{
    public string Prefix { get; set; } = string.Empty;
    public DateTime DocDate { get; set; }
    public int Sequence { get; set; }
}
