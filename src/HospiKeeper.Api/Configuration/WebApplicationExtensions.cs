using HospiKeeper.Api.Endpoints;
using HospiKeeper.Api.Middleware;
using HospiKeeper.Api.OpenApi;
using HospiKeeper.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;
using Sylvie233.Database;

namespace HospiKeeper.Api.Configuration;

/// <summary>API 启动流程：数据库初始化、中间件顺序与端点映射。</summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// 执行 EF Core 迁移与空库演示数据初始化。
    /// 数据库不可用时记录 Critical 日志并让进程快速失败，避免“带病启动”。
    /// </summary>
    public static async Task InitializeHospiKeeperDatabaseAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<HotelDataInitializer>().InitializeAsync();
            var databaseOptions = scope.ServiceProvider.GetRequiredService<DatabaseOptions>();
            app.Logger.LogInformation(
                "HospiKeeper EF Core / MySQL ready: {Database}",
                databaseOptions.SafeDisplay);
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(
                ex,
                "HospiKeeper 数据库初始化失败，请检查 Scripts/docker-compose.yml 中的 MySQL 配置");
            throw;
        }
    }

    /// <summary>
    /// 配置请求管道：请求日志 → 异常处理 → CORS → 接口文档 → 鉴权 → 端点。
    /// 请求日志必须在异常中间件外层，才能记录最终状态码。
    /// </summary>
    public static WebApplication UseHospiKeeperApi(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0} ms";
        });
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors();

        // 文档只在开发环境开放，生产环境不暴露 OpenAPI / Scalar。
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
                .AddPreferredSecuritySchemes(
                    HospiKeeperOpenApiDocumentTransformer.SecuritySchemeName));
        }

        app.UseMiddleware<ApiAuthMiddleware>();
        app.MapHealthEndpoints();
        app.MapControllers();

        return app;
    }
}
