using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Desktop.Services;

/// <summary>桌面端当前登录用户（真实权限仍由 API 端校验）。</summary>
public sealed class ClientSession
{
    public UserDto? User { get; private set; }

    public bool IsAuthenticated => User is not null;

    public bool IsAdmin => User?.Role == UserRole.Admin;

    public int UserId => User?.Id ?? 0;

    public string DisplayName => User?.DisplayName ?? "未登录";

    public string UserInitial => string.IsNullOrWhiteSpace(User?.RealName) ? "?" : User!.RealName[..1];

    public string RoleText => IsAdmin ? "管理员" : "前台操作员";

    public void SignIn(UserDto user) => User = user;

    public void SignOut() => User = null;
}
