using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

/// <summary>预定仓储：状态查询、待入住校验与写入。</summary>
internal sealed class ReservationRepository : IReservationRepository
{
    private readonly HotelDbContext _db;

    public ReservationRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default)
        => await _db.Reservations.AsNoTracking()
            .Where(r => status == null || r.Status == status)
            .OrderBy(r => r.Status == ReservationStatus.Pending ? 0 : 1)
            .ThenBy(r => r.ArriveTime)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Reservation?> GetByIdAsync(int reservationId, CancellationToken cancellationToken = default)
        => _db.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await _db.Reservations.AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.ArriveTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
        => await _db.Reservations.AsNoTracking()
            .Where(r => r.GuestPhone == phone)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Reservation?> GetByResNoAsync(string resNo, CancellationToken cancellationToken = default)
        => _db.Reservations.FirstOrDefaultAsync(r => r.ResNo == resNo, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetOverlappingPendingAsync(
        int typeId,
        DateTime checkInDate,
        DateTime checkOutDate,
        CancellationToken cancellationToken = default)
        => await _db.Reservations.AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Pending
                        && r.TypeId == typeId
                        && r.CheckInDate < checkOutDate.Date
                        && r.CheckOutDate > checkInDate.Date)
            .ToListAsync(cancellationToken);

    public Task<bool> HasPendingForRoomAsync(
        int roomId,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default)
        => _db.Reservations.AnyAsync(
            r => r.RoomId == roomId
                 && r.Status == ReservationStatus.Pending
                 && (excludeReservationId == null || r.Id != excludeReservationId),
            cancellationToken);

    public Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default)
        => _db.Reservations.AnyAsync(r => r.RoomId == roomId, cancellationToken);

    public Task<int> CountPendingAsync(CancellationToken cancellationToken = default)
        => _db.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending, cancellationToken);

    public void Add(Reservation reservation) => _db.Reservations.Add(reservation);
}
