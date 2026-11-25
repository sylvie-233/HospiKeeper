using HospiKeeper.App.Services;
using Microsoft.Extensions.DependencyInjection;
using HospiKeeper.App.Views;

namespace HospiKeeper.App;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var login = _services.GetRequiredService<LoginPage>();
        NavigationPage.SetHasNavigationBar(login, false);
        return new Window(new NavigationPage(login)) { Title = "云栖民宿管理" };
    }
}
