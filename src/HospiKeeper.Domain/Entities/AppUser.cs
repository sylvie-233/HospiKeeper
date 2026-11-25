using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>操作员账号。</summary>
public sealed class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Front;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsAdmin => Role == UserRole.Admin;
    public string RoleDisplay => Role == UserRole.Admin ? "管理员" : "前台";
    public string DisplayName => $"{RealName}（{RoleDisplay}）";
}
