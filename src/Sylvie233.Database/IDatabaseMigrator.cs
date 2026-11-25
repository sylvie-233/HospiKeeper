namespace Sylvie233.Database;

/// <summary>数据库迁移执行入口，启动时可先迁移再执行业务初始化。</summary>
public interface IDatabaseMigrator
{
    /// <summary>应用全部待执行迁移。</summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
