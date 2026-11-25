# CLAUDE.md · HospiKeeper 开发指引

本文件为 Claude Code / 后续开发者提供项目约定。所有说明以 **当前代码** 为准；修改架构或命令时请同步更新本文件。

## 1. 项目定位

HospiKeeper（云栖民宿管理系统）是面向中小型民宿 / 小旅馆的桌面经营系统，覆盖：

- 登录与角色权限
- 房态看板（六种房态、快捷操作、筛选、图例）
- 入住登记（议价、押金、同住人）
- 预定管理（待到店 / 转入住 / 取消）
- 在住与退房结算（追加消费、损坏扣费、加钟、押金抵扣）
- 入住 / 账单记录（票据式详情）
- 统计报表（指标卡、7 日柱状图、区间营收、房型分析）
- 操作日志
- 房间 / 房型 / 操作员账号管理

项目同时承担 **技术栈验证** 目标：.NET 10 + Avalonia 12 + ASP.NET Core 10 + EF Core 10 + MySQL 8。

## 2. 技术栈与版本

| 组件 | 版本 / 说明 |
|------|-------------|
| .NET | 10（LTS），目标框架 `net10.0` |
| Avalonia | 12.2 |
| CommunityToolkit.Mvvm | 8.4 |
| ASP.NET Core | 10（Web API + Controllers + Swagger） |
| EF Core | 10.0.12 |
| MySQL Provider | `MySql.EntityFrameworkCore` 10.0.12（官方 Provider，底层 MySql.Data） |
| MySQL | 8（Docker Compose） |
| 测试 | xUnit 2.9 + EF Core InMemory |
| 客户端 | `HospiKeeper.Client` 内手写 `HttpClient` 封装 |

版本统一由根目录 `Directory.Packages.props` 管理；不要在子项目中硬编码版本。

## 3. 模块与依赖规则

```
HospiKeeper.Domain          实体 / 枚举 / 计费领域服务，无框架依赖
HospiKeeper.Contracts       前后端共享请求、响应、DTO（API 契约）
HospiKeeper.Application     业务服务、仓储接口、工作单元接口、会话
HospiKeeper.Infrastructure  EF Core DbContext / 配置 / 仓储 / 迁移 / 种子
HospiKeeper.Api             独立 Web API（控制器、鉴权、Swagger）
HospiKeeper.Client          桌面端 HTTP 客户端
HospiKeeper.UI              Avalonia 设计系统与自研控件（不依赖业务模块）
HospiKeeper.Desktop         Avalonia 桌面端（MVVM 页面与弹窗）
```

依赖方向必须保持：

- `Api → Application / Infrastructure / Contracts`
- `Infrastructure → Application / Contracts`
- `Application → Contracts → Domain`
- `Desktop → Client / Contracts / UI`
- `UI` 不得引用 `Domain` / `Application` / `Api`

新增 Blazor 等客户端时：引用 `Contracts` + `Client`，只通过 API 访问业务，不直连数据库。

## 4. 常用命令

```bash
# 构建 / 测试
dotnet build HospiKeeper.slnx
dotnet test

# 启动 MySQL
docker compose -f Scripts/docker-compose.yml up -d --wait

# 启动 API（http://localhost:5080，Swagger /swagger）
dotnet run --project src/HospiKeeper.Api

# 启动桌面端
dotnet run --project src/HospiKeeper.Desktop

# 一键开发启动
./Scripts/run-dev.ps1
```

### EF Core 迁移

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

约定：

- 迁移文件位于 `src/HospiKeeper.Infrastructure/Persistence/Migrations`，不要手改生成文件；如需调整请修改实体 / 配置后重新生成。
- API 启动时自动执行 `Database.MigrateAsync()`；空库首次启动会调用 `DemoDataSeeder.SeedAsync` 写入演示数据。
- `Docs/民宿管理系统-数据库设计-v1.sql` 仅作为表结构设计参考，EF 模型为唯一事实来源。
- `Scripts/docker-compose.yml` 不再挂载 SQL 初始化脚本。

## 5. 持久化架构

- 入口：`HotelDbContext`（`src/HospiKeeper.Infrastructure/Persistence/HotelDbContext.cs`）
- 映射：`Persistence/Configurations/*Configuration.cs`，表名与字段使用数据库设计的 snake_case。
- 仓储：`Persistence/Repositories/*Repository.cs`，实现 `Application/Abstractions/Repositories`。
- 工作单元：`EfUnitOfWork` 聚合仓储，`SaveChangesAsync` 统一提交，保证一次业务动作的多表变更原子性。
- 业务服务只依赖 `IUnitOfWork`，不出现 `DbContext` / EF API。
- 单据序号：`DocumentNumberRepository` 使用 `doc_sequence` 表，`FindAsync` 会优先读取当前上下文已跟踪的行，避免同一工作单元内重复序号。
- 测试使用 `Microsoft.EntityFrameworkCore.InMemory`，通过 `ServiceTestContext` 构造完整服务栈并调用 `DemoDataSeeder.SeedAsync`。

新增实体时需同步：

1. `Domain/Entities` 实体；
2. `HotelDbContext` DbSet；
3. `Persistence/Configurations` 映射；
4. 仓储接口与实现（如需要）；
5. EF 迁移；
6. 业务服务与测试。

## 6. 业务规则

- **房态**：`Free / Occupied / Reserved / Cleaning / Repair / Disabled`；看板颜色与文案由 UI 映射。
- **计费**：
  - 按天：不足一晚按一晚，跨 12:00 视为新一天（实现为 `ceil(时长/24h)`，至少 1 晚）。
  - 加钟：小时数向上取整，房费 = `ceil(小时 × 成交单价 ÷ 24)`。
  - 应收 = 房费 + 追加消费 + 损坏扣费；差额 = 应收 − 押金。
- **单号**：入住 `I`、结算 `S`、预定 `R`，格式 `前缀+yyyyMMdd+3位序号`。
- **房态流转**：
  - 入住：Free → Occupied（或 Reserved → Occupied，同时预定转 Converted）
  - 退房：Occupied → Cleaning
  - 保洁完成 / 恢复：Cleaning / Repair / Disabled → Free
  - 预定创建：Free → Reserved；取消：Reserved → Free
- **权限**：
  - 前台可开房 / 退房 / 追加消费 / 打扫；记录、账单、日志仅看本人经办。
  - 管理员额外管理房间、房型、账号，并查看全部记录。
  - 服务端必须校验，不能只依赖 UI 隐藏。
- **房间删除**：有在住 / 预定 / 历史单据的房间禁止删除；可停用。
- **房型改名**：同步所有房间冗余 `TypeName`；改价写「改价」日志。
- **账号**：不能停用 / 删除当前登录账号；至少保留一名启用管理员。

## 7. API 约定

- 路由统一 `api/<resource>`，返回领域实体或 `Contracts` DTO；JSON 枚举以字符串输出。
- 登录：`POST /api/auth/login` 返回 Bearer Token；除 `/health`、`/swagger`、登录接口外均需 `Authorization: Bearer`。
- Token 为内存会话（12 小时）；服务端每次请求重新加载用户，角色 / 停用状态即时生效。
- 业务异常 `DomainException` 由 `ExceptionHandlingMiddleware` 转为 400 / 403；未处理异常返回 500。
- 控制器保持轻薄：参数绑定 → 调用 Application 服务 → 返回结果；不要在控制器写业务逻辑。
- 桌面端接入点：`HospiKeeper.Client/IHospiKeeperApi` / `HospiKeeperApiClient`。

## 8. UI 约定

- 设计系统在 `HospiKeeper.UI`：
  - 控件：`AntButton / AntTag / AntCard / AntStatCard / AntPageHeader / AntFormItem / AntSectionTitle / AntModal / AntRoomCard / AntBarChart / AntEmpty / AntToast / AntTable / AntIcons`
  - 主题：`Theme/AntResources.axaml`、`Theme/AntControls.axaml`、`Theme/AntStyles.axaml`
- 页面统一使用 `AntPageHeader` + `AntCard` + `AntTable` 的组合，风格参考 Ant Design（主色 `#1677FF`，28-32px 紧凑控件）。
- Avalonia 12 已废弃 `DataGrid`；表格统一使用自研 `AntTable`。
- Desktop 使用 MVVM：View 只绑定 ViewModel，业务调用全部经 `IHospiKeeperApi`。
- 弹窗统一由 `DialogService` 管理；成功 / 失败提示统一走 `ToastService`。
- 页面不展示演示账号、测试口令等演示信息。
- 新控件需同时提供 C# 属性与 `ControlTheme`，并在 `AntControls.axaml` / `AntStyles.axaml` 中注册。

## 9. 代码风格

- 命名空间与类型命名使用英文；用户可见文案使用中文。
- 使用 `Directory.Build.props` 中的 `Nullable`、`ImplicitUsings`；不要关闭可空检查。
- 业务服务方法使用 `Async` 后缀并接收 `CancellationToken`（默认参数 `default`）。
- 输入校验统一使用 `InputGuard`；业务错误抛 `DomainException`。
- 时间统一通过 `IClock`（测试可替换）；除种子 / 展示文案外避免直接使用 `DateTime.Now`。
- 不要在 Application / Domain 引入 EF Core、Avalonia 或 ASP.NET Core 依赖。
- 删除不再使用的代码、图标、样式与包；不要保留“以后可能用到”的空实现。

## 10. 测试

- 位置：`tests/HospiKeeper.Tests`
- `ServiceTestContext` 使用 EF Core InMemory + `DemoDataSeeder` 构造完整业务栈，并默认登录 `admin`。
- 新增业务规则必须补充测试；当前覆盖 25 个用例。
- 不要用测试去连接真实 MySQL；需要集成验证时直接启动 API 进行手工 / curl 冒烟。

## 11. Git 与协作

- 项目按阶段 / 模块提交，**直接在 `main` 分支操作**（该项目的约定）。
- 提交信息建议使用：

```
feat(<module>): <中文说明>
fix(<module>): <中文说明>
refactor(<module>): <中文说明>
test: <中文说明>
docs: <中文说明>
chore: <中文说明>
```

- 提交前至少执行：

```bash
dotnet build HospiKeeper.slnx
dotnet test
```

- 不提交 `bin/`、`obj/`、本地日志、数据库文件或密钥。
- `appsettings.json` 中为本地开发默认口令，生产部署必须改为环境变量 / 密钥管理。

## 12. 后续 Blazor 模块

计划新增 `src/HospiKeeper.Blazor` 时：

1. 引用 `HospiKeeper.Contracts` 与 `HospiKeeper.Client`；
2. 通过 `HospiKeeper.Api` 登录并携带 Bearer Token，不在 Web 端直连 EF Core / MySQL；
3. 复用现有权限、业务规则与单据编号逻辑；
4. 若需要统一视觉，可将 Ant Design Token 映射为 Blazor CSS 变量；Avalonia 控件不可在 Blazor 中复用；
5. 新增项目后更新 `HospiKeeper.slnx`、README 与本文件。
