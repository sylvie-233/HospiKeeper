using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>供 dotnet-ef 生成迁移时使用的设计时上下文。</summary>
public sealed class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var options = new DatabaseOptions
        {
            Host = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_HOST")
                   ?? Environment.GetEnvironmentVariable("MYSQL_HOST")
                   ?? "127.0.0.1",
            Name = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_NAME")
                   ?? Environment.GetEnvironmentVariable("MYSQL_DATABASE")
                   ?? "hospikeeper",
            User = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_USER")
                   ?? Environment.GetEnvironmentVariable("MYSQL_USER")
                   ?? "hospikeeper",
            Password = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_PASSWORD")
                       ?? Environment.GetEnvironmentVariable("MYSQL_PASSWORD")
                       ?? "123456"
        };

        var builder = new DbContextOptionsBuilder<HotelDbContext>()
            .UseMySQL(options.BuildConnectionString());

        return new HotelDbContext(builder.Options);
    }
}
