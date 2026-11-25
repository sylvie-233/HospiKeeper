using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Domain.Entities;

/// <summary>操作员账号。</summary>
public sealed class AppUser
{
    public int Id { get; set; }

    /// <summary>登录账号，创建后不建议修改。</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>SHA-256 十六进制密码摘要，不保存明文。</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Front;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>是否管理员。</summary>
    public bool IsAdmin => Role == UserRole.Admin;

    /// <summary>角色的中文名称。</summary>
    public string RoleDisplay => Role == UserRole.Admin ? "管理员" : "前台";

    /// <summary>日志与界面展示用的「姓名（角色）」。</summary>
    public string DisplayName => $"{RealName}（{RoleDisplay}）";
}
