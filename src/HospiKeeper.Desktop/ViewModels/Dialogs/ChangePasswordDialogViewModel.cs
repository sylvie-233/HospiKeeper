using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>修改密码弹窗。</summary>
public sealed partial class ChangePasswordDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;

    public ChangePasswordDialogViewModel(DialogService host, IHospiKeeperApi api, ToastService toast) : base(host)
    {
        _api = api;
        _toast = toast;
        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
    }

    public override string Title => "修改密码";

    public override double Width => 460;

    [ObservableProperty]
    private string _oldPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _oldPasswordError;

    [ObservableProperty]
    private string? _newPasswordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        OldPasswordError = string.IsNullOrWhiteSpace(OldPassword) ? "请输入原密码" : null;
        NewPasswordError = NewPassword.Length < 6 ? "新密码至少 6 位" : null;
        ConfirmPasswordError = ConfirmPassword != NewPassword ? "两次输入的新密码不一致" : null;
        SubmitError = null;

        if (OldPasswordError is not null || NewPasswordError is not null || ConfirmPasswordError is not null)
        {
            return;
        }

        try
        {
            await _api.ChangePasswordAsync(new ChangePasswordRequest(OldPassword, NewPassword, ConfirmPassword));
            _toast.Success("密码修改成功，请牢记新密码。");
            Host.Close();
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
