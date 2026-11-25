# Sylvie233.Database · MySQL 共享组件

只负责“连接 + 注册 + 迁移执行”，不感知任何业务表。业务 `DbContext`、映射、仓储、迁移与种子仍留在 `HospiKeeper.Infrastructure`。

## 组成

- `DatabaseOptions`：Host / Port / Name / User / Password，连接串构造与 `SafeDisplay`；
- `DatabaseEnvironmentOverrides`：按“业务前缀 → `SYLVIE233_` → `MYSQL_`”解析环境变量；
- `AddSylvie233MySql<TContext>(options)`：以 MySQL Provider 注册任意 DbContext；
- `IDatabaseMigrator` / `MySqlDatabaseMigrator<TContext>`：记录待应用迁移并执行 `MigrateAsync`。

## 使用

API 组合根注册 `HotelDbContext` 时调用共享扩展，并通过 `IDatabaseMigrator` 执行迁移。`HospiKeeper.Infrastructure` 的 `HotelDataInitializer` 顺序为：

```text
HotelDataInitializer
  ├─ IDatabaseMigrator.MigrateAsync()
  ├─ ReservationSchemaUpgrader
  └─ DemoDataSeeder.SeedAsync()
```

## 边界

- 不引用任何 HospiKeeper 业务模块；
- 不包含迁移文件、表映射、仓储或种子；
- 可被其他 .NET + MySQL 项目直接复用。

数据库结构、迁移命令与新增实体流程见 [Infrastructure README](../HospiKeeper.Infrastructure/README.md)。
