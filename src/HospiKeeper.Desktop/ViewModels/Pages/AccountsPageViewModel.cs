using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Domain.Enums;
using HospiKeeper.UI.Controls;
using HospiKeeper.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public sealed class AccountRowViewModel
{
    public AccountRowViewModel(UserDto user, AccountsPageViewModel page)
    {
        User = user;
        IsSelf = page.IsSelf(user);
        EditCommand = new RelayCommand(() => page.ShowEdit(this));
        ToggleCommand = new AsyncRelayCommand(() => page.ToggleAsync(this));
        DeleteCommand = new AsyncRelayCommand(() => page.DeleteAsync(this));
    }

    public UserDto User { get; }
    public bool IsSelf { get; }
    public string Username => User.Username;
    public string RealName => User.RealName;
    public string RoleText => User.Role == UserRole.Admin ? "管理员" : "前台";
    public AntTagVariant RoleVariant => User.Role == UserRole.Admin ? AntTagVariant.Purple : AntTagVariant.Processing;
    public string StatusText => User.Status == AccountStatus.Active ? "启用" : "停用";
    public AntTagVariant StatusVariant => User.Status == AccountStatus.Active ? AntTagVariant.Success : AntTagVariant.Default;
    public string LastLoginText => User.LastLogin?.ToString("MM-dd HH:mm") ?? "从未登录";
    public string ToggleText => User.Status == AccountStatus.Active ? "停用" : "启用";
    public bool CanToggle => !IsSelf;
    public bool CanDelete => !IsSelf;
    public IRelayCommand EditCommand { get; }
    public IAsyncRelayCommand ToggleCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
}

public sealed partial class AccountsPageViewModel : PageViewModelBase
{
    private readonly ClientSession _session;

    public AccountsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation,
        ClientSession session) : base(api, dialog, toast, navigation)
    {
        _session = session;
        AddCommand = new AsyncRelayCommand(AddAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public override string Key => PageKeys.Accounts;
    public override string Title => "操作员账号";
    public override Geometry Icon => AntIcons.Users;
    public override string Group => "基础信息（管理员）";
    public override bool AdminOnly => true;
    public override int GroupOrder => 2;
    public override int PageOrder => 100;

    public ObservableCollection<AccountRowViewModel> Rows { get; } = new();

    [ObservableProperty]
    private string _countText = "共 0 个账号";

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    internal bool IsSelf(UserDto user) => user.Id == _session.UserId;

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();


    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var users = await Api.GetUsersAsync();
            Rows.Clear();
            foreach (var user in users)
            {
                Rows.Add(new AccountRowViewModel(user, this));
            }

            CountText = $"共 {Rows.Count} 个账号";
        });
    }

    private Task AddAsync()
    {
        Dialog.Show(new AccountEditDialogViewModel(
            Dialog, Toast, Api, null, false, async () =>
            {
                await LoadAsync();
                NotifyDataChanged();
            }));
        return Task.CompletedTask;
    }

    internal void ShowEdit(AccountRowViewModel row)
        => Dialog.Show(new AccountEditDialogViewModel(
            Dialog, Toast, Api, row.User, row.IsSelf, async () =>
            {
                if (row.IsSelf)
                {
                    try
                    {
                        var me = await Api.GetMeAsync();
                        _session.SignIn(me);
                    }
                    catch
                    {
                        // 忽略刷新会话失败
                    }
                }

                await LoadAsync();
                NotifyDataChanged();
            }));

    internal async Task ToggleAsync(AccountRowViewModel row)
    {
        if (row.IsSelf)
        {
            Toast.Warning("不能停用当前登录账号。");
            return;
        }

        var target = row.User.Status == AccountStatus.Active ? AccountStatus.Disabled : AccountStatus.Active;
        var verb = target == AccountStatus.Disabled ? "停用" : "启用";
        var confirmed = await Dialog.ConfirmAsync(
            $"{verb}账号",
            $"确定{verb}账号 {row.Username}（{row.RealName}）吗？",
            verb,
            "取消",
            danger: target == AccountStatus.Disabled);
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.SetUserStatusAsync(row.User.Id, target);
            await LoadAsync();
        }, $"账号 {row.Username} 已{verb}");
    }

    internal async Task DeleteAsync(AccountRowViewModel row)
    {
        if (row.IsSelf)
        {
            Toast.Warning("不能删除当前登录账号。");
            return;
        }

        var confirmed = await Dialog.ConfirmAsync(
            "删除账号",
            $"确定删除账号 {row.Username}（{row.RealName}）吗？历史单据中的经办人姓名快照会保留。",
            "删除",
            "取消");
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.DeleteUserAsync(row.User.Id);
            await LoadAsync();
        }, $"账号 {row.Username} 已删除");
    }
}
