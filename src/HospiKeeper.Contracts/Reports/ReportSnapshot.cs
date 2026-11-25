namespace HospiKeeper.Contracts;

/// <summary>单日营收点。</summary>
/// <param name="Date">日期。</param>
/// <param name="Revenue">当日营收。</param>
/// <param name="CheckOutCount">当日退房单数。</param>
public sealed record DailyRevenuePoint(DateTime Date, decimal Revenue, int CheckOutCount);

/// <summary>房型经营统计。</summary>
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
    /// <summary>把各房态数量拼成一行中文摘要，供表格展示。</summary>
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

/// <summary>报表页概览数据。</summary>
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
    /// <summary>今日相对昨日的营收变化百分比；昨日无营收时为 <c>null</c>。</summary>
    public double? TodayTrend
        => YesterdayRevenue > 0m
            ? (double)((TodayRevenue - YesterdayRevenue) / YesterdayRevenue * 100m)
            : null;
}

/// <summary>区间营收明细行。</summary>
public sealed record RevenueRangeRow(DateTime Date, decimal Revenue, double? DayOverDay, double Share);

/// <summary>区间营收汇总。</summary>
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
