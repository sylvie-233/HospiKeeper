using Avalonia;
using HospiKeeper.Desktop.Services;
using Sylvie233.Logging;

namespace HospiKeeper.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        DesktopLogging.Initialize();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Sylvie233Log.Logger.Fatal(ex, "桌面端启动失败");
            throw;
        }
        finally
        {
            Sylvie233Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

internal static class DesktopLogging
{
    public static void Initialize()
    {
        var options = LoggingSettings.Load();
        Sylvie233Log.Initialize(options);
        Sylvie233Log.ForContext(typeof(DesktopLogging)).Information("HospiKeeper Desktop 启动");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Sylvie233Log.Logger.Error(e.ExceptionObject as Exception, "未处理的应用异常");

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Sylvie233Log.Logger.Error(e.Exception, "未观察的异步任务异常");
            e.SetObserved();
        };
    }
}
