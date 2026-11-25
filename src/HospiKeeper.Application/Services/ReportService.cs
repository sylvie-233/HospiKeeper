using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Abstractions;
using Sylvie233.Caching;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Services;

/// <summary>经营统计报表实现，读多写少并带缓存。</summary>
public sealed class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICacheService _cache;

    public ReportService(IUnitOfWork unitOfWork, IClock clock, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _cache = cache;
    }

    public async Task<ReportSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.ReportSnapshot,
            LoadSnapshotAsync,
            CacheDurations.Reports,
            cancellationToken);

    private async Task<ReportSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        var now = _clock.Now;
        var today = now.Date;
        var yesterday = today.AddDays(-1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var bills = await _unitOfWork.Bills.GetAllAsync(cancellationToken);
        var living = await _unitOfWork.CheckIns.GetLivingAsync(cancellationToken);
        var rooms = await _unitOfWork.Rooms.GetAllAsync(cancellationToken: cancellationToken);
        var roomTypes = await _unitOfWork.RoomTypes.GetAllAsync(cancellationToken);

        var sellable = rooms.Count(r => r.IsSellable);
        var occupied = rooms.Count(r => r.Status == RoomStatus.Occupied);

        var daily = new List<DailyRevenuePoint>(30);
        for (var i = 29; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayBills = bills.Where(b => b.CheckoutTime.Date == date).ToList();
            daily.Add(new DailyRevenuePoint(date, dayBills.Sum(b => b.Total), dayBills.Count));
        }

        var typeStats = roomTypes
            .Select(type =>
            {
                var typeRooms = rooms.Where(r => r.TypeId == type.Id).ToList();
                var typeSellable = typeRooms.Count(r => r.IsSellable);
                var typeOccupied = typeRooms.Count(r => r.Status == RoomStatus.Occupied);
                var typeReserved = typeRooms.Count(r => r.Status == RoomStatus.Reserved);
                var rate = typeSellable == 0
                    ? 0d
                    : Math.Round((typeOccupied + typeReserved) * 100d / typeSellable, 0);
                return new RoomTypeStat(
                    type.Name,
                    type.BasePrice,
                    typeRooms.Count,
                    typeOccupied,
                    typeReserved,
                    typeRooms.Count(r => r.Status == RoomStatus.Free),
                    typeRooms.Count(r => r.Status == RoomStatus.Cleaning),
                    typeRooms.Count(r => r.Status == RoomStatus.Repair),
                    typeRooms.Count(r => r.Status == RoomStatus.Disabled),
                    typeSellable,
                    rate);
            })
            .OrderBy(s => s.BasePrice)
            .ToList();

        return new ReportSnapshot(
            TodayRevenue: bills.Where(b => b.CheckoutTime.Date == today).Sum(b => b.Total),
            YesterdayRevenue: bills.Where(b => b.CheckoutTime.Date == yesterday).Sum(b => b.Total),
            MonthRevenue: bills.Where(b => b.CheckoutTime >= monthStart && b.CheckoutTime < nextMonth).Sum(b => b.Total),
            OccupancyRate: sellable == 0 ? 0d : Math.Round(occupied * 100d / sellable, 1),
            OccupiedRooms: occupied,
            SellableRooms: sellable,
            AverageRoomRate: living.Count == 0 ? 0m : Math.Round(living.Average(c => c.Price), 2),
            TodayCheckIns: await _unitOfWork.CheckIns.CountByCheckInDateAsync(today, cancellationToken),
            TodayCheckOuts: bills.Count(b => b.CheckoutTime.Date == today),
            PendingReservations: await _unitOfWork.Reservations.CountPendingAsync(cancellationToken),
            CleaningRooms: rooms.Count(r => r.Status == RoomStatus.Cleaning),
            DepositHold: living.Sum(c => c.Deposit),
            Last30Days: daily,
            RoomTypeStats: typeStats);
    }

    public async Task<RevenueRangeResult> GetRevenueRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var from = start.Date;
        var to = end.Date;
        if (to < from)
        {
            throw new DomainException("结束日期不能早于开始日期。");
        }

        return await _cache.GetOrCreateAsync(
            CacheKeys.ReportRange(from, to),
            ct => LoadRevenueRangeAsync(from, to, ct),
            CacheDurations.Reports,
            cancellationToken);
    }

    private async Task<RevenueRangeResult> LoadRevenueRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        var length = (to - from).Days + 1;
        var previousFrom = from.AddDays(-length);
        var previousTo = from.AddDays(-1);

        var bills = await _unitOfWork.Bills.GetByCheckoutRangeAsync(previousFrom, to, cancellationToken);

        decimal RevenueOn(DateTime date)
            => bills.Where(b => b.CheckoutTime.Date == date).Sum(b => b.Total);

        var days = new List<DailyRevenuePoint>(length);
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var dayBills = bills.Where(b => b.CheckoutTime.Date == date).ToList();
            days.Add(new DailyRevenuePoint(date, dayBills.Sum(b => b.Total), dayBills.Count));
        }

        var total = days.Sum(d => d.Revenue);
        var previousTotal = bills
            .Where(b => b.CheckoutTime.Date >= previousFrom && b.CheckoutTime.Date <= previousTo)
            .Sum(b => b.Total);

        var rows = new List<RevenueRangeRow>(length);
        for (var i = 0; i < days.Count; i++)
        {
            var previousDayRevenue = RevenueOn(days[i].Date.AddDays(-1));
            var dayOverDay = previousDayRevenue > 0m
                ? (double?)((days[i].Revenue - previousDayRevenue) / previousDayRevenue * 100m)
                : null;
            var share = total > 0m ? (double)(days[i].Revenue / total * 100m) : 0d;
            rows.Add(new RevenueRangeRow(days[i].Date, days[i].Revenue, dayOverDay, share));
        }

        var max = days.Count == 0 ? 0m : days.Max(d => d.Revenue);
        var min = days.Count == 0 ? 0m : days.Min(d => d.Revenue);
        var change = previousTotal > 0m
            ? (double?)((total - previousTotal) / previousTotal * 100m)
            : null;

        return new RevenueRangeResult(
            from,
            to,
            total,
            Math.Round(total / length, 2),
            max,
            min,
            change,
            days,
            rows);
    }
}