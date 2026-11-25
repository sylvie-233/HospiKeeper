using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Messages;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Desktop.ViewModels.Pages;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels;

/// <summary>主窗口 ViewModel：导航菜单、当前页面与用户操作。</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly ClientSession _session;
    private readonly IHospiKeeperApi _api;
    private readonly DialogService _dialog;
    private readonly ToastService _toast;
    private readonly WindowCoordinator _coordinator;
    private readonly Dictionary<string, PageViewModelBase> _pages;
    private readonly Dictionary<string, NavigationItemViewModel> _navItems = new();
    private readonly DispatcherTimer _clockTimer;

    public MainViewModel(
        ClientSession session,
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation,
        WindowCoordinator coordinator,
        IEnumerable<PageViewModelBase> pages)
    {
        _session = session;
        _api = api;
        _dialog = dialog;
        _toast = toast;
        _coordinator = coordinator;

        _pages = pages.ToDictionary(p => p.Key);
        NavigateCommand = new RelayCommand<string>(key =>
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _ = NavigateAsync(key);
            }
        });
        ChangePasswordCommand = new RelayCommand(ShowChangePassword);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        AboutCommand = new RelayCommand(ShowAbout);
        ShortcutsCommand = new RelayCommand(() => _toast.Info(
            "快捷键：F2 入住登记 · Ctrl+P 修改密码 · Enter 提交表单 · Esc 关闭弹窗"));

        BuildNavigation();
        navigation.Requested += OnNavigationRequested;
        WeakReferenceMessenger.Default.Register<DataChangedMessage>(
            this, (_, _) => _ = RefreshCountsAsync());

        UpdateClock();
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();
    }

    public DialogService Dialog => _dialog;

    public ToastService Toasts => _toast;

    public ObservableCollection<NavigationGroupViewModel> NavigationGroups { get; } = new();

    [ObservableProperty]
    private PageViewModelBase? _currentPage;

    [ObservableProperty]
    private string _pageTitle = "房态看板";

    [ObservableProperty]
    private string _windowTitle = "云栖民宿管理系统 v1.0　—　[房态看板]";

    [ObservableProperty]
    private string _clockText = string.Empty;

    [ObservableProperty]
    private string _statusSummary = "正在加载房态汇总…";

    public string UserDisplay => _session.DisplayName;

    public string UserInitial => _session.UserInitial;

    public string RoleText => _session.RoleText;

    public bool IsAdmin => _session.IsAdmin;

    public IRelayCommand<string> NavigateCommand { get; }

    public IRelayCommand ChangePasswordCommand { get; }

    public IAsyncRelayCommand LogoutCommand { get; }

    public IRelayCommand AboutCommand { get; }

    public IRelayCommand ShortcutsCommand { get; }

    public async Task InitializeAsync()
    {
        await NavigateAsync(PageKeys.Dashboard);
        await RefreshCountsAsync();
    }

    public async Task RefreshCountsAsync()
    {
        try
        {
            var counts = await _api.GetCountsAsync();
            StatusSummary =
                $"在住 {counts.Living} ｜ 待打扫 {counts.Cleaning} ｜ 待到店预定 {counts.PendingReservations}";
            SetBadge(PageKeys.Stay, counts.Living);
            SetBadge(PageKeys.Reservations, counts.PendingReservations);
        }
        catch
        {
            StatusSummary = "房态汇总暂不可用";
        }
    }

    private void BuildNavigation()
    {
        var groups = new Dictionary<string, NavigationGroupViewModel>();
        foreach (var page in _pages.Values
                     .Where(p => !p.AdminOnly || IsAdmin)
                     .OrderBy(p => p.GroupOrder)
                     .ThenBy(p => p.PageOrder))
        {
            if (!groups.TryGetValue(page.Group, out var group))
            {
                group = new NavigationGroupViewModel(page.Group, page.GroupOrder);
                groups[page.Group] = group;
                NavigationGroups.Add(group);
            }

            var key = page.Key;
            var item = new NavigationItemViewModel(key, page.Title, page.Icon,
                () => _ = NavigateAsync(key));
            group.Items.Add(item);
            _navItems[key] = item;
        }
    }

    private void OnNavigationRequested(string key, object? argument)
        => _ = NavigateAsync(key, argument);

    private async Task NavigateAsync(string key, object? argument = null)
    {
        if (!_pages.TryGetValue(key, out var page))
        {
            return;
        }

        if (page.AdminOnly && !IsAdmin)
        {
            _toast.Warning("该功能仅管理员可用。");
            return;
        }

        CurrentPage = page;
        PageTitle = page.Title;
        WindowTitle = $"云栖民宿管理系统 v1.0　—　[{page.Title}]";

        foreach (var item in _navItems.Values)
        {
            item.IsActive = item.Key == key;
        }

        await page.OnNavigatedToAsync(argument);
    }

    private void SetBadge(string key, int value)
    {
        if (_navItems.TryGetValue(key, out var item))
        {
            item.Badge = value;
        }
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        var weekdays = "日一二三四五六";
        ClockText = $"{now:yyyy-MM-dd} 星期{weekdays[(int)now.DayOfWeek]} {now:HH:mm}";
    }

    private void ShowChangePassword()
    {
        _dialog.Show(new ChangePasswordDialogViewModel(_dialog, _api, _toast));
    }

    private void ShowAbout()
    {
        _dialog.Show(new AboutDialogViewModel(_dialog));
    }

    private async Task LogoutAsync()
    {
        var confirmed = await _dialog.ConfirmAsync(
            "退出登录",
            "确定要退出当前账号吗？未保存的表单内容将丢失。",
            "退出登录",
            "取消");
        if (!confirmed)
        {
            return;
        }

        _clockTimer.Stop();
        await _coordinator.LogoutAsync();
    }
}
