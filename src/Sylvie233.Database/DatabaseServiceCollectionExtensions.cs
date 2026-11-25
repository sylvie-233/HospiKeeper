using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sylvie233.Database;

/// <summary>共享数据库注册入口。</summary>
public static class DatabaseServiceCollectionExtensions
{
    /// <summary>注册指定 DbContext（MySQL 提供程序）。业务层只需定义自己的 DbContext 与映射。</summary>
    public static IServiceCollection AddSylvie233MySql<TContext>(
        this IServiceCollection services,
        DatabaseOptions options)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddDbContext<TContext>(builder => builder.UseMySQL(options.BuildConnectionString()));
        return services;
    }
}
