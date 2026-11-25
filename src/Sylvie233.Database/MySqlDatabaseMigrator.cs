using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Sylvie233.Database;

/// <summary>基于 EF Core 的通用迁移执行器：记录待应用迁移与执行结果。</summary>
public sealed class MySqlDatabaseMigrator<TContext> : IDatabaseMigrator
    where TContext : DbContext
{
    private readonly TContext _db;
    private readonly ILogger<MySqlDatabaseMigrator<TContext>> _logger;

    public MySqlDatabaseMigrator(TContext db, ILogger<MySqlDatabaseMigrator<TContext>> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        var pending = (await _db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            _logger.LogInformation(
                "{Context} 待应用迁移 {Count} 个：{Migrations}",
                typeof(TContext).Name,
                pending.Count,
                string.Join(", ", pending));
        }
        else
        {
            _logger.LogInformation("{Context} 数据库结构已是最新。", typeof(TContext).Name);
        }

        await _db.Database.MigrateAsync(cancellationToken);
    }
}
