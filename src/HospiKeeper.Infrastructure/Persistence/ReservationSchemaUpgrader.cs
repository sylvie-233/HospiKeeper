using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>
/// 游客预订 v1.1 的数据库结构补丁。
/// 按项目约定，本次结构调整直接维护 SQL 设计文件，不生成 dotnet-ef 迁移；
/// 启动时对 MySQL 做一次幂等检查，确保新库 / 旧库都能获得 `reservation` 的房型日期段字段。
/// </summary>
public sealed class ReservationSchemaUpgrader
{
    private readonly HotelDbContext _db;
    private readonly ILogger<ReservationSchemaUpgrader> _logger;

    public ReservationSchemaUpgrader(
        HotelDbContext db,
        ILogger<ReservationSchemaUpgrader> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        // InMemory 测试库没有关系型结构，无需补丁。
        if (!_db.Database.IsRelational()
            || _db.Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) != true)
        {
            return;
        }

        if (await IsCurrentAsync(cancellationToken))
        {
            _logger.LogDebug("reservation 已是 v1.1 结构，跳过结构补丁。");
            return;
        }

        await EnsureColumnAsync(
            "type_id",
            "ALTER TABLE `reservation` ADD COLUMN `type_id` INT NULL AFTER `guest_phone`",
            cancellationToken);

        await EnsureColumnAsync(
            "check_in_date",
            "ALTER TABLE `reservation` ADD COLUMN `check_in_date` DATE NULL AFTER `room_type_name`",
            cancellationToken);

        await EnsureColumnAsync(
            "check_out_date",
            "ALTER TABLE `reservation` ADD COLUMN `check_out_date` DATE NULL AFTER `check_in_date`",
            cancellationToken);

        await EnsureColumnAsync(
            "guest_id_card",
            "ALTER TABLE `reservation` ADD COLUMN `guest_id_card` VARCHAR(18) NULL AFTER `guest_phone`",
            cancellationToken);

        await EnsureColumnAsync(
            "channel",
            "ALTER TABLE `reservation` ADD COLUMN `channel` VARCHAR(16) NOT NULL DEFAULT 'WALK_IN' AFTER `check_out_date`",
            cancellationToken);

        await EnsureColumnAsync(
            "unit_price",
            "ALTER TABLE `reservation` ADD COLUMN `unit_price` DECIMAL(10,2) NULL AFTER `channel`",
            cancellationToken);

        // 旧数据：房型、日期段从已分配房间与到店时间回填。
        await _db.Database.ExecuteSqlRawAsync(
            """
            UPDATE `reservation` r
            JOIN `room` rm ON r.`room_id` = rm.`id`
            SET r.`type_id` = rm.`type_id`,
                r.`check_in_date` = DATE(r.`arrive_time`),
                r.`check_out_date` = DATE_ADD(DATE(r.`arrive_time`), INTERVAL 1 DAY)
            WHERE r.`type_id` IS NULL
               OR r.`check_in_date` IS NULL
               OR r.`check_out_date` IS NULL
            """,
            cancellationToken);

        // Web 预订允许不指定房间；前台代客预订仍会回填房号。
        await _db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `reservation` MODIFY COLUMN `room_id` INT NULL",
            cancellationToken);
        await _db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `reservation` MODIFY COLUMN `room_no` VARCHAR(16) NULL",
            cancellationToken);

        // 回填完成后收紧必填字段。
        await _db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `reservation` MODIFY COLUMN `type_id` INT NOT NULL",
            cancellationToken);
        await _db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `reservation` MODIFY COLUMN `check_in_date` DATE NOT NULL",
            cancellationToken);
        await _db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `reservation` MODIFY COLUMN `check_out_date` DATE NOT NULL",
            cancellationToken);

        await EnsureIndexAsync(
            "idx_type_dates",
            "CREATE INDEX `idx_type_dates` ON `reservation` (`type_id`, `check_in_date`, `check_out_date`)",
            cancellationToken);

        await EnsureConstraintAsync(
            "fk_res_type",
            "ALTER TABLE `reservation` ADD CONSTRAINT `fk_res_type` FOREIGN KEY (`type_id`) REFERENCES `room_type` (`id`)",
            cancellationToken);

        _logger.LogInformation("游客预订数据结构已就绪（reservation v1.1）。");
    }

    private async Task<bool> IsCurrentAsync(CancellationToken cancellationToken)
    {
        foreach (var column in new[] { "type_id", "check_in_date", "check_out_date", "channel", "guest_id_card", "unit_price" })
        {
            if (!await ExistsAsync(
                    "SELECT COUNT(*) FROM information_schema.COLUMNS " +
                    "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND COLUMN_NAME = @name",
                    column,
                    cancellationToken))
            {
                return false;
            }
        }

        var hasIndex = await ExistsAsync(
            "SELECT COUNT(*) FROM information_schema.STATISTICS " +
            "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND INDEX_NAME = @name",
            "idx_type_dates",
            cancellationToken);
        var hasConstraint = await ExistsAsync(
            "SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS " +
            "WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND CONSTRAINT_NAME = @name",
            "fk_res_type",
            cancellationToken);

        return hasIndex && hasConstraint;
    }

    private async Task EnsureColumnAsync(
        string column,
        string alterSql,
        CancellationToken cancellationToken)
    {
        if (await ExistsAsync(
                "SELECT COUNT(*) FROM information_schema.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND COLUMN_NAME = @name",
                column,
                cancellationToken))
        {
            return;
        }

        await _db.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);
        _logger.LogInformation("已补充 reservation.{Column} 字段。", column);
    }

    private async Task EnsureIndexAsync(
        string index,
        string createSql,
        CancellationToken cancellationToken)
    {
        if (await ExistsAsync(
                "SELECT COUNT(*) FROM information_schema.STATISTICS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND INDEX_NAME = @name",
                index,
                cancellationToken))
        {
            return;
        }

        await _db.Database.ExecuteSqlRawAsync(createSql, cancellationToken);
        _logger.LogInformation("已创建 reservation.{Index} 索引。", index);
    }

    private async Task EnsureConstraintAsync(
        string constraint,
        string alterSql,
        CancellationToken cancellationToken)
    {
        if (await ExistsAsync(
                "SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS " +
                "WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'reservation' AND CONSTRAINT_NAME = @name",
                constraint,
                cancellationToken))
        {
            return;
        }

        await _db.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);
        _logger.LogInformation("已创建 reservation.{Constraint} 外键。", constraint);
    }

    private async Task<bool> ExistsAsync(
        string sql,
        string parameter,
        CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var dbParameter = command.CreateParameter();
        dbParameter.ParameterName = "@name";
        dbParameter.Value = parameter;
        command.Parameters.Add(dbParameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }
}
