using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Models;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public sealed class StayRowViewModel
{
    public StayRowViewModel(StayInfo stay, StayPageViewModel page)
    {
        Stay = stay;
        CheckoutCommand = new RelayCommand(() => page.ShowCheckout(this));
        AddExtraCommand = new RelayCommand(() => page.ShowAddExtra(this));
        DetailCommand = new RelayCommand(() => page.ShowDetail(this));
    }

    public StayInfo Stay { get; }
    public string RoomNo => Stay.CheckIn.RoomNo;
    public string GuestName => Stay.CheckIn.GuestName;
    public string GuestPhone => Stay.CheckIn.GuestPhone;
    public string CheckInText => Stay.CheckIn.CheckInTime.ToString("MM-dd HH:mm");
    public string PlanOutText => Stay.CheckIn.PlanOutTime.ToString("MM-dd HH:mm");
    public string PriceText => $"¥{Stay.CheckIn.Price:0.00}";
    public string DepositText => $"¥{Stay.CheckIn.Deposit:0.00}";
    public string CostText => $"¥{Stay.EstimatedTotal:0.00}";
    public string ExtraCountText => Stay.CheckIn.ExtraCharges.Count == 0
        ? "无追加"
        : $"{Stay.CheckIn.ExtraCharges.Count} 项 · ¥{Stay.ExtraTotal:0.00}";

    public bool IsOverdue => Stay.CheckIn.PlanOutTime < DateTime.Now;

    public IRelayCommand CheckoutCommand { get; }
    public IRelayCommand AddExtraCommand { get; }
    public IRelayCommand DetailCommand { get; }
}

public sealed partial class StayPageViewModel : PageViewModelBase
{
    public StayPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public override string Key => PageKeys.Stay;
    public override string Title => "在住 / 退房结算";
    public override Geometry Icon => AntIcons.Bed;
    public override int PageOrder => 40;

    public ObservableCollection<StayRowViewModel> Rows { get; } = new();

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private string _countText = "共 0 位在住";

    public IAsyncRelayCommand RefreshCommand { get; }

    partial void OnKeywordChanged(string value) => _ = LoadAsync();

    public override async Task OnNavigatedToAsync(object? argument)
    {
        if (argument is StayFilter filter && filter.Keyword is not null)
        {
            Keyword = filter.Keyword;
        }

        await LoadAsync();
    }


    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var stays = await Api.GetLivingStaysAsync(Keyword.Trim());
            Rows.Clear();
            foreach (var stay in stays)
            {
                Rows.Add(new StayRowViewModel(stay, this));
            }

            CountText = $"共 {Rows.Count} 位在住";
        });
    }

    internal void ShowCheckout(StayRowViewModel row)
        => Dialog.Show(new CheckoutDialogViewModel(Dialog, Toast, Api, row.Stay.CheckIn, async () =>
        {
            await LoadAsync();
            NotifyDataChanged();
        }));

    internal void ShowAddExtra(StayRowViewModel row)
        => Dialog.Show(new AddExtraDialogViewModel(Dialog, Toast, Api, row.Stay.CheckIn, async () =>
        {
            await LoadAsync();
            NotifyDataChanged();
        }));

    internal void ShowDetail(StayRowViewModel row)
        => Dialog.Show(new CheckInDetailDialogViewModel(Dialog, row.Stay.CheckIn));
}
