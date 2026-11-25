using HospiKeeper.Application.Common;
using Sylvie233.Database;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>业务数据初始化：先执行迁移，再在空库中写入演示数据。</summary>
public sealed class HotelDataInitializer
{
    private readonly HotelDbContext _db;
    private readonly IDatabaseMigrator _migrator;
    private readonly IClock _clock;

    public HotelDataInitializer(HotelDbContext db, IDatabaseMigrator migrator, IClock clock)
    {
        _db = db;
        _migrator = migrator;
        _clock = clock;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _migrator.MigrateAsync(cancellationToken);
        await DemoDataSeeder.SeedAsync(_db, _clock.Now, cancellationToken);
    }
}
