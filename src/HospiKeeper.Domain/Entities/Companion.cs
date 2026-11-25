namespace HospiKeeper.Domain.Entities;

/// <summary>同住人登记。</summary>
public sealed class Companion
{
    public int Id { get; set; }
    public int CheckInId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
}
