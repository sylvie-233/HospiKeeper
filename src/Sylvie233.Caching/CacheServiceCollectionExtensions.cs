using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Sylvie233.Caching;

public static class CacheServiceCollectionExtensions
{
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
