using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Mapping;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Services;

/// <summary>登录、会话与账号管理用例实现。</summary>
public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;
    private readonly IOperationLogger _logger;
    private readonly IClock _clock;

    public AuthService(IUnitOfWork unitOfWork, SessionContext session, IOperationLogger logger, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _session = session;
        _logger = logger;
        _clock = clock;
    }

    public async Task<LoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var name = username?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            return LoginResult.Fail("请输入账号和密码。");
        }

        var user = await _unitOfWork.Users.FindByUsernameAsync(name, cancellationToken);
        if (user is null)
        {
            return LoginResult.Fail("账号不存在。");
        }

        if (user.Status == AccountStatus.Disabled)
        {
            return LoginResult.Fail("账号已停用，请联系管理员。");
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
            return LoginResult.Fail("密码错误，请重试。");
        }

        _session.SignIn(user);
        try
        {
            user.LastLogin = _clock.Now;
            _logger.Append(OperationActions.Login, user.Username, "登录成功");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _session.SignOut();
            throw;
        }

        return LoginResult.Ok(user.ToDto());
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_session.CurrentUser is { } user)
            {
                await _logger.LogAsync(OperationActions.Logout, user.Username, "退出登录", null, cancellationToken);
            }
        }
        finally
        {
            _session.SignOut();
        }
    }

    public async Task ChangeOwnPasswordAsync(
        string oldPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var current = _session.RequireUser();
        InputGuard.Ensure(!string.IsNullOrEmpty(oldPassword), "请输入原密码。");
        InputGuard.Ensure(newPassword == confirmPassword, "两次输入的新密码不一致。");
        InputGuard.Ensure(newPassword is { Length: >= 6 }, "新密码至少 6 位。");
        InputGuard.Ensure(oldPassword != newPassword, "新密码不能与原密码相同。");

        var user = await _unitOfWork.Users.GetByIdAsync(current.Id, cancellationToken)
                   ?? throw new DomainException("账号不存在。");

        if (!PasswordHasher.Verify(oldPassword, user.PasswordHash))
        {
            throw new DomainException("原密码错误。");
        }

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        _logger.Append(OperationActions.Password, user.Username, "修改登录密码");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users
            .OrderByDescending(u => u.Role)
            .ThenBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .Select(u => u.ToDto())
            .ToList();
    }

    public async Task<UserDto> CreateUserAsync(AccountInput input, CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var username = InputGuard.Text(input.Username, "登录账号", 32);
        var realName = InputGuard.Text(input.RealName, "姓名", 32);
        InputGuard.Ensure(input.Password is { Length: >= 6 }, "初始密码至少 6 位。");
        InputGuard.Ensure(!await _unitOfWork.Users.UsernameExistsAsync(username, null, cancellationToken),
            "登录账号已存在，请更换。");

        var user = new AppUser
        {
            Username = username,
            RealName = realName,
            Role = input.Role,
            Status = input.Status,
            PasswordHash = PasswordHasher.Hash(input.Password!),
            CreatedAt = _clock.Now
        };

        _unitOfWork.Users.Add(user);
        _logger.Append(OperationActions.Account, username, $"新增账号 {realName}（{user.RoleDisplay}）");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task UpdateUserAsync(
        int userId,
        AccountInput input,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var username = InputGuard.Text(input.Username, "登录账号", 32);
        var realName = InputGuard.Text(input.RealName, "姓名", 32);
        if (!string.IsNullOrEmpty(input.Password) && input.Password.Length < 6)
        {
            throw new DomainException("新密码至少 6 位。");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new DomainException("账号不存在。");
        var isSelf = user.Id == _session.UserId;

        if (isSelf && (user.Role != input.Role || user.Status != input.Status))
        {
            throw new DomainException("不能修改当前登录账号的角色或状态。");
        }

        if (await _unitOfWork.Users.UsernameExistsAsync(username, userId, cancellationToken))
        {
            throw new DomainException("登录账号已存在，请更换。");
        }

        var wasActiveAdmin = user.Role == UserRole.Admin && user.Status == AccountStatus.Active;
        var willBeActiveAdmin = input.Role == UserRole.Admin && input.Status == AccountStatus.Active;
        if (wasActiveAdmin && !willBeActiveAdmin)
        {
            await EnsureAnotherActiveAdminAsync(user.Id, cancellationToken);
        }

        var oldDisplay = user.DisplayName;
        user.Username = username;
        user.RealName = realName;
        user.Role = input.Role;
        user.Status = input.Status;
        if (!string.IsNullOrEmpty(input.Password))
        {
            user.PasswordHash = PasswordHasher.Hash(input.Password);
        }

        _logger.Append(OperationActions.Account, username,
            $"修改账号资料：{oldDisplay} → {user.DisplayName}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetUserStatusAsync(
        int userId,
        AccountStatus status,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new DomainException("账号不存在。");

        if (user.Id == _session.UserId)
        {
            throw new ForbiddenException("不能停用当前登录账号。");
        }

        if (status == user.Status)
        {
            return;
        }

        if (status == AccountStatus.Disabled && user.Role == UserRole.Admin)
        {
            await EnsureAnotherActiveAdminAsync(user.Id, cancellationToken);
        }

        user.Status = status;
        var action = status == AccountStatus.Active ? OperationActions.Account : OperationActions.Disable;
        _logger.Append(action, user.Username, status == AccountStatus.Active ? "启用账号" : "停用账号");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new DomainException("账号不存在。");

        if (user.Id == _session.UserId)
        {
            throw new ForbiddenException("不能删除当前登录账号。");
        }
        if (user.Role == UserRole.Admin && user.Status == AccountStatus.Active)
        {
            await EnsureAnotherActiveAdminAsync(user.Id, cancellationToken);
        }

        _unitOfWork.Users.Remove(user);
        _logger.Append(OperationActions.Account, user.Username, $"删除账号 {user.DisplayName}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<AppUser?> FindUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        => _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

    private async Task EnsureAnotherActiveAdminAsync(int excludeUserId, CancellationToken cancellationToken)
    {
        var activeAdmins = await _unitOfWork.Users.CountActiveAdminsAsync(excludeUserId, cancellationToken);
        if (activeAdmins == 0)
        {
            throw new DomainException("系统至少需要保留一名启用状态的管理员。");
        }
    }
}