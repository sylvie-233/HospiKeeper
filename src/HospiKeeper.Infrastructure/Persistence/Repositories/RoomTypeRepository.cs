using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

/// <summary>房型仓储：名称唯一性与引用计数检查。</summary>
internal sealed class RoomTypeRepository : IRoomTypeRepository
{
    private readonly HotelDbContext _db;

    public RoomTypeRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<RoomType>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.RoomTypes.AsNoTracking()
            .OrderBy(t => t.BasePrice)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public Task<RoomType?> GetByIdAsync(int roomTypeId, CancellationToken cancellationToken = default)
        => _db.RoomTypes.FirstOrDefaultAsync(t => t.Id == roomTypeId, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        int? excludeRoomTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return _db.RoomTypes.AnyAsync(
            t => (excludeRoomTypeId == null || t.Id != excludeRoomTypeId)
                 && t.Name.ToLower() == normalized,
            cancellationToken);
    }

    public Task<int> CountRoomsAsync(int roomTypeId, CancellationToken cancellationToken = default)
        => _db.Rooms.CountAsync(r => r.TypeId == roomTypeId, cancellationToken);

    public void Add(RoomType roomType) => _db.RoomTypes.Add(roomType);

    public void Remove(RoomType roomType) => _db.RoomTypes.Remove(roomType);
}
