using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sylvie233.Database;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>供 dotnet-ef 生成迁移时使用的设计时上下文。</summary>
public sealed class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var options = DatabaseOptions.FromEnvironment("HOSPITKEEPER");

        var builder = new DbContextOptionsBuilder<HotelDbContext>()
            .UseMySQL(options.BuildConnectionString());

        return new HotelDbContext(builder.Options);
    }
}
