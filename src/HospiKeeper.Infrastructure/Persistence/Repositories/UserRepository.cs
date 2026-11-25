using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly HotelDbContext _db;

    public UserRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Users.AsNoTracking().ToListAsync(cancellationToken);

    public Task<AppUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized, cancellationToken);
    }

    public Task<AppUser?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public Task<bool> UsernameExistsAsync(
        string username,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return _db.Users.AnyAsync(
            u => (excludeUserId == null || u.Id != excludeUserId)
                 && u.Username.ToLower() == normalized,
            cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
        => _db.Users.CountAsync(
            u => u.Role == UserRole.Admin
                 && u.Status == AccountStatus.Active
                 && (excludeUserId == null || u.Id != excludeUserId),
            cancellationToken);

    public void Add(AppUser user) => _db.Users.Add(user);

    public void Remove(AppUser user) => _db.Users.Remove(user);
}
