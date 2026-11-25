using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>角色下拉选项。</summary>
public sealed record RoleOption(UserRole Value, string Text);

/// <summary>状态下拉选项。</summary>
public sealed record StatusOption(AccountStatus Value, string Text);

/// <summary>账号新增 / 编辑弹窗。</summary>
public sealed partial class AccountEditDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly UserDto? _user;
    private readonly bool _isSelf;
    private readonly Func<Task> _onSaved;

    public AccountEditDialogViewModel(
        DialogService host,
        ToastService toast,
        IHospiKeeperApi api,
        UserDto? user,
        bool isSelf,
        Func<Task> onSaved) : base(host)
    {
        _toast = toast;
        _api = api;
        _user = user;
        _isSelf = isSelf;
        _onSaved = onSaved;

        RoleOptions = new ObservableCollection<RoleOption>
        {
            new(UserRole.Front, "前台"),
            new(UserRole.Admin, "管理员")
        };
        StatusOptions = new ObservableCollection<StatusOption>
        {
            new(AccountStatus.Active, "启用"),
            new(AccountStatus.Disabled, "停用")
        };

        _selectedRole = user is null ? RoleOptions[0] : RoleOptions.First(r => r.Value == user.Role);
        _selectedStatus = user is null ? StatusOptions[0] : StatusOptions.First(s => s.Value == user.Status);

        if (user is not null)
        {
            Username = user.Username;
            RealName = user.RealName;
        }

        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
    }

    public override string Title => _user is null ? "新增操作员账号" : "编辑操作员账号";
    public override double Width => 520;

    public ObservableCollection<RoleOption> RoleOptions { get; }
    public ObservableCollection<StatusOption> StatusOptions { get; }

    public bool CanEditRoleStatus => !_isSelf;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _realName = string.Empty;

    [ObservableProperty]
    private RoleOption _selectedRole;

    [ObservableProperty]
    private StatusOption _selectedStatus;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _usernameError;

    [ObservableProperty]
    private string? _realNameError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _submitError;

    public string PasswordHint => _user is null ? "初始密码至少 6 位" : "留空则不修改密码；填写则至少 6 位";

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        UsernameError = string.IsNullOrWhiteSpace(Username) ? "请输入登录账号" : null;
        RealNameError = string.IsNullOrWhiteSpace(RealName) ? "请输入姓名" : null;
        var requirePassword = _user is null;
        PasswordError = requirePassword && string.IsNullOrWhiteSpace(Password)
            ? "请输入初始密码"
            : !string.IsNullOrEmpty(Password) && Password.Length < 6
                ? "密码至少 6 位"
                : null;
        SubmitError = null;

        if (UsernameError is not null || RealNameError is not null || PasswordError is not null)
        {
            return;
        }

        try
        {
            var input = new AccountInput(
                Username.Trim(),
                RealName.Trim(),
                _isSelf && _user is not null ? _user.Role : SelectedRole.Value,
                _isSelf && _user is not null ? _user.Status : SelectedStatus.Value,
                string.IsNullOrWhiteSpace(Password) ? null : Password);

            if (_user is null)
            {
                await _api.CreateUserAsync(input);
                _toast.Success($"账号 {input.Username} 已创建");
            }
            else
            {
                await _api.UpdateUserAsync(_user.Id, input);
                _toast.Success($"账号 {input.Username} 已更新");
            }

            Host.Close();
            await _onSaved();
        }
        catch (ApiException ex)
        {
            SubmitError = ex.Message;
        }
        catch (Exception ex)
        {
            SubmitError = ex.Message;
        }
    }
}
