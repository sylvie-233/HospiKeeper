using HospiKeeper.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>应用启动时执行 EF Core 迁移，并在空库中写入演示数据。</summary>
public sealed class DatabaseInitializer
{
    private readonly HotelDbContext _db;
    private readonly IClock _clock;

    public DatabaseInitializer(HotelDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);
        await DemoDataSeeder.SeedAsync(_db, _clock.Now, cancellationToken);
    }
}
