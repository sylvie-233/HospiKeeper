using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>操作员账号查询与变更。</summary>
public interface IUserRepository
{
    /// <summary>查询全部账号。</summary>
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>按登录账号查询。</summary>
    Task<AppUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询。</summary>
    Task<AppUser?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>判断登录账号是否已存在。</summary>
    Task<bool> UsernameExistsAsync(
        string username,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>统计启用中的管理员数量，用于保证至少保留一名管理员。</summary>
    Task<int> CountActiveAdminsAsync(
        int? excludeUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(AppUser user);

    /// <summary>标记删除，等待统一保存。</summary>
    void Remove(AppUser user);
}
