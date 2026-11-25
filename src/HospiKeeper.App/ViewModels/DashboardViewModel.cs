using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.App.ViewModels;

public sealed class WeekBarViewModel
{
    public string Label { get; init; } = string.Empty;
    public string ValueText { get; init; } = string.Empty;
    public double BarHeight { get; init; } = 8;
    public Color BarColor { get; init; } = Color.FromArgb("#2563EB");
}

public sealed class DashboardPendingItem
{
    public DashboardPendingItem(string title, string detail, Color dotColor, Func<Task> open)
    {
        Title = title;
        Detail = detail;
        DotColor = dotColor;
        OpenCommand = new AsyncRelayCommand(open);
    }

    public string Title { get; }
    public string Detail { get; }
    public Color DotColor { get; }
    public IAsyncRelayCommand OpenCommand { get; }
}

/// <summary>经营看板：老板视角的收入、入住率、预定与待处理提醒。</summary>
public sealed partial class DashboardViewModel : BaseViewModel
{
    private readonly List<DashboardPendingItem> _allPending = [];
    private readonly List<ReservationDto> _pendingReservations = [];
    private readonly List<RoomDto> _cleaningRooms = [];
    private readonly List<StayInfo> _todayCheckouts = [];

    public DashboardViewModel(ApiProvider apiProvider, AppSession session, ToastService toast)
        : base(apiProvider, session, toast)
    {
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        TogglePendingCommand = new RelayCommand(TogglePending);
    }

    [ObservableProperty] private string _todayRevenueText = "¥0.00";
    [ObservableProperty] private string _todayTrendText = "较昨日 —";
    [ObservableProperty] private Color _todayTrendColor = Color.FromArgb("#9CA3AF");
    [ObservableProperty] private string _occupancyRateText = "0%";
    [ObservableProperty] private string _occupancyDetailText = "0 / 0 间在住";
    [ObservableProperty] private string _pendingReservationText = "0 间";
    [ObservableProperty] private string _webPendingText = "WEB 渠道 0 单";
    [ObservableProperty] private string _depositHoldText = "¥0.00";
    [ObservableProperty] private string _livingCountText = "在住 0 间";
    [ObservableProperty] private string _weekSummary = string.Empty;
    [ObservableProperty] private bool _isPendingExpanded;
    [ObservableProperty] private bool _hasPending;
    [ObservableProperty] private bool _hasMorePending;
    [ObservableProperty] private string _pendingToggleText = "查看全部";
    [ObservableProperty] private string _pendingCountText = "待处理 0 条";

    public ObservableCollection<WeekBarViewModel> WeekBars { get; } = [];
    public ObservableCollection<DashboardPendingItem> VisiblePending { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand TogglePendingCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            var snapshotTask = Api.GetReportSnapshotAsync();
            var pendingTask = Api.GetReservationsAsync(ReservationStatus.Pending);
            var roomsTask = Api.GetRoomsAsync();
            var staysTask = Api.GetLivingStaysAsync();
            await Task.WhenAll(snapshotTask, pendingTask, roomsTask, staysTask);

            var snapshot = await snapshotTask;
            _pendingReservations.Clear();
            _pendingReservations.AddRange(await pendingTask);
            _cleaningRooms.Clear();
            _cleaningRooms.AddRange((await roomsTask).Where(r => r.Status == RoomStatus.Cleaning));
            _todayCheckouts.Clear();
            _todayCheckouts.AddRange((await staysTask)
                .Where(s => s.CheckIn.PlanOutTime.Date == DateTime.Today));

            TodayRevenueText = Money(snapshot.TodayRevenue);
            if (snapshot.TodayTrend is { } trend)
            {
                TodayTrendText = trend >= 0 ? $"↑ 较昨日 +{trend:0.0}%" : $"↓ 较昨日 {trend:0.0}%";
                TodayTrendColor = trend >= 0
                    ? Color.FromArgb("#F87171")   // 国内习惯：涨红
                    : Color.FromArgb("#4ADE80");  // 跌绿
            }
            else
            {
                TodayTrendText = "较昨日 —";
                TodayTrendColor = Color.FromArgb("#9CA3AF");
            }

            OccupancyRateText = $"{snapshot.OccupancyRate:0.0}%";
            OccupancyDetailText = $"{snapshot.OccupiedRooms} / {snapshot.SellableRooms} 间在住";
            PendingReservationText = $"{snapshot.PendingReservations} 间";
            WebPendingText = $"WEB 渠道 {_pendingReservations.Count(r => r.Channel == ReservationChannel.Web)} 单";
            DepositHoldText = Money(snapshot.DepositHold);
            LivingCountText = $"在住 {snapshot.OccupiedRooms} 间";

            var week = snapshot.Last30Days.TakeLast(7).ToList();
            var max = week.Count == 0 ? 1m : Math.Max(1m, week.Max(d => d.Revenue));
            WeekBars.Clear();
            for (var i = 0; i < week.Count; i++)
            {
                var day = week[i];
                WeekBars.Add(new WeekBarViewModel
                {
                    Label = i == week.Count - 1 ? "今天" : day.Date.ToString("MM-dd"),
                    ValueText = day.Revenue >= 1000 ? $"{day.Revenue / 1000:0.0}k" : $"{day.Revenue:0}",
                    BarHeight = Math.Max(6, (double)(day.Revenue / max) * 78),
                    BarColor = Color.FromArgb(i == week.Count - 1 ? "#FBBF24" : "#60A5FA")
                });
            }

            var weekSum = week.Sum(d => d.Revenue);
            WeekSummary = $"合计 {Money(weekSum)} · 日均 {Money(week.Count == 0 ? 0 : weekSum / week.Count)}";

            BuildPendingItems();
            MarkSynced();
        });

    private void BuildPendingItems()
    {
        _allPending.Clear();
        foreach (var reservation in _pendingReservations.Where(r => r.Channel == ReservationChannel.Web))
        {
            _allPending.Add(new DashboardPendingItem(
                $"游客新预订 · {reservation.ResNo}",
                $"{reservation.GuestName} · {reservation.RoomTypeName} · {reservation.CheckInDate:MM-dd} → {reservation.CheckOutDate:MM-dd}",
                Color.FromArgb("#F59E0B"),
                () => GoAsync("//reservations")));
        }

        foreach (var room in _cleaningRooms)
        {
            _allPending.Add(new DashboardPendingItem(
                $"待保洁 · {room.RoomNo}",
                $"{room.TypeName} · 等待保洁确认",
                Color.FromArgb("#06B6D4"),
                () => GoAsync("//rooms")));
        }

        foreach (var stay in _todayCheckouts)
        {
            _allPending.Add(new DashboardPendingItem(
                $"今日预退 · {stay.CheckIn.RoomNo}",
                $"{stay.CheckIn.GuestName} · {stay.CheckIn.PlanOutTime:HH:mm} 预计退房",
                Color.FromArgb("#F97316"),
                () => GoAsync("//stays")));
        }

        PendingCountText = $"待处理 {_allPending.Count} 条";
        HasPending = _allPending.Count > 0;
        HasMorePending = _allPending.Count > 3;
        IsPendingExpanded = false;
        RebuildVisiblePending();
    }

    private void TogglePending()
    {
        IsPendingExpanded = !IsPendingExpanded;
        RebuildVisiblePending();
    }

    private void RebuildVisiblePending()
    {
        VisiblePending.Clear();
        foreach (var item in IsPendingExpanded ? _allPending : _allPending.Take(3))
        {
            VisiblePending.Add(item);
        }

        PendingToggleText = IsPendingExpanded
            ? "收起"
            : HasMorePending ? $"查看全部 {_allPending.Count} 条" : string.Empty;
    }

    private static async Task GoAsync(string route)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(route);
        }
    }

    private static string Money(decimal value) => $"¥{value:#,0.00}";
}
