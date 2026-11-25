using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

internal sealed class CheckInRepository : ICheckInRepository
{
    private readonly HotelDbContext _db;

    public CheckInRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<CheckInRecord>> GetLivingAsync(
        CancellationToken cancellationToken = default)
        => await _db.CheckIns.AsNoTracking()
            .Include(c => c.ExtraCharges)
            .Where(c => c.Status == CheckInStatus.Living)
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync(cancellationToken);

    public Task<CheckInRecord?> GetByIdAsync(
        int checkInId,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var query = _db.CheckIns.AsQueryable();
        if (includeDetails)
        {
            query = query.Include(c => c.Companions).Include(c => c.ExtraCharges);
        }

        return query.FirstOrDefaultAsync(c => c.Id == checkInId, cancellationToken);
    }

    public async Task<IReadOnlyList<CheckInRecord>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var roomNo = query.RoomNo?.Trim().ToLowerInvariant();
        var keyword = query.Keyword?.Trim().ToLowerInvariant();
        var from = query.From?.Date;
        var to = query.To?.Date;

        var source = _db.CheckIns.AsNoTracking()
            .Include(c => c.Companions)
            .Include(c => c.ExtraCharges)
            .AsQueryable();

        if (!isAdmin && operatorUserId is not null)
        {
            source = source.Where(c => c.OpUserId == operatorUserId);
        }

        if (!string.IsNullOrEmpty(roomNo))
        {
            source = source.Where(c => c.RoomNo.ToLower().Contains(roomNo));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            source = source.Where(c =>
                c.GuestName.ToLower().Contains(keyword)
                || c.GuestPhone.Contains(keyword)
                || c.GuestIdCard.ToLower().Contains(keyword)
                || c.CheckInNo.ToLower().Contains(keyword));
        }

        if (from is not null)
        {
            source = source.Where(c => c.CheckInTime >= from.Value);
        }

        if (to is not null)
        {
            var end = to.Value.AddDays(1);
            source = source.Where(c => c.CheckInTime < end);
        }

        return await source
            .OrderByDescending(c => c.CheckInTime)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default)
        => _db.CheckIns.AnyAsync(c => c.RoomId == roomId, cancellationToken);

    public Task<int> CountLivingAsync(CancellationToken cancellationToken = default)
        => _db.CheckIns.CountAsync(c => c.Status == CheckInStatus.Living, cancellationToken);

    public Task<int> CountByCheckInDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var day = date.Date;
        var end = day.AddDays(1);
        return _db.CheckIns.CountAsync(c => c.CheckInTime >= day && c.CheckInTime < end, cancellationToken);
    }

    public void Add(CheckInRecord record) => _db.CheckIns.Add(record);
}
