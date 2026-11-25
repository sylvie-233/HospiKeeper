using HospiKeeper.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HospiKeeper.Infrastructure.Persistence;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        DatabaseOptions options)
    {
        services.AddDbContext<HotelDbContext>(builder =>
            builder.UseMySQL(options.BuildConnectionString()));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
