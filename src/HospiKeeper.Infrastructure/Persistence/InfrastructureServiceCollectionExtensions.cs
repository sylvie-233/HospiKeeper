using HospiKeeper.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        // 本项目游客预订 v1.1 通过 SQL 维护结构（见 ReservationSchemaUpgrader），
        // EF 模型会领先于迁移快照，因此忽略启动时的待迁移模型检查警告。
        services.AddSylvie233MySql<HotelDbContext>(
            options,
            builder => builder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning)));
        services.AddScoped<IDatabaseMigrator, MySqlDatabaseMigrator<HotelDbContext>>();
        services.AddScoped<ReservationSchemaUpgrader>();
        services.AddScoped<HotelDataInitializer>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
