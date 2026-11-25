namespace HospiKeeper.Domain.Entities;

/// <summary>房型（单人间 / 双人间 / 大床房 ...）。</summary>
public sealed class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
