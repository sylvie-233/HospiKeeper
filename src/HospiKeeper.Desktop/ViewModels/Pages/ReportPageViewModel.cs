using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using Sylvie233.UI;
using Sylvie233.UI.Controls;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public sealed class StatCardViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? SubText { get; init; }
    public Geometry Icon { get; init; } = AntIcons.Chart;
    public IBrush IconBackground { get; init; } = AntPalette.PrimaryBg;
    public IBrush IconForeground { get; init; } = AntPalette.Primary;
    public string? TrendText { get; init; }
    public IBrush? TrendBrush { get; init; }
}

public sealed class RevenueDayRowViewModel
{
    public string DateText { get; init; } = string.Empty;
    public string RevenueText { get; init; } = string.Empty;
    public string DayOverDayText { get; init; } = string.Empty;
    public string ShareText { get; init; } = string.Empty;
    public bool IsUp { get; init; }
    public bool IsDown { get; init; }
}

public sealed partial class ReportPageViewModel : PageViewModelBase
{
    public ReportPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        QueryRangeCommand = new AsyncRelayCommand(QueryRangeAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAllAsync);
        Last7Command = new AsyncRelayCommand(async () =>
        {
            DateFrom = DateTime.Today.AddDays(-6);
            DateTo = DateTime.Today;
            await QueryRangeAsync();
        });
        Last30Command = new AsyncRelayCommand(async () =>
        {
            DateFrom = DateTime.Today.AddDays(-29);
            DateTo = DateTime.Today;
            await QueryRangeAsync();
        });
        DateFrom = DateTime.Today.AddDays(-6);
        DateTo = DateTime.Today;
    }

    public override string Key => PageKeys.Report;
    public override string Title => "统计报表";
    public override Geometry Icon => AntIcons.Chart;
    public override string Group => "记录与统计";
    public override int PageOrder => 60;

    public ObservableCollection<StatCardViewModel> Stats { get; } = new();
    public ObservableCollection<RevenueDayRowViewModel> RangeRows { get; } = new();
    public ObservableCollection<RoomTypeStat> RoomTypes { get; } = new();

    [ObservableProperty]
    private IReadOnlyList<ChartBar> _weekBars = Array.Empty<ChartBar>();

    [ObservableProperty]
    private string _weekSummary = string.Empty;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    [ObservableProperty]
    private string _rangeTotalText = "—";

    [ObservableProperty]
    private string _rangeAverageText = "—";

    [ObservableProperty]
    private string _rangeMaxMinText = "—";

    [ObservableProperty]
    private string _rangeChangeText = "—";

    [ObservableProperty]
    private IBrush _rangeChangeBrush = AntPalette.TextSecondary;

    [ObservableProperty]
    private IReadOnlyList<ChartBar> _rangeBars = Array.Empty<ChartBar>();

    public IAsyncRelayCommand QueryRangeCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand Last7Command { get; }
    public IAsyncRelayCommand Last30Command { get; }

    public override async Task OnNavigatedToAsync(object? argument)
    {
        await LoadSnapshotAsync();
        await QueryRangeAsync();
    }

    private async Task RefreshAllAsync()
    {
        await LoadSnapshotAsync();
        await QueryRangeAsync();
    }

    private async Task LoadSnapshotAsync()
    {
        await TryLoadAsync(async () =>
        {
            var snapshot = await Api.GetReportSnapshotAsync();

            var trendText = snapshot.TodayTrend is { } trend
                ? (trend >= 0 ? $"↑ 较昨日 +{trend:0.0}%" : $"↓ 较昨日 {trend:0.0}%")
                : "较昨日 —";
            var trendBrush = snapshot.TodayTrend is { } t
                ? (t >= 0 ? AntPalette.Success : AntPalette.Error)
                : AntPalette.TextTertiary;

            Stats.Clear();
            Stats.Add(new StatCardViewModel
            {
                Title = "今日营收",
                Value = Money(snapshot.TodayRevenue),
                SubText = "按今日退房结算统计",
                TrendText = trendText,
                TrendBrush = trendBrush,
                Icon = AntIcons.Money,
                IconBackground = AntPalette.OrangeBg,
                IconForeground = AntPalette.Orange
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "本月累计营收",
                Value = Money(snapshot.MonthRevenue),
                SubText = $"{DateTime.Today.Month} 月至今",
                Icon = AntIcons.Calendar,
                IconBackground = AntPalette.PrimaryBg,
                IconForeground = AntPalette.Primary
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "当前入住率",
                Value = $"{snapshot.OccupancyRate:0.0}%",
                SubText = $"{snapshot.OccupiedRooms} / {snapshot.SellableRooms} 间在住",
                Icon = AntIcons.Chart,
                IconBackground = AntPalette.PurpleBg,
                IconForeground = AntPalette.Purple
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "平均房价",
                Value = snapshot.AverageRoomRate > 0 ? Money(snapshot.AverageRoomRate) : "—",
                SubText = "当前在住成交均价",
                Icon = AntIcons.Tag,
                IconBackground = AntPalette.ErrorBg,
                IconForeground = AntPalette.Red
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "今日开房 / 退房",
                Value = $"{snapshot.TodayCheckIns} / {snapshot.TodayCheckOuts}",
                SubText = "实时统计",
                Icon = AntIcons.Refresh,
                IconBackground = AntPalette.PrimaryBg,
                IconForeground = AntPalette.Primary
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "待到店预定",
                Value = $"{snapshot.PendingReservations} 间",
                SubText = snapshot.PendingReservations > 0 ? "记得提前备房" : "暂无预定",
                Icon = AntIcons.Calendar,
                IconBackground = AntPalette.OrangeBg,
                IconForeground = AntPalette.Orange
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "待打扫",
                Value = $"{snapshot.CleaningRooms} 间",
                SubText = snapshot.CleaningRooms > 0 ? "打扫完成后可售" : "全部干净",
                Icon = AntIcons.Sparkle,
                IconBackground = AntPalette.WarningBg,
                IconForeground = AntPalette.Warning
            });
            Stats.Add(new StatCardViewModel
            {
                Title = "押金占用",
                Value = Money(snapshot.DepositHold),
                SubText = "在住客人押金合计",
                Icon = AntIcons.Key,
                IconBackground = AntPalette.SuccessBg,
                IconForeground = AntPalette.Success
            });

            var week = snapshot.Last30Days.TakeLast(7).ToList();
            WeekBars = week
                .Select((d, i) => new ChartBar(
                    i == week.Count - 1 ? "今天" : d.Date.ToString("MM-dd"),
                    (double)d.Revenue))
                .ToList();
            var weekSum = week.Sum(d => d.Revenue);
            WeekSummary = $"合计 {Money(weekSum)} · 日均 {Money(week.Count == 0 ? 0 : weekSum / week.Count)}";

            RoomTypes.Clear();
            foreach (var stat in snapshot.RoomTypeStats)
            {
                RoomTypes.Add(stat);
            }
        });
    }

    private async Task QueryRangeAsync()
    {
        var start = DateFrom ?? DateTime.Today.AddDays(-6);
        var end = DateTo ?? DateTime.Today;
        if (end < start)
        {
            Toast.Warning("结束日期不能早于开始日期。");
            return;
        }

        await TryLoadAsync(async () =>
        {
            var result = await Api.GetRevenueRangeAsync(start, end);
            RangeTotalText = Money(result.Total);
            RangeAverageText = Money(result.DailyAverage);
            RangeMaxMinText = $"{Money(result.MaxDay)} / {Money(result.MinDay)}";

            if (result.ChangeRate is { } change)
            {
                RangeChangeText = change >= 0 ? $"↑ +{change:0.0}%" : $"↓ {change:0.0}%";
                RangeChangeBrush = change >= 0 ? AntPalette.Success : AntPalette.Error;
            }
            else
            {
                RangeChangeText = "—";
                RangeChangeBrush = AntPalette.TextSecondary;
            }

            RangeBars = result.Days
                .Select(d => new ChartBar(d.Date.ToString("MM-dd"), (double)d.Revenue))
                .ToList();

            RangeRows.Clear();
            foreach (var row in result.Rows)
            {
                var d = row.DayOverDay;
                RangeRows.Add(new RevenueDayRowViewModel
                {
                    DateText = row.Date.ToString("MM-dd"),
                    RevenueText = Money(row.Revenue),
                    DayOverDayText = d is null ? "—" : d >= 0 ? $"↑ +{d:0.0}%" : $"↓ {d:0.0}%",
                    ShareText = $"{row.Share:0.0}%",
                    IsUp = d > 0,
                    IsDown = d < 0
                });
            }
        });
    }

    private static string Money(decimal value) => $"¥{value:#,0.00}";
}
