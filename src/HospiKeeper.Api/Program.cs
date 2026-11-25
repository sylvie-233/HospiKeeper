using System.Text.Json.Serialization;
using HospiKeeper.Api.Middleware;
using HospiKeeper.Api.OpenApi;
using HospiKeeper.Api.Security;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Abstractions.Caching;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;
using Sylvie233.Caching;
using Sylvie233.Database;
using Sylvie233.Logging;

var builder = WebApplication.CreateBuilder(args);

var loggingOptions = new LoggingOptions
{
    ApplicationName = "HospiKeeper.Api",
    FileNamePrefix = "api"
};
builder.Configuration.GetSection("Logging:Sylvie233").Bind(loggingOptions);

// 宿主构建前的启动日志（数据库 / 配置错误也能落盘）
Sylvie233Log.Initialize(loggingOptions);
builder.Host.UseSylvie233Logging(loggingOptions);

try
{
    var dbOptions = new DatabaseOptions();
    builder.Configuration.GetSection("Database").Bind(dbOptions);
    DatabaseEnvironmentOverrides.Apply(dbOptions, "HOSPITKEEPER");

    var cacheOptions = new CacheOptions();
    builder.Configuration.GetSection("Cache").Bind(cacheOptions);

    builder.Services.AddSingleton(dbOptions);
    builder.Services.AddSingleton<IClock, SystemClock>();
    builder.Services.AddInfrastructure(dbOptions);
    builder.Services.AddCaching(cacheOptions);
    builder.Services.AddSingleton<IHotelCacheInvalidator, HotelCacheInvalidator>();
    builder.Services.AddSingleton<ApiTokenStore>();

    builder.Services.AddScoped<SessionContext>();
    builder.Services.AddScoped<IOperationLogger, OperationLogger>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRoomService, RoomService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    builder.Services.AddScoped<IRecordService, RecordService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer<HospiKeeperOpenApiDocumentTransformer>();
        options.AddOperationTransformer<HospiKeeperOpenApiOperationTransformer>();
    });
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var app = builder.Build();

    // 启动时执行 EF Core 迁移与首次演示数据灌入；数据库不可用时快速失败并打印明确信息。
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<HotelDataInitializer>().InitializeAsync();
        app.Logger.LogInformation("HospiKeeper EF Core / MySQL ready: {Database}", dbOptions.SafeDisplay);
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "HospiKeeper 数据库初始化失败，请检查 Scripts/docker-compose.yml 中的 MySQL 配置");
        throw;
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0} ms";
    });
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseCors();

    // 接口文档只在开发环境开放，生产环境不暴露 OpenAPI / Scalar。
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithTitle("HospiKeeper API")
            .AddDocument(
                "v1",
                "HospiKeeper API",
                "/openapi/{documentName}.json",
                isDefault: true,
                agentKey: null)
            .AddPreferredSecuritySchemes(HospiKeeperOpenApiDocumentTransformer.SecuritySchemeName));
    }

    app.UseMiddleware<ApiAuthMiddleware>();

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "ok",
        storage = "mysql",
        time = DateTime.Now
    }))
    .WithTags("健康检查")
    .WithSummary("健康检查")
    .WithDescription("返回 API 与存储状态，无需鉴权。");

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HospiKeeper.Api 启动失败");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
