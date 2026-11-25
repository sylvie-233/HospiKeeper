using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AppUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null, CancellationToken cancellationToken = default);

    Task<int> CountActiveAdminsAsync(int? excludeUserId = null, CancellationToken cancellationToken = default);

    void Add(AppUser user);

    void Remove(AppUser user);
}
