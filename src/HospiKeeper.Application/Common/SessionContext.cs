using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Common;

/// <summary>当前登录会话。UI 与业务服务共用同一份操作员上下文。</summary>
public sealed class SessionContext
{
    public AppUser? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public int UserId => CurrentUser?.Id ?? throw new DomainException("请先登录。");

    public string DisplayName => CurrentUser?.DisplayName ?? "未登录";

    public void SignIn(AppUser user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;

    public AppUser RequireUser()
    {
        return CurrentUser ?? throw new DomainException("登录状态已失效，请重新登录。");
    }

    public void RequireAdmin()
    {
        if (!IsAdmin)
        {
            throw new DomainException("该操作仅管理员可用。");
        }
    }
}
