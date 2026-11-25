using CommunityToolkit.Mvvm.ComponentModel;
using HospiKeeper.App.Services;
using HospiKeeper.Client;
using HospiKeeper.Contracts;

namespace HospiKeeper.App.ViewModels;

/// <summary>App 页面 ViewModel 基类：统一 API、会话、提示与错误处理。</summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected BaseViewModel(ApiProvider apiProvider, AppSession session, ToastService toast)
    {
        ApiProvider = apiProvider;
        Session = session;
        Toast = toast;
    }

    protected ApiProvider ApiProvider { get; }
    protected IHospiKeeperApi Api => ApiProvider.Api;
    protected AppSession Session { get; }
    protected ToastService Toast { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _lastSyncText = "尚未同步";

    public string ServerText => ApiProvider.BaseUrl;

    protected async Task ExecuteAsync(Func<Task> action, string? successMessage = null)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await action();
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                Toast.Success(successMessage);
            }
        }
        catch (ApiException ex)
        {
            Toast.Error(ex.Message);
        }
        catch (HttpRequestException)
        {
            Toast.Error("无法连接 API，请检查登录页的服务器地址与网络。");
        }
        catch (TaskCanceledException)
        {
            Toast.Error("请求超时，请稍后重试。");
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void MarkSynced() => LastSyncText = $"同步于 {DateTime.Now:HH:mm}";

    public virtual Task OnAppearingAsync() => Task.CompletedTask;
}
