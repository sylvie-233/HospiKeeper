using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Contracts;

public sealed record LoginResult(bool Success, string? Error, AppUser? User)
{
    public static LoginResult Fail(string message) => new(false, message, null);

    public static LoginResult Ok(AppUser user) => new(true, null, user);
}

/// <summary>看板上的一个房间及其当前住客 / 预定信息。</summary>
public sealed record BoardRoom(Room Room, CheckInRecord? ActiveStay, Reservation? PendingReservation);

/// <summary>在住列表的一行（含消费合计与预计房费）。</summary>
public sealed record StayInfo(CheckInRecord CheckIn, decimal ExtraTotal, decimal EstimatedRoomFee)
{
    public decimal EstimatedTotal => EstimatedRoomFee + ExtraTotal;
}

public sealed record FrontDeskCounts(int Living, int PendingReservations, int Cleaning);

public sealed record DailyRevenuePoint(DateTime Date, decimal Revenue, int CheckOutCount);

public sealed record RoomTypeStat(
    string TypeName,
    decimal BasePrice,
    int TotalRooms,
    int Occupied,
    int Reserved,
    int Free,
    int Cleaning,
    int Repair,
    int Disabled,
    int Sellable,
    double OccupancyRate)
{
    public string StatusSummary
    {
        get
        {
            var parts = new List<string>();
            if (Occupied > 0) parts.Add($"已入住 {Occupied}");
            if (Reserved > 0) parts.Add($"预定 {Reserved}");
            if (Free > 0) parts.Add($"空闲 {Free}");
            if (Cleaning > 0) parts.Add($"打扫 {Cleaning}");
            if (Repair > 0) parts.Add($"维修 {Repair}");
            if (Disabled > 0) parts.Add($"停用 {Disabled}");
            return parts.Count == 0 ? "暂无房间" : string.Join(" · ", parts);
        }
    }
}

public sealed record ReportSnapshot(
    decimal TodayRevenue,
    decimal YesterdayRevenue,
    decimal MonthRevenue,
    double OccupancyRate,
    int OccupiedRooms,
    int SellableRooms,
    decimal AverageRoomRate,
    int TodayCheckIns,
    int TodayCheckOuts,
    int PendingReservations,
    int CleaningRooms,
    decimal DepositHold,
    List<DailyRevenuePoint> Last30Days,
    List<RoomTypeStat> RoomTypeStats)
{
    public double? TodayTrend
        => YesterdayRevenue > 0m
            ? (double)((TodayRevenue - YesterdayRevenue) / YesterdayRevenue * 100m)
            : null;
}

public sealed record RevenueRangeRow(DateTime Date, decimal Revenue, double? DayOverDay, double Share);

public sealed record RevenueRangeResult(
    DateTime Start,
    DateTime End,
    decimal Total,
    decimal DailyAverage,
    decimal MaxDay,
    decimal MinDay,
    double? ChangeRate,
    List<DailyRevenuePoint> Days,
    List<RevenueRangeRow> Rows);
