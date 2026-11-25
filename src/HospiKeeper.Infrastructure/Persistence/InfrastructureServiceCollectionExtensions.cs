using HospiKeeper.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sylvie233.Database;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>基础设施注册：共享数据库组件 + 业务仓储 / 初始化器。</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>组合共享数据库组件与酒店业务仓储、初始化器。</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        DatabaseOptions options)
    {
        services.AddSylvie233MySql<HotelDbContext>(options);
        services.AddScoped<IDatabaseMigrator, MySqlDatabaseMigrator<HotelDbContext>>();
        services.AddScoped<HotelDataInitializer>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
