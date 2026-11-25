using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;

namespace HospiKeeper.App.ViewModels;

public sealed class LogRowViewModel
{
    public LogRowViewModel(OperationLogDto log) => Log = log;

    public OperationLogDto Log { get; }
    public string Action => Log.Action;
    public string UserName => Log.UserName;
    public string Target => string.IsNullOrWhiteSpace(Log.Target) ? "—" : Log.Target!;
    public string Detail => string.IsNullOrWhiteSpace(Log.Detail) ? "—" : Log.Detail!;
    public string BizNo => string.IsNullOrWhiteSpace(Log.BizNo) ? string.Empty : Log.BizNo!;
    public string TimeText => Log.CreatedAt.ToString("MM-dd HH:mm");
    public Color ActionColor => Log.Action switch
    {
        OperationActions.CheckIn or OperationActions.CheckOut => Color.FromArgb("#2563EB"),
        OperationActions.Reservation or OperationActions.CancelReservation => Color.FromArgb("#7C3AED"),
        OperationActions.ModifyPrice => Color.FromArgb("#EA580C"),
        OperationActions.Login or OperationActions.Logout => Color.FromArgb("#16A34A"),
        _ => Color.FromArgb("#6B7280")
    };
}

/// <summary>管理端操作日志（只读二级页）。</summary>
public sealed partial class LogsViewModel : BaseViewModel
{
    private readonly AppNavigator _navigator;
    private string? _action;

    public LogsViewModel(
        ApiProvider apiProvider,
        AppSession session,
        ToastService toast,
        AppNavigator navigator) : base(apiProvider, session, toast)
    {
        _navigator = navigator;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        BackCommand = new AsyncRelayCommand(AppNavigator.CloseModalAsync);
        BuildChips();
    }

    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string _summaryText = "共 0 条记录";

    public ObservableCollection<LogRowViewModel> Logs { get; } = [];
    public ObservableCollection<FilterChipViewModel> FilterChips { get; } = [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private void BuildChips()
    {
        AddChip("全部", null, true);
        AddChip("预定", OperationActions.Reservation);
        AddChip("改价", OperationActions.ModifyPrice);
        AddChip("登录", OperationActions.Login);
    }

    private void AddChip(string text, string? action, bool selected = false)
        => FilterChips.Add(new FilterChipViewModel(text, () =>
        {
            _action = action;
            foreach (var chip in FilterChips)
            {
                chip.IsSelected = action is null ? chip.Text == "全部" : chip.Text == text;
            }

            return LoadAsync();
        })
        {
            IsSelected = selected
        });

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            var logs = await Api.QueryLogsAsync(_action);
            Logs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.CreatedAt))
            {
                Logs.Add(new LogRowViewModel(log));
            }

            IsEmpty = Logs.Count == 0;
            SummaryText = $"共 {Logs.Count} 条记录";
            MarkSynced();
        });
}
