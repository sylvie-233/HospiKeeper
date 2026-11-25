namespace HospiKeeper.Domain.Entities;

/// <summary>操作日志：谁在什么时候做了什么。</summary>
public sealed class OperationLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Target { get; set; }
    public string? Detail { get; set; }
    public string? BizNo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
