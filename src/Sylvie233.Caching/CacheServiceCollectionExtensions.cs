using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Sylvie233.Caching;

/// <summary>缓存依赖注册：根据配置选择 Memory 或 Redis，并向业务层暴露 <see cref="ICacheService"/>。</summary>
public static class CacheServiceCollectionExtensions
{
    /// <summary>注册缓存实现；Redis 连接失败不会阻止应用启动，运行期按未命中降级。</summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        CacheOptions options)
    {
        Normalize(options);
        services.AddSingleton<IOptions<CacheOptions>>(Options.Create(options));

        if (options.Provider == CacheProvider.Redis)
        {
            var configuration = ConfigurationOptions.Parse(options.RedisConnectionString);
            configuration.AbortOnConnectFail = false;
            configuration.ConnectRetry = 2;
            configuration.ConnectTimeout = 3000;
            configuration.AsyncTimeout = 1500;
            configuration.SyncTimeout = 1500;

            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        return services;
    }

    private static void Normalize(CacheOptions options)
    {
        options.InstanceName = string.IsNullOrWhiteSpace(options.InstanceName)
            ? "hospikeeper:"
            : options.InstanceName.Trim();

        if (!options.InstanceName.EndsWith(':'))
        {
            options.InstanceName += ":";
        }

        if (options.DefaultExpirationSeconds <= 0)
        {
            options.DefaultExpirationSeconds = 60;
        }
    }
}
