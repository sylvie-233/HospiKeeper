using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;

namespace HospiKeeper.App.ViewModels;

public sealed class StayCardViewModel
{
    public StayCardViewModel(StayInfo info)
    {
        Info = info;
        CheckIn = info.CheckIn;
    }

    public StayInfo Info { get; }
    public CheckInRecordDto CheckIn { get; }
    public string RoomNo => CheckIn.RoomNo;
    public string GuestName => CheckIn.GuestName;
    public string GuestPhone => CheckIn.GuestPhone;
    public string RoomTypeName => CheckIn.RoomTypeName;
    public string CheckInText => CheckIn.CheckInTime.ToString("MM-dd HH:mm");
    public string PlanOutText => CheckIn.PlanOutTime.ToString("MM-dd HH:mm");
    public string PriceText => $"¥{CheckIn.Price:0.00}";
    public string DepositText => $"¥{CheckIn.Deposit:0.00}";
    public string ExtraText => CheckIn.ExtraCharges.Count == 0
        ? "暂无追加"
        : $"追加 {CheckIn.ExtraCharges.Count} 笔 · ¥{Info.ExtraTotal:0.00}";
    public string OperatorName => CheckIn.OperatorName;
    public string CostText => $"¥{Info.EstimatedTotal:0.00}";
}

/// <summary>在住列表：只读展示，结算引导回前台。</summary>
public sealed partial class StaysViewModel : BaseViewModel
{
    public StaysViewModel(ApiProvider apiProvider, AppSession session, ToastService toast)
        : base(apiProvider, session, toast)
    {
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        InfoCommand = new AsyncRelayCommand(ShowBoundaryAsync);
    }

    [ObservableProperty] private string _depositTotalText = "¥0.00";
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string _summaryText = "在住 0 间";

    public ObservableCollection<StayCardViewModel> Stays { get; } = [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand InfoCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            var stays = await Api.GetLivingStaysAsync();
            Stays.Clear();
            decimal deposit = 0;
            foreach (var stay in stays.OrderByDescending(s => s.CheckIn.CheckInTime))
            {
                Stays.Add(new StayCardViewModel(stay));
                deposit += stay.CheckIn.Deposit;
            }

            DepositTotalText = $"¥{deposit:#,0.00}";
            IsEmpty = Stays.Count == 0;
            SummaryText = $"在住 {Stays.Count} 间";
            MarkSynced();
        });

    private async Task ShowBoundaryAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            await page.DisplayAlertAsync("前台业务提醒", "退房、结算与收款请回前台桌面端办理。管理端仅提供在住信息查看。", "知道了");
        }
    }
}
