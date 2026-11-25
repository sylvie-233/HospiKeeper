using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

internal sealed class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _db;

    public RoomRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<Room>> GetAllAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Rooms.AsNoTracking()
            .Where(r => (floor == null || r.Floor == floor)
                        && (typeId == null || r.TypeId == typeId)
                        && (status == null || r.Status == status));

        return await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetFreeAsync(CancellationToken cancellationToken = default)
        => await _db.Rooms.AsNoTracking()
            .Where(r => r.Status == RoomStatus.Free)
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNo)
            .ToListAsync(cancellationToken);

    public Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
        => _db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);

    public async Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default)
        => await _db.Rooms.AsNoTracking()
            .Select(r => r.Floor)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Room>> GetByTypeAsync(
        int roomTypeId,
        CancellationToken cancellationToken = default)
        => await _db.Rooms
            .Where(r => r.TypeId == roomTypeId)
            .ToListAsync(cancellationToken);

    public Task<bool> RoomNoExistsAsync(
        string roomNo,
        int? excludeRoomId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = roomNo.Trim().ToLowerInvariant();
        return _db.Rooms.AnyAsync(
            r => (excludeRoomId == null || r.Id != excludeRoomId)
                 && r.RoomNo.ToLower() == normalized,
            cancellationToken);
    }

    public async Task<bool> HasBusinessHistoryAsync(
        int roomId,
        CancellationToken cancellationToken = default)
    {
        if (await _db.CheckIns.AnyAsync(c => c.RoomId == roomId, cancellationToken))
        {
            return true;
        }

        return await _db.Reservations.AnyAsync(r => r.RoomId == roomId, cancellationToken);
    }

    public Task<int> CountByStatusAsync(
        RoomStatus status,
        CancellationToken cancellationToken = default)
        => _db.Rooms.CountAsync(r => r.Status == status, cancellationToken);

    public void Add(Room room) => _db.Rooms.Add(room);

    public void Remove(Room room) => _db.Rooms.Remove(room);
}
