using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels;

/// <summary>登录窗口 ViewModel。</summary>
public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IHospiKeeperApi _api;
    private readonly ClientSession _session;
    private readonly WindowCoordinator _coordinator;
    private readonly AppSettings _settings;

    public LoginViewModel(
        IHospiKeeperApi api,
        ClientSession session,
        WindowCoordinator coordinator,
        AppSettings settings)
    {
        _api = api;
        _session = session;
        _coordinator = coordinator;
        _settings = settings;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorText;

    [ObservableProperty]
    private bool _isBusy;

    public IAsyncRelayCommand LoginCommand { get; }

    partial void OnUsernameChanged(string value) => ErrorText = null;

    partial void OnPasswordChanged(string value) => ErrorText = null;

    private async Task LoginAsync()
    {
        ErrorText = null;
        IsBusy = true;
        try
        {
            var response = await _api.LoginAsync(Username.Trim(), Password);
            _session.SignIn(response.User);
            _coordinator.EnterMain();
        }
        catch (ApiException ex)
        {
            ErrorText = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorText = $"无法连接 API 服务（{_settings.ApiBaseUrl}）。\n请先在命令行运行：dotnet run --project src/HospiKeeper.Api";
        }
        catch (TaskCanceledException)
        {
            ErrorText = "连接 API 服务超时，请检查服务是否已启动。";
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
