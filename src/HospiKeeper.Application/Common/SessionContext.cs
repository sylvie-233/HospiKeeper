using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Common;

/// <summary>当前登录会话。UI 与业务服务共用同一份操作员上下文。</summary>
public sealed class SessionContext
{
    /// <summary>当前登录用户；未登录时为 <c>null</c>。</summary>
    public AppUser? CurrentUser { get; private set; }

    /// <summary>是否已登录。</summary>
    public bool IsAuthenticated => CurrentUser is not null;

    /// <summary>当前用户是否管理员。</summary>
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    /// <summary>当前用户 ID；未登录时抛业务异常。</summary>
    public int UserId => CurrentUser?.Id ?? throw new DomainException("请先登录。");

    /// <summary>当前用户展示名。</summary>
    public string DisplayName => CurrentUser?.DisplayName ?? "未登录";

    /// <summary>登录成功后写入会话。</summary>
    public void SignIn(AppUser user) => CurrentUser = user;

    /// <summary>退出登录时清空会话。</summary>
    public void SignOut() => CurrentUser = null;

    /// <summary>获取当前用户；未登录时抛业务异常。</summary>
    public AppUser RequireUser()
    {
        return CurrentUser ?? throw new DomainException("登录状态已失效，请重新登录。");
    }

    /// <summary>要求当前用户为管理员；否则抛 <see cref="ForbiddenException"/>。</summary>
    public void RequireAdmin()
    {
        if (!IsAdmin)
        {
            throw new ForbiddenException("该操作仅管理员可用。");
        }
    }
}
