using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Client;

namespace HospiKeeper.App.ViewModels;

/// <summary>管理端登录：仅允许 ADMIN 账号。</summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly ApiProvider _apiProvider;
    private readonly AppSession _session;
    private readonly ToastService _toast;
    private readonly AppNavigator _navigator;

    public LoginViewModel(
        ApiProvider apiProvider,
        AppSession session,
        ToastService toast,
        AppNavigator navigator)
    {
        _apiProvider = apiProvider;
        _session = session;
        _toast = toast;
        _navigator = navigator;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        ChangeServerCommand = new AsyncRelayCommand(ChangeServerAsync);
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _isBusy;

    public string ServerText => _apiProvider.BaseUrl;

    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand ChangeServerCommand { get; }

    private async Task LoginAsync()
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "请输入账号和密码。";
            return;
        }

        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _apiProvider.Api.LoginAsync(Username.Trim(), Password);
            if (!response.User.IsAdmin)
            {
                await _apiProvider.Api.LogoutAsync();
                Error = "管理端仅限管理员账号登录。";
                return;
            }

            _session.SignIn(response.User);
            _toast.Success($"欢迎回来，{response.User.RealName}");
            _navigator.ShowShell();
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        catch (HttpRequestException)
        {
            Error = "无法连接 API，请点击下方服务器地址确认配置。";
        }
        catch (TaskCanceledException)
        {
            Error = "连接 API 超时，请检查服务器地址与网络。";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangeServerAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return;
        }

        var value = await page.DisplayPromptAsync(
            "API 服务器",
            "Android 模拟器访问宿主机通常使用 10.0.2.2；真机请填写电脑局域网 IP。",
            initialValue: _apiProvider.BaseUrl,
            keyboard: Keyboard.Url,
            placeholder: "http://10.0.2.2:5080",
            accept: "保存",
            cancel: "取消");

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!_apiProvider.SetBaseUrl(value))
        {
            Error = "服务器地址格式无效，请输入 http:// 或 https:// 开头的地址。";
            return;
        }

        Error = null;
        OnPropertyChanged(nameof(ServerText));
        _toast.Success("服务器地址已更新，请重新登录。");
    }
}
