using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Entities;
using HospiKeeper.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public sealed record LogRowViewModel(OperationLog Log)
{
    public string TimeText => Log.CreatedAt.ToString("yyyy-MM-dd HH:mm");
    public string UserName => Log.UserName;
    public string Action => Log.Action;
    public string Target => Log.Target ?? "—";
    public string Detail => Log.Detail ?? "—";
    public string BizNo => Log.BizNo ?? "—";
}

public sealed partial class LogsPageViewModel : PageViewModelBase
{
    public LogsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        ActionOptions = new ObservableCollection<string>(
            new[] { "全部" }.Concat(OperationActions.All));
        _selectedAction = "全部";
        QueryCommand = new AsyncRelayCommand(LoadAsync);
        ResetCommand = new RelayCommand(() =>
        {
            SelectedAction = "全部";
            Keyword = string.Empty;
            _ = LoadAsync();
        });
    }

    public override string Key => PageKeys.Logs;
    public override string Title => "操作日志";
    public override Geometry Icon => AntIcons.Records;
    public override string Group => "记录与统计";
    public override int PageOrder => 70;

    public ObservableCollection<string> ActionOptions { get; }
    public ObservableCollection<LogRowViewModel> Rows { get; } = new();

    [ObservableProperty]
    private string _selectedAction;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private string _countText = "共 0 条";

    public IAsyncRelayCommand QueryCommand { get; }
    public IRelayCommand ResetCommand { get; }

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();


    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var action = SelectedAction == "全部" ? null : SelectedAction;
            var logs = await Api.QueryLogsAsync(action, string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim());
            Rows.Clear();
            foreach (var log in logs)
            {
                Rows.Add(new LogRowViewModel(log));
            }

            CountText = $"共 {Rows.Count} 条";
        });
    }
}
