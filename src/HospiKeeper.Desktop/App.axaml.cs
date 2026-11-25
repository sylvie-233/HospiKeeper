using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels;
using HospiKeeper.Desktop.ViewModels.Pages;
using HospiKeeper.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.Desktop;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            Services = BuildServices();
            var coordinator = Services.GetRequiredService<WindowCoordinator>();
            coordinator.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider BuildServices()
    {
        var settings = AppSettings.Load();
        var services = new ServiceCollection();

        services.AddSingleton(settings);
        services.AddSingleton<ClientSession>();
        services.AddSingleton<ToastService>();
        services.AddSingleton<DialogService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<WindowCoordinator>();
        services.AddSingleton<IHospiKeeperApi>(_ => new HospiKeeperApiClient(new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(25)
        }));

        services.AddSingleton<PageViewModelBase, DashboardPageViewModel>();
        services.AddSingleton<PageViewModelBase, CheckInPageViewModel>();
        services.AddSingleton<PageViewModelBase, ReservationsPageViewModel>();
        services.AddSingleton<PageViewModelBase, StayPageViewModel>();
        services.AddSingleton<PageViewModelBase, RecordsPageViewModel>();
        services.AddSingleton<PageViewModelBase, ReportPageViewModel>();
        services.AddSingleton<PageViewModelBase, LogsPageViewModel>();
        services.AddSingleton<PageViewModelBase, RoomsPageViewModel>();
        services.AddSingleton<PageViewModelBase, RoomTypesPageViewModel>();
        services.AddSingleton<PageViewModelBase, AccountsPageViewModel>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginWindow>();

        return services.BuildServiceProvider();
    }
}
