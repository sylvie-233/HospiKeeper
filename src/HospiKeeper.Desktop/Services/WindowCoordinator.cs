using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.Desktop.Services;

/// <summary>登录窗口 ↔ 主窗口切换。</summary>
public sealed class WindowCoordinator
{
    private readonly IServiceProvider _services;
    private readonly ClientSession _session;
    private readonly IHospiKeeperApi _api;
    private LoginWindow? _login;
    private MainWindow? _main;

    public WindowCoordinator(IServiceProvider services, ClientSession session, IHospiKeeperApi api)
    {
        _services = services;
        _session = session;
        _api = api;
    }

    public void Start() => ShowLogin();

    public void EnterMain()
    {
        var main = _services.GetRequiredService<MainWindow>();
        _main = main;
        var lifetime = GetLifetime();
        main.Show();
        WindowCentering.Center(main);
        lifetime.MainWindow = main;
        _login?.Close();
        _login = null;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _api.LogoutAsync();
        }
        catch
        {
            // 网络异常时也允许本地退出
        }

        _session.SignOut();
        var login = _services.GetRequiredService<LoginWindow>();
        _login = login;
        var lifetime = GetLifetime();
        login.Show();
        WindowCentering.Center(login);
        lifetime.MainWindow = login;
        var oldMain = _main;
        _main = null;
        oldMain?.Close();
    }

    private void ShowLogin()
    {
        var login = _services.GetRequiredService<LoginWindow>();
        _login = login;
        login.Show();
        WindowCentering.Center(login);
        GetLifetime().MainWindow = login;
    }

    private static IClassicDesktopStyleApplicationLifetime GetLifetime()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime;
        }

        throw new InvalidOperationException("桌面应用生命周期不可用。");
    }
}
