using HospiKeeper.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.App.Services;

/// <summary>登录页 / 主 Shell / 日志页之间的窗口与导航切换。</summary>
public sealed class AppNavigator
{
    private readonly IServiceProvider _services;

    public AppNavigator(IServiceProvider services) => _services = services;

    public void ShowShell()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            window.Page = _services.GetRequiredService<AppShell>();
        }
    }

    public void ShowLogin()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is null)
        {
            return;
        }

        var login = _services.GetRequiredService<LoginPage>();
        NavigationPage.SetHasNavigationBar(login, false);
        window.Page = new NavigationPage(login);
    }

    public async Task ShowLogsAsync()
    {
        var logs = _services.GetRequiredService<LogsPage>();
        if (Shell.Current is not null)
        {
            await Shell.Current.Navigation.PushModalAsync(logs);
        }
    }

    public static Task CloseModalAsync()
        => Shell.Current is null ? Task.CompletedTask : Shell.Current.Navigation.PopModalAsync();
}
