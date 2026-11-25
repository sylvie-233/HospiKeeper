using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.App.Services;

/// <summary>供 Shell / ContentPage 在少量场景中解析 DI 服务。</summary>
public static class ServiceHelper
{
    public static IServiceProvider Services
        => IPlatformApplication.Current?.Services
           ?? throw new InvalidOperationException("MAUI 服务容器尚未初始化。");

    public static T Get<T>() where T : notnull
        => Services.GetRequiredService<T>();
}
