using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Abstractions.Caching;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.Application;

/// <summary>
/// 业务层依赖注册入口。API、测试宿主或未来的 Blazor 服务端都可复用同一套注册。
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>注册时钟、会话、业务服务与缓存失效协调器。</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<SessionContext>();
        services.AddScoped<IOperationLogger, OperationLogger>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IRecordService, RecordService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IGuestBookingService, GuestBookingService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();

        services.AddSingleton<IHotelCacheInvalidator, HotelCacheInvalidator>();

        return services;
    }
}
