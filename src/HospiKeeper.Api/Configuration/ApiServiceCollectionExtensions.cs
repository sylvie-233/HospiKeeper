using System.Text.Json.Serialization;
using HospiKeeper.Api.OpenApi;
using HospiKeeper.Api.Security;
using HospiKeeper.Application;
using HospiKeeper.Contracts;
using HospiKeeper.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Sylvie233.Caching;
using Sylvie233.Database;

namespace HospiKeeper.Api.Configuration;

/// <summary>
/// API 组合根：集中注册数据库、缓存、业务服务、鉴权、MVC 与接口文档，避免 Program.cs 膨胀。
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>注册 HospiKeeper API 的全部依赖。</summary>
    public static IServiceCollection AddHospiKeeperApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var databaseOptions = BindDatabaseOptions(configuration);
        var cacheOptions = BindCacheOptions(configuration);

        services.AddSingleton(databaseOptions);
        services.AddInfrastructure(databaseOptions);
        services.AddCaching(cacheOptions);
        services.AddApplication();
        services.AddSingleton<ApiTokenStore>();

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var message = context.ModelState
                    .SelectMany(entry => entry.Value?.Errors ?? [])
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

                return new BadRequestObjectResult(
                    new ApiErrorResponse(message ?? "请求参数无效。"));
            };
        });

        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<HospiKeeperOpenApiDocumentTransformer>();
            options.AddOperationTransformer<HospiKeeperOpenApiOperationTransformer>();
        });

        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    /// <summary>绑定数据库配置并应用环境变量覆盖（业务前缀 → SYLVIE233_ → MYSQL_）。</summary>
    private static DatabaseOptions BindDatabaseOptions(IConfiguration configuration)
    {
        var options = new DatabaseOptions();
        configuration.GetSection("Database").Bind(options);
        DatabaseEnvironmentOverrides.Apply(options, "HOSPITKEEPER");
        return options;
    }

    /// <summary>绑定缓存配置；未配置时使用 Redis 作为本项目默认实现。</summary>
    private static CacheOptions BindCacheOptions(IConfiguration configuration)
    {
        var options = new CacheOptions();
        configuration.GetSection("Cache").Bind(options);
        return options;
    }
}
