using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Messages;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public abstract class PageViewModelBase : ObservableObject
{
    protected PageViewModelBase(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation)
    {
        Api = api;
        Dialog = dialog;
        Toast = toast;
        Navigation = navigation;
    }

    protected IHospiKeeperApi Api { get; }
    protected DialogService Dialog { get; }
    protected ToastService Toast { get; }
    protected NavigationService Navigation { get; }

    public abstract string Key { get; }
    public abstract string Title { get; }
    public abstract Geometry Icon { get; }
    public virtual string Group => "前台运营";
    public virtual bool AdminOnly => false;
    public virtual int GroupOrder => 0;
    public virtual int PageOrder => 0;

    public virtual Task OnNavigatedToAsync(object? argument) => Task.CompletedTask;

    protected void NotifyDataChanged()
        => WeakReferenceMessenger.Default.Send(new DataChangedMessage());

    protected async Task RunAsync(Func<Task> action, string? successMessage = null)
    {
        try
        {
            await action();
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                Toast.Success(successMessage);
            }

            NotifyDataChanged();
        }
        catch (ApiException ex)
        {
            LogError(ex);
            Toast.Error(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            LogError(ex);
            Toast.Error("无法连接 API 服务，请确认 HospiKeeper.Api 已启动。");
        }
        catch (TaskCanceledException ex)
        {
            LogError(ex);
            Toast.Error("请求超时，请检查 API 服务。");
        }
        catch (Exception ex)
        {
            LogError(ex);
            Toast.Error(ex.Message);
        }
    }

    private static void LogError(Exception ex)
        => System.Diagnostics.Debug.WriteLine(ex);

    protected async Task<bool> TryLoadAsync(Func<Task> load)
    {
        try
        {
            await load();
            return true;
        }
        catch (ApiException ex)
        {
            LogError(ex);
            Toast.Error(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            LogError(ex);
            Toast.Error("无法连接 API 服务，请确认 HospiKeeper.Api 已启动。");
        }
        catch (Exception ex)
        {
            LogError(ex);
            Toast.Error($"加载数据失败：{ex.Message}");
        }

        return false;
    }
}
