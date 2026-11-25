using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

internal sealed class CheckoutBillRepository : ICheckoutBillRepository
{
    private readonly HotelDbContext _db;

    public CheckoutBillRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<CheckoutBill>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Bills.AsNoTracking()
            .OrderByDescending(b => b.CheckoutTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CheckoutBill>> GetByCheckoutRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        return await _db.Bills.AsNoTracking()
            .Where(b => b.CheckoutTime >= start && b.CheckoutTime < end)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CheckoutBill>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var roomNo = query.RoomNo?.Trim().ToLowerInvariant();
        var keyword = query.Keyword?.Trim().ToLowerInvariant();
        var from = query.From?.Date;
        var to = query.To?.Date;

        var source = _db.Bills.AsNoTracking()
            .Include(b => b.Extras)
            .AsQueryable();

        if (!isAdmin && operatorUserId is not null)
        {
            source = source.Where(b => b.OpUserId == operatorUserId);
        }

        if (!string.IsNullOrEmpty(roomNo))
        {
            source = source.Where(b => b.RoomNo.ToLower().Contains(roomNo));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            source = source.Where(b =>
                b.GuestName.ToLower().Contains(keyword)
                || b.RoomNo.ToLower().Contains(keyword)
                || b.BillNo.ToLower().Contains(keyword));
        }

        if (from is not null)
        {
            source = source.Where(b => b.CheckoutTime >= from.Value);
        }

        if (to is not null)
        {
            var end = to.Value.AddDays(1);
            source = source.Where(b => b.CheckoutTime < end);
        }

        return await source
            .OrderByDescending(b => b.CheckoutTime)
            .ToListAsync(cancellationToken);
    }

    public Task<CheckoutBill?> GetByIdAsync(int billId, CancellationToken cancellationToken = default)
        => _db.Bills.AsNoTracking()
            .Include(b => b.Extras)
            .FirstOrDefaultAsync(b => b.Id == billId, cancellationToken);

    public void Add(CheckoutBill bill) => _db.Bills.Add(bill);
}
