using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;

namespace HospiKeeper.App.ViewModels;

public sealed partial class PriceItemViewModel : ObservableObject
{
    private readonly Func<PriceItemViewModel, Task> _save;

    public PriceItemViewModel(RoomTypeDto type, Func<PriceItemViewModel, Task> save)
    {
        Type = type;
        _save = save;
        _currentPrice = type.BasePrice;
        _newPriceText = type.BasePrice.ToString("0.00");
        SaveCommand = new AsyncRelayCommand(() => _save(this));
    }

    public RoomTypeDto Type { get; }

    [ObservableProperty]
    private decimal _currentPrice;

    [ObservableProperty]
    private string _newPriceText;

    public string Name => Type.Name;
    public string CurrentPriceText => $"¥{CurrentPrice:0.00}";
    public IAsyncRelayCommand SaveCommand { get; }

    public void ApplySaved(decimal price)
    {
        CurrentPrice = price;
        NewPriceText = price.ToString("0.00");
        OnPropertyChanged(nameof(CurrentPriceText));
    }
}

/// <summary>我的：个人信息、快捷改价、日志入口与退出。</summary>
public sealed partial class MineViewModel : BaseViewModel
{
    private readonly AppNavigator _navigator;

    public MineViewModel(
        ApiProvider apiProvider,
        AppSession session,
        ToastService toast,
        AppNavigator navigator) : base(apiProvider, session, toast)
    {
        _navigator = navigator;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        OpenPriceSheetCommand = new AsyncRelayCommand(OpenPriceSheetAsync);
        ClosePriceSheetCommand = new RelayCommand(ClosePriceSheet);
        OpenLogsCommand = new AsyncRelayCommand(() => _navigator.ShowLogsAsync());
        AboutCommand = new AsyncRelayCommand(ShowAboutAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        MarkSynced();
    }

    [ObservableProperty] private bool _isPriceSheetOpen;
    [ObservableProperty] private string _syncText = "尚未同步";

    public string Initial => Session.Initial;
    public string DisplayName => Session.User?.RealName ?? "管理员";
    public string RoleText => "ADMIN · 老板端";
    public string AppVersion => "v1.0";
    public ObservableCollection<PriceItemViewModel> PriceItems { get; } = [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand OpenPriceSheetCommand { get; }
    public IRelayCommand ClosePriceSheetCommand { get; }
    public IAsyncRelayCommand OpenLogsCommand { get; }
    public IAsyncRelayCommand AboutCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            var types = await Api.GetRoomTypesAsync();
            PriceItems.Clear();
            foreach (var type in types)
            {
                PriceItems.Add(new PriceItemViewModel(type,
                    item => SavePriceAsync(item)));
            }

            SyncText = $"数据同步于 {DateTime.Now:HH:mm}";
            MarkSynced();
        });

    private async Task OpenPriceSheetAsync()
    {
        IsPriceSheetOpen = true;
        if (PriceItems.Count == 0)
        {
            await LoadAsync();
        }
    }

    private void ClosePriceSheet() => IsPriceSheetOpen = false;

    private async Task SavePriceAsync(PriceItemViewModel item)
    {
        if (!decimal.TryParse(item.NewPriceText, out var price) || price <= 0)
        {
            Toast.Error("请输入大于 0 的有效价格。");
            return;
        }

        price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        if (price == item.Type.BasePrice)
        {
            Toast.Warning("新价格与当前价格相同。");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await Api.UpdateRoomTypeAsync(
                item.Type.Id,
                new RoomTypeInput(item.Type.Name, price, item.Type.Note));
            item.ApplySaved(price);
        }, $"{item.Name} 已调整为 ¥{price:0.00}");
    }

    private async Task ShowAboutAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            await page.DisplayAlertAsync(
                "关于管理端",
                "云栖民宿管理端：老板视角的经营看板、房态、预定、在住与日志。\n\n"
                + "全 App 仅支持取消预定与快捷改价；入住、退房、收款与证件登记请回前台桌面端。",
                "知道了");
        }
    }

    private async Task LogoutAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return;
        }

        var confirmed = await page.DisplayAlertAsync("退出登录", "确定退出当前管理员账号吗？", "退出", "取消");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await Api.LogoutAsync();
        }
        catch
        {
            // 网络不可用时也允许清除本地会话。
        }

        Session.SignOut();
        _navigator.ShowLogin();
    }
}
