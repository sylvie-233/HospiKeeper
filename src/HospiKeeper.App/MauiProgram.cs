using HospiKeeper.App.Services;
using HospiKeeper.App.ViewModels;
using HospiKeeper.App.Views;
using Microsoft.Extensions.Logging;

namespace HospiKeeper.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<ApiProvider>();
        builder.Services.AddSingleton<AppSession>();
        builder.Services.AddSingleton<ToastService>();
        builder.Services.AddSingleton<AppNavigator>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<RoomsViewModel>();
        builder.Services.AddTransient<RoomsPage>();
        builder.Services.AddTransient<ReservationsViewModel>();
        builder.Services.AddTransient<ReservationsPage>();
        builder.Services.AddTransient<StaysViewModel>();
        builder.Services.AddTransient<StaysPage>();
        builder.Services.AddTransient<MineViewModel>();
        builder.Services.AddTransient<MinePage>();
        builder.Services.AddTransient<LogsViewModel>();
        builder.Services.AddTransient<LogsPage>();

        builder.Services.AddTransient<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
