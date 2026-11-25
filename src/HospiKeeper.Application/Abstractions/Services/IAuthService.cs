using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>登录、会话与操作员账号管理用例。</summary>
public interface IAuthService
{
    /// <summary>校验账号密码并写入当前会话。</summary>
    Task<LoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>清理当前会话并记录退出日志。</summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>当前登录账号修改自己的密码。</summary>
    Task ChangeOwnPasswordAsync(
        string oldPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default);

    /// <summary>查询全部账号（仅管理员）。</summary>
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>创建账号（仅管理员）。</summary>
    Task<UserDto> CreateUserAsync(AccountInput input, CancellationToken cancellationToken = default);

    /// <summary>修改账号资料（仅管理员）。</summary>
    Task UpdateUserAsync(int userId, AccountInput input, CancellationToken cancellationToken = default);

    /// <summary>启用 / 停用账号（仅管理员，不能停用当前账号）。</summary>
    Task SetUserStatusAsync(int userId, AccountStatus status, CancellationToken cancellationToken = default);

    /// <summary>删除账号（仅管理员，不能删除当前账号）。</summary>
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>供 API 鉴权中间件按令牌还原用户。</summary>
    Task<AppUser?> FindUserByIdAsync(int userId, CancellationToken cancellationToken = default);
}
