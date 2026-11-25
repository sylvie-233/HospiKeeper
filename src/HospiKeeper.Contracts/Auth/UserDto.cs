using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

/// <summary>操作员账号的对外视图（不含密码散列）。</summary>
public sealed record UserDto(
    int Id,
    string Username,
    string RealName,
    UserRole Role,
    AccountStatus Status,
    DateTime? LastLogin,
    string DisplayName,
    bool IsAdmin);
