# HospiKeeper.Infrastructure · EF Core 持久化

业务持久化模块，包含 `HotelDbContext`、表映射、仓储、EF Core 迁移、启动初始化和演示数据。共享的 MySQL 连接 / 注册 / 迁移执行在 [Sylvie233.Database](../Sylvie233.Database/README.md)。

## 结构

```text
Persistence/
├─ HotelDbContext.cs
├─ Configurations/          # snake_case 表名与字段映射
├─ Repositories/            # 实现 Application 的仓储接口
├─ Migrations/              # dotnet-ef 生成，不手改
├─ DemoDataSeeder.cs
└─ HotelDataInitializer.cs
```

## 启动初始化

```text
HotelDataInitializer
  ├─ IDatabaseMigrator.MigrateAsync()     # 共享模块
  ├─ ReservationSchemaUpgrader             # 游客预订 v1.1 幂等补列
  └─ DemoDataSeeder.SeedAsync()            # 空库演示数据
```

游客预订的房型 / 日期段 / 渠道字段直接维护在 `Docs/民宿管理系统-数据库设计-v1.sql`，由 `ReservationSchemaUpgrader` 在启动时补列和回填，不为其生成 dotnet-ef 迁移。

## 工作单元

- 业务服务只依赖 `IUnitOfWork`，不出现 `DbContext` / EF API。
- `EfUnitOfWork` 聚合仓储，`SaveChangesAsync` 统一提交。
- 一次业务动作的多表变更应保持在同一工作单元内。
- `DocumentNumberRepository` 使用 `doc_sequence` 表生成入住 `I`、结算 `S`、预定 `R` 单号；同一工作单元内优先读取已跟踪行，避免重复。

## 迁移

```bash
dotnet tool restore

dotnet ef migrations add <Name> \
  --project src/HospiKeeper.Infrastructure \
  --startup-project src/HospiKeeper.Api \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/HospiKeeper.Infrastructure \
  --startup-project src/HospiKeeper.Api
```

- 迁移文件不要手改；需要调整时修改实体 / `Configurations` 后重新生成。
- API 启动时自动执行迁移；测试使用 EF Core InMemory + `DemoDataSeeder`，不连接真实 MySQL。
- `Docs/民宿管理系统-数据库设计-v1.sql` 仅作表结构设计参考，EF 模型是唯一事实来源（游客预订字段除外）。

## 新增实体清单

1. `Domain/Entities` 实体；
2. `HotelDbContext` DbSet；
3. `Configurations/*Configuration.cs` 映射；
4. 仓储接口与实现（如需要）；
5. EF 迁移；
6. 业务服务与测试。
