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
| ASP.NET Core | 10（Web API + Controllers + OpenAPI 3.1 + Scalar） |
| EF Core | 10.0.12 |
| MySQL Provider | `MySql.EntityFrameworkCore` 10.0.12（官方 Provider，底层 MySql.Data） |
| MySQL / Redis | MySQL 8 · Redis 7（Docker Compose） |
| 缓存 | `Sylvie233.Caching`（Memory / Redis 可切换）· StackExchange.Redis 3.4 |
| 日志 | `Sylvie233.Logging`（Serilog 4 · 控制台 + 滚动文件） |
| 测试 | xUnit 2.9 + EF Core InMemory |
| 客户端 | `HospiKeeper.Client` 内手写 `HttpClient` 封装 |
| 游客 Web | Blazor Web App（Interactive Server SSR）· `HospiKeeper.Web` |

版本统一由根目录 `Directory.Packages.props` 管理；不要在子项目中硬编码版本。

## 3. 模块与依赖规则

```
HospiKeeper.Domain          实体 / 枚举 / 计费领域服务，无框架依赖
HospiKeeper.Contracts       请求 / 响应 DTO、DataAnnotations 校验、动作常量（API 契约）
HospiKeeper.Application     用例接口与实现、仓储抽象、缓存协调、实体→DTO 映射、会话
Sylvie233.Caching           ICacheService 端口 + Memory / Redis 实现（无业务依赖，可独立复用）
Sylvie233.Logging           Serilog 统一配置：控制台 / 滚动文件（无业务依赖，可独立复用）
Sylvie233.Database          MySQL 连接配置 / DbContext 注册 / 迁移执行（无业务依赖）
HospiKeeper.Infrastructure  EF Core DbContext / 配置 / 仓储 / 迁移 / 种子
HospiKeeper.Api             独立 Web API（组合根、控制器、鉴权、OpenAPI / Scalar）
HospiKeeper.Client          桌面端 HTTP 客户端
Sylvie233.UI                Avalonia 设计系统与自研控件（不依赖业务模块）
HospiKeeper.Desktop         Avalonia 桌面端（MVVM 页面与弹窗）
HospiKeeper.Web             Blazor Web App 游客预订门户（只调用 API，不直连数据库）
```

依赖方向必须保持：

- `Api → Application / Infrastructure / Contracts`
- `Infrastructure → Application / Contracts / Sylvie233.Database`
- `Application → Sylvie233.Caching`（业务层使用共享缓存端口）
- `Application → Contracts → Domain`
- `Desktop → Client / Contracts / Sylvie233.UI / Sylvie233.Logging`（只消费 DTO；Domain 仅用于枚举 / 计费结果）
- `Web → Client / Contracts / Sylvie233.Logging`（Blazor 服务端渲染，只通过公开 API 访问业务）
- `Api → Sylvie233.Logging`
- `Sylvie233.UI` 不得引用 `Domain` / `Application` / `Api` / `Caching` / `Logging`
- `Sylvie233.Caching` 不得引用任何 HospiKeeper 业务模块
- `Sylvie233.Logging` 不得引用任何 HospiKeeper 业务模块
- `Sylvie233.Database` 不得引用任何 HospiKeeper 业务模块（业务 DbContext 由业务模块自己定义）
- `Contracts` 不得依赖 EF Core / ASP.NET Core；不得返回 Domain 实体，只能返回 DTO

新增 Blazor 等客户端时：引用 `Contracts` + `Client`，只通过 API 访问业务，不直连数据库。

## 4. 组合根与契约约定

### API 组合根

- `Program.cs` 只保留：日志初始化 → 构建宿主 → 数据库初始化 → 请求管道 → 运行。
- 依赖注册放在 `src/HospiKeeper.Api/Configuration/ApiServiceCollectionExtensions.cs`：
  - 数据库 / 缓存配置绑定；
  - `AddApplication()`、`AddInfrastructure()`、`AddCaching()`；
  - MVC、JSON、模型校验、OpenAPI、CORS。
- 管道与端点放在 `Configuration/WebApplicationExtensions.cs`；新增中间件必须在该文件调整顺序。
- 健康检查等端点放在 `Endpoints/`；OpenAPI 相关放在 `OpenApi/`。
- 新增注册不要写回 `Program.cs`。

### Contracts

- 目录按业务特性划分（Auth / Rooms / FrontDesk / Reservations / Records / Reports / Common），
  命名空间统一为 `HospiKeeper.Contracts`。
- 请求类型以 `Request` / `Input` 结尾，响应类型以 `Dto` 结尾；不要返回 EF 实体。
- 记录类型（record）的校验特性必须标在构造参数上，例如
  `[param: Required(ErrorMessage = "...")] string Name`；
  标在 `[property:]` 上会被 MVC 忽略并抛异常。
- 校验特性只做格式 / 必填 / 范围等“入口约束”；跨字段、唯一性、状态流转仍在业务服务中用 `InputGuard` 校验。
- 实体 → DTO 映射统一放在 `Application/Mapping/ContractMappings.cs`；新增字段时同步 DTO 与映射。
- 修改 DTO 会同时影响 API、Client 与 Desktop，必须同步更新并跑通桌面端。

### Application

- 用例接口位于 `Application/Abstractions/Services`，实现位于 `Application/Services`；不要在实现文件里再声明接口。
- 依赖注册统一走 `AddApplication()`；新增服务在此注册。
- 业务异常：`DomainException` 映射 400，`ForbiddenException` 映射 403；不要再用消息字符串判断权限。
- 应用层返回 Contracts DTO；仓储返回领域实体，映射在应用层完成。

### 容器镜像

- Dockerfile：`src/HospiKeeper.Api/Dockerfile`、`src/HospiKeeper.Web/Dockerfile`；
- 运行基础镜像：本机 `mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0`，构建时不要拉取外网基础镜像；
- 镜像标签：`hospikeeper-api:local`、`hospikeeper-web:local`；
- Compose：`Scripts/docker-compose.yml` 的 `apps` profile，服务名 `api` / `web`，容器名 `hospikeeper-api` / `hospikeeper-web`；
- 容器内端口 `8080`，宿主机 `5080` / `5081`；Web 通过 `Api__BaseUrl=http://api:8080` 访问 API；
- 容器日志输出到 stdout（`SYLVIE233_LOG_FILE=false`），使用 `docker logs` / `manage.ps1 image:logs` 查看；
- 修改 Dockerfile / compose 后同步更新 README 与 `manage.ps1` 帮助；
- `.dockerignore` 排除 `bin/`、`obj/`、`logs/`、`tests/`、`Docs/`，保持构建上下文精简。

## 5. 常用命令

```bash
# 构建 / 测试
dotnet build HospiKeeper.slnx
dotnet test

# 启动 MySQL + Redis
docker compose -f Scripts/docker-compose.yml up -d --wait

# 启动 API（http://localhost:5080，Scalar /scalar/v1）
dotnet run --project src/HospiKeeper.Api

# 启动桌面端
dotnet run --project src/HospiKeeper.Desktop

# 启动游客预订 Web（先启动 API）
dotnet run --project src/HospiKeeper.Web
# http://localhost:5081

# 本地运维（根目录 manage.ps1）
pwsh ./manage.ps1 up
pwsh ./manage.ps1 status
pwsh ./manage.ps1 dev
pwsh ./manage.ps1 logs -Target api -Follow   # 优先读取 logs/api-*.log
pwsh ./manage.ps1 logs -Target redis -Follow
pwsh ./manage.ps1 cli -Target redis
pwsh ./manage.ps1 reset -Force
pwsh ./manage.ps1 migrate

# 容器镜像（API / Web）
pwsh ./manage.ps1 image:build          # 构建两个镜像；-Target api|web 可单独构建
pwsh ./manage.ps1 image:up             # 启动 MySQL / Redis + API / Web 容器
pwsh ./manage.ps1 image:status
pwsh ./manage.ps1 image:logs -Target web -Follow
pwsh ./manage.ps1 image:down           # 只移除 API / Web 容器
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

## 6. 持久化架构

- 共享部件：`src/Sylvie233.Database`
  - `DatabaseOptions` / `DatabaseEnvironmentOverrides`：连接配置与环境变量；
  - `AddSylvie233MySql<TContext>`：MySQL DbContext 注册；
  - `IDatabaseMigrator` / `MySqlDatabaseMigrator<TContext>`：迁移执行与日志。
- 入口：`HotelDbContext`（`src/HospiKeeper.Infrastructure/Persistence/HotelDbContext.cs`）
- 映射：`Persistence/Configurations/*Configuration.cs`，表名与字段使用数据库设计的 snake_case。
- 仓储：`Persistence/Repositories/*Repository.cs`，实现 `Application/Abstractions/Repositories`。
- 工作单元：`EfUnitOfWork` 聚合仓储，`SaveChangesAsync` 统一提交，保证一次业务动作的多表变更原子性。
- 业务服务只依赖 `IUnitOfWork`，不出现 `DbContext` / EF API。
- 单据序号：`DocumentNumberRepository` 使用 `doc_sequence` 表，`FindAsync` 会优先读取当前上下文已跟踪的行，避免同一工作单元内重复序号。
- 启动初始化：`HotelDataInitializer` 先调用共享 `IDatabaseMigrator`，再执行 `ReservationSchemaUpgrader`，最后执行 `DemoDataSeeder`。
- **游客预订 v1.1 例外**：`reservation` 的房型 / 日期段 / 渠道字段直接维护 `Docs/民宿管理系统-数据库设计-v1.sql`，
  由 `ReservationSchemaUpgrader` 在启动时幂等补列并回填旧数据；不要为这些字段生成 dotnet-ef 迁移。
- 测试使用 `Microsoft.EntityFrameworkCore.InMemory`，通过 `ServiceTestContext` 构造完整服务栈并调用 `DemoDataSeeder.SeedAsync`。

新增实体时需同步：

1. `Domain/Entities` 实体；
2. `HotelDbContext` DbSet；
3. `Persistence/Configurations` 映射；
4. 仓储接口与实现（如需要）；
5. EF 迁移；
6. 业务服务与测试。

共享模块边界：**迁移文件、DbContext、映射、仓储、种子始终留在 Infrastructure**；
`Sylvie233.Database` 只放与具体表结构无关的连接、注册、迁移执行逻辑。

## 7. 缓存约定

- 端口：`ICacheService`（`src/Sylvie233.Caching/ICacheService.cs`）。
- 实现：`src/Sylvie233.Caching`
  - `Memory`：`IMemoryCache` + key 注册表；
  - `Redis`：`StackExchange.Redis`，支持前缀批量清理。
- 切换：`appsettings.json` → `Cache:Provider`（`Redis` / `Memory`），或环境变量 `Cache__Provider`。
- 业务 key 只能通过 `src/HospiKeeper.Application/Caching/CacheKeys.cs` 定义；TTL 通过 `CacheDurations.cs` 统一维护。
- 写操作成功（`SaveChangesAsync` 之后）必须调用 `IHotelCacheInvalidator` 对应方法：
  - 房间 / 房型 → `InvalidateRoomsAsync`
  - 入住 / 退房 / 预定 / 追加消费 → `InvalidateFrontDeskAsync`
- 当前缓存范围：看板、房态汇总、在住、预定、房间、空闲房、楼层、房型、报表快照、区间营收。
- `GetOrCreateAsync` 内部有按 key 的进程内防击穿锁；缓存失败按 miss 处理，不阻断业务。
- 新增读多写少接口时：在 Application 服务中调用 `_cache.GetOrCreateAsync`，并在相关写操作中补失效逻辑；不要直接在 Controller 中写缓存。
- Redis 服务由 compose 管理：`hospikeeper-redis`，`localhost:6379`，开启 AOF；
  普通缓存 key 带 TTL，`volatile-lru` 只淘汰带过期时间的 key；系统配置 key 无 TTL。
- 修改 Redis 启动参数后，开发机需 `docker compose -f Scripts/docker-compose.yml up -d --force-recreate redis` 才能生效。
- `SetPersistentAsync` 用于写入无 TTL 的持久项（系统配置），返回是否成功；普通缓存继续使用 `SetAsync` + TTL。

## 8. 日志约定

- 模块：`src/Sylvie233.Logging`（Serilog 4）。
- API 启动顺序：`Sylvie233Log.Initialize(options)`（宿主构建前兜底）→ `builder.Host.UseSylvie233Logging(options)`（接管 ILogger）→ `finally { await Log.CloseAndFlushAsync(); }`。
- 桌面端：`DesktopLogging.Initialize()` 设置静态 `Log.Logger`，并在退出时 `CloseAndFlush`。
- 配置：`appsettings.json` → `Logging:Sylvie233`；环境变量 `SYLVIE233_LOG_DIRECTORY` / `SYLVIE233_LOG_LEVEL` / `SYLVIE233_LOG_CONSOLE` / `SYLVIE233_LOG_FILE` 可覆盖。
- 文件：`logs/<FileNamePrefix>-yyyyMMdd.log`，按天滚动，默认保留 30 个文件、单文件上限 20MB；`logs/` 已被 gitignore。
- 级别：默认 Information；`Microsoft` / `Microsoft.EntityFrameworkCore` 压制到 Warning；业务校验失败用 Warning。
- 用法：
  - 业务服务优先构造注入 `ILogger<T>`；
  - 桌面端 / 启动兜底使用 `Sylvie233Log.ForContext<T>()` / `Sylvie233Log.Logger`；
  - 不要记录密码、令牌、身份证号等敏感明文。
- 中间件顺序：`UseSerilogRequestLogging()` 必须位于 `ExceptionHandlingMiddleware` 外层，保证记录最终状态码。
- 请求日志模板：`HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0} ms`。
- 新增日志不要使用 `Console.WriteLine` / `Debug.WriteLine`。

## 9. 系统配置约定

- DTO：`HospiKeeper.Contracts.SiteSettings`，包含品牌、首页、联系、入住支付、页脚、桌面端文案与特色 / 须知列表；默认值写在该类中。
- 服务：`ISiteSettingsService` / `SiteSettingsService`
  - `GetPublicAsync`：公开读取；Redis 未配置时写入默认值；
  - `GetForAdminAsync` / `UpdateAsync`：要求管理员；
  - 存储 key：`CacheKeys.SiteSettings`（`config:site`），使用 `SetPersistentAsync` 无 TTL 写入；
  - 保存失败（Redis 不可用）抛 `DomainException`，不要静默成功。
- API：
  - `GET /api/public/settings` 公开；
  - `GET/PUT /api/settings` 管理员（控制器 `SettingsController`）。
- 桌面端：
  - 页面 `SystemSettingsPageViewModel` + `SystemSettingsView`，位于「基础信息（管理员）」；
  - 保存后发送 `SiteSettingsChangedMessage`，主窗口即时刷新标题 / 版本 / 状态栏；
  - 「关于本系统」使用 `SiteSettings` 的桌面端文案。
- 游客 Web：
  - `SiteSettingsState` 在每个 Blazor 电路中缓存一次配置；
  - `NavMenu`、`PageFooter`、`Home`、`Rooms`、`BookingDialog`、`BookingSuccess` 均从配置读取；
  - 新增静态文案时优先扩展 `SiteSettings` 与设置页，不要在 Razor 中硬编码。
- 变更字段时同步：Contracts → 设置页 ViewModel/XAML → Web 组件 → README 本文件。

## 10. 业务规则

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

## 11. API 约定

- 路由统一 `api/<resource>`，返回 `Contracts` DTO；JSON 枚举以字符串输出。
- 登录：`POST /api/auth/login` 返回 Bearer Token；除 `/health`、`/scalar`、`/openapi`、登录接口与 `/api/public` 游客接口外均需 `Authorization: Bearer`。
- Token 为内存会话（12 小时）；服务端每次请求重新加载用户，角色 / 停用状态即时生效。
- 业务异常 `DomainException` 由 `ExceptionHandlingMiddleware` 转为 400；`ForbiddenException` 转为 403；未处理异常返回 500。
- 请求 DTO 的 DataAnnotations 校验失败由 `ApiBehaviorOptions.InvalidModelStateResponseFactory` 转为 400 `ApiErrorResponse`。
- 控制器保持轻薄：参数绑定 → 调用 Application 服务 → 返回结果；不要在控制器写业务逻辑。
- 桌面端接入点：`HospiKeeper.Client/IHospiKeeperApi` / `HospiKeeperApiClient`。

### 接口文档

- 生成器：`Microsoft.AspNetCore.OpenApi`（内置，OpenAPI 3.1）；UI：`Scalar.AspNetCore`。
- 端点：`/openapi/v1.json`（文档）与 `/scalar/v1`（界面）。
- 只在 Development 映射；不要在生产环境开放。`manage.ps1 up` 已为本地 API 设置 `ASPNETCORE_ENVIRONMENT=Development`。
- 文档补充集中在 `src/HospiKeeper.Api/OpenApi/`：
  - `HospiKeeperOpenApiDocumentTransformer`：标题、描述、`Bearer` 安全方案与全局要求；
  - `HospiKeeperOpenApiOperationTransformer`：统一错误响应，并为登录等公开接口取消鉴权要求。
- 新增控制器 / 接口时必须：
  - 控制器加 `[Tags("中文分组")]`；
  - 类型与接口补 `<summary>` 中文 XML 注释（`GenerateDocumentationFile` 已开启，缺失注释不报错但不进文档）；
  - 公开接口在 `PublicPaths` 中登记并取消安全要求。
- 错误响应统一引用 `ApiErrorResponse`；不要为每个接口重复定义错误 schema。
- 改动鉴权、公开路径或安全方案时同步更新 README 的「接口文档」章节。

## 12. UI 约定

- 设计系统在 `Sylvie233.UI`：
  - 控件：`AntButton / AntTag / AntCard / AntStatCard / AntPageHeader / AntFormItem / AntSectionTitle / AntModal / AntRoomCard / AntBarChart / AntEmpty / AntToast / AntTable / AntIcons`
  - 主题：`Theme/AntResources.axaml`、`Theme/AntControls.axaml`、`Theme/AntStyles.axaml`
- 页面统一使用 `AntPageHeader` + `AntCard` + `AntTable` 的组合，风格参考 Ant Design（主色 `#1677FF`，28-32px 紧凑控件）。
- Avalonia 12 已废弃 `DataGrid`；表格统一使用自研 `AntTable`。
- `AntTable` 默认 `PageSize=10` 并自动分页；不足一页时自动隐藏分页条。
  - 固定高度表格（如报表区间表）可通过 `PageSize` 调整每页条数；`PageSize=0` 可关闭分页。
  - 不要在 ViewModel 中重复实现客户端分页；将来接服务端分页时再引入 `Total` / `PageIndex` 参数。
  - 文本列会自动附加 Tooltip，列宽不足时可悬停查看完整内容。
- Desktop 使用 MVVM：View 只绑定 ViewModel，业务调用全部经 `IHospiKeeperApi`。
- `Desktop/GlobalUsings.cs` 用别名把 `RoomDto` 等契约类型映射回旧名称；不要新增 `using HospiKeeper.Domain.Entities;`。
- 弹窗统一由 `DialogService` 管理；成功 / 失败提示统一走 `ToastService`。
- 交互动效约定：
  - 页面切换时 `PageHost` 淡入（180ms）；
  - 弹窗遮罩与 `AntModal` 打开淡入（160ms），关闭由 `DialogService` 立即收起；
  - Toast 淡入 / 淡出，队列最多 4 条，错误停留时间更长；
  - 按钮、表格行使用 Avalonia `Transitions`，不要在页面里手写动画代码。
- 统计报表布局：
  - 顶部 4 列指标卡（8 张）；
  - 近 7 日营收与房型经营分析左右并排（1.45 : 1）；
  - 下方时间段营收明细：日期查询 + 今日 / 近 3 日 / 近 7 日 / 近 30 日快捷区间 + 汇总 + 分页表格；
  - 区间占比使用 `ProgressBar`，金额 / 占比列右对齐。
- 页面不展示演示账号、测试口令等演示信息。
- 新控件需同时提供 C# 属性与 `ControlTheme`，并在 `AntControls.axaml` / `AntStyles.axaml` 中注册。

## 13. 代码风格

- 命名空间与类型命名使用英文；用户可见文案使用中文。
- 使用 `Directory.Build.props` 中的 `Nullable`、`ImplicitUsings`；不要关闭可空检查。
- 业务服务方法使用 `Async` 后缀并接收 `CancellationToken`（默认参数 `default`）。
- 输入校验统一使用 `InputGuard`；业务错误抛 `DomainException`。
- 时间统一通过 `IClock`（测试可替换）；除种子 / 展示文案外避免直接使用 `DateTime.Now`。
- 不要在 Application / Domain 引入 EF Core、Avalonia 或 ASP.NET Core 依赖。
- 删除不再使用的代码、图标、样式与包；不要保留“以后可能用到”的空实现。

## 14. 测试

- 位置：`tests/HospiKeeper.Tests`
- `ServiceTestContext` 使用 EF Core InMemory + `DemoDataSeeder` 构造完整业务栈，并默认登录 `admin`。
- 新增业务规则必须补充测试；当前覆盖 37 个用例。
- 不要用测试去连接真实 MySQL；需要集成验证时直接启动 API 进行手工 / curl 冒烟。

## 15. Git 与协作

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

## 16. Blazor Web 模块

`src/HospiKeeper.Web` 是游客预订门户，已实现首页 / 客房预订 / 我的预订三页。

约定：

- 技术形态：**Blazor Web App + Interactive Server**（服务端渲染，非 WebAssembly）；
- 只通过 `HospiKeeper.Client.IGuestBookingApi` 调用 `HospiKeeper.Api`，不引用 EF Core / MySQL；
- 页面路由：`/`、`/rooms`、`/mine`；日期条件通过 query string 传递；
- 预订模型：只锁房型 + 日期段，不锁具体房间；到店由桌面端「转入住」选择该房型空闲房并回填 `room_id`；
- 公开接口统一 `/api/public/*`，在 `ApiAuthMiddleware` 中免鉴权；OpenAPI 中通过 `PublicPrefixes` 取消 Bearer 要求；
- 库存计算集中在 `GuestBookingService`，UI 不重复实现；提交时通过 `IUnitOfWork.ExecuteInTransactionAsync` 做事务内二次校验；
- 游客端不改变任何 `room.status`；
- 品牌 / Hero / 联系方式 / 页脚 / 特色 / 须知等静态文案全部来自 `GET /api/public/settings`，不要硬编码；
- Web 日志：`logs/web-*.log`，配置项与 API 相同（`Logging:Sylvie233`）。

后续新增 Web 页面时：

1. 引用 `Contracts` 与 `Client`，不要新增数据库访问；
2. 新增公开 API 时同步更新 `PublicBookingController`、`ApiAuthMiddleware.IsPublic` 与 OpenAPI 公开路径；
3. 新增页面后更新本文件与 README。
