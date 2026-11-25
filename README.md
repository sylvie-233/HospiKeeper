# HospiKeeper · 云栖民宿管理系统

面向中小型民宿 / 小旅馆的桌面经营系统，覆盖「登录与权限、房态看板、入住登记、预定、在住、追加消费、退房结算、记录查询、统计报表、房间 / 房型 / 账号管理」的完整闭环。

项目采用 **Avalonia 桌面客户端 → ASP.NET Core API → EF Core / MySQL** 的分层架构，并内置一套参考 Ant Design 规范实现的可复用控件库。当前代码主要用于验证 .NET 桌面 + Web API + EF Core 的主流技术栈组合，同时为后续新增 Blazor 模块预留了清晰的模块边界。

## 技术栈

| 层 | 技术 |
|----|------|
| 运行时 | .NET 10（LTS） |
| 桌面客户端 | Avalonia 12.2 · CommunityToolkit.Mvvm 8.4 |
| 游客 Web | Blazor Web App（Interactive Server SSR）· HttpClient 调用 Web API |
| UI 设计系统 | 自研 Ant 风格控件库 `Sylvie233.UI`（TemplatedControl / ControlTheme） |
| API | ASP.NET Core 10 · Controllers · OpenAPI 3.1 · Scalar |
| 持久化 | EF Core 10.0.12 · MySql.EntityFrameworkCore 10.0.12（官方 MySQL Provider） |
| 数据库组件 | 自研 `Sylvie233.Database`：连接配置 + DbContext 注册 + 迁移执行（无业务依赖） |
| 缓存 | 自研 `Sylvie233.Caching`：Memory / Redis 可切换 · StackExchange.Redis 3.4 |
| 日志 | 自研 `Sylvie233.Logging`：Serilog 4 · 控制台 + 按天滚动文件 · 结构化字段 |
| 基础设施 | MySQL 8 · Redis 7（`Scripts/docker-compose.yml`，3306 / 6379） |
| 测试 | xUnit · EF Core InMemory（37 个用例） |

## 解决方案结构

```
HospiKeeper.slnx
├─ src/
│  ├─ HospiKeeper.Domain/          # 实体、枚举、计费领域服务（无框架依赖）
│  ├─ HospiKeeper.Contracts/       # 请求 / 响应 DTO、校验规则、动作常量（API 契约）
│  ├─ HospiKeeper.Application/     # 用例接口与实现 + 仓储抽象 + 缓存协调 + 实体→DTO 映射
│  ├─ Sylvie233.Caching/           # ICacheService 端口 + Memory / Redis 实现（无业务依赖）
│  ├─ Sylvie233.Logging/           # Serilog 统一配置：控制台 / 滚动文件（无业务依赖）
│  ├─ Sylvie233.Database/          # 连接配置、MySQL DbContext 注册、迁移执行（无业务依赖）
│  ├─ HospiKeeper.Infrastructure/  # EF Core DbContext、映射、仓储、迁移、演示数据
│  ├─ HospiKeeper.Api/             # 独立 Web API（组合根、Token 鉴权、OpenAPI / Scalar）
│  ├─ HospiKeeper.Client/          # 桌面端 HTTP 客户端（IHospiKeeperApi）
│  ├─ Sylvie233.UI/              # 可复用 UI 设计系统与自研控件
│  ├─ HospiKeeper.Desktop/         # Avalonia 桌面应用（MVVM 页面 + 弹窗）
│  └─ HospiKeeper.Web/             # 游客在线预订 Web（Blazor Server SSR，仅调用 API）
├─ tests/
│  └─ HospiKeeper.Tests/           # 领域与业务流程测试（EF Core InMemory）
├─ Docs/                           # 原型、功能设计、MySQL 数据库设计参考
├─ Scripts/                        # docker compose 与开发启动脚本
├─ logs/                           # 运行日志（api-*.log / desktop-*.log，已 gitignore）
├─ CLAUDE.md                       # 项目开发规范与 Claude Code 指引
└─ dotnet-tools.json               # 本地 dotnet-ef 工具清单
```

依赖方向：`Desktop → Client → Contracts → Domain`，并引用共享控件库 `Sylvie233.UI` 与 `Sylvie233.Logging`；`Api → Application → Contracts → Domain`；`Infrastructure → Application/Contracts`，并引用 `Sylvie233.Database`；`Application → Sylvie233.Caching`（使用共享缓存端口）。桌面端只消费 `Contracts` 中的 DTO，不直接使用 EF 实体。`HospiKeeper.Web → Client / Contracts`（只通过公开 API 访问业务，不直连数据库）。`Sylvie233.UI`、`Sylvie233.Caching`、`Sylvie233.Logging`、`Sylvie233.Database` 均不依赖业务模块，Blazor 等新客户端可直接复用 `Client` / `Contracts` / `Sylvie233.UI` 设计规范 / `Sylvie233.Logging` / `Api`。

## 分层与契约

### Contracts

`HospiKeeper.Contracts` 是前后端共享的 API 契约层，按业务特性分目录：

```text
Contracts/
├─ Auth/          登录、用户、改密、账号
├─ Rooms/         房间 / 房型输入与 DTO
├─ FrontDesk/     入住、退房、看板、在住 DTO
├─ Reservations/  预定输入与 DTO
├─ Records/       记录查询与操作日志 DTO
├─ Reports/       报表快照与区间营收
└─ Common/        错误响应、健康检查、动作常量、校验正则
```

约定：

- 请求类型以 `Request` / `Input` 结尾，响应类型以 `Dto` 结尾；不再直接返回 EF 实体；
- 请求 DTO 使用 `System.ComponentModel.DataAnnotations` 标注必填、长度、范围与正则，
  `[ApiController]` 会在进入业务层前完成校验，错误统一为 `ApiErrorResponse`；
- 命名空间保持 `HospiKeeper.Contracts`（目录只用于组织文件），避免调用方大量改 using；
- 领域实体 → DTO 的映射集中在 `HospiKeeper.Application/Mapping/ContractMappings.cs`；
- Contracts 只引用 Domain 中的枚举等基础类型，不依赖 EF Core / ASP.NET Core。

### Application

- 用例接口位于 `Application/Abstractions/Services`，实现位于 `Application/Services`；
- 依赖注册统一由 `ApplicationServiceCollectionExtensions.AddApplication()` 完成，
  API 组合根不再逐个 `AddScoped`；
- 业务异常分两类：`DomainException` → 400，`ForbiddenException`（如非管理员）→ 403；
- 应用层负责把实体转换为 Contracts DTO，控制器只做参数绑定与返回。

### API 组合根

`Program.cs` 只保留启动骨架，其余拆分为：

| 文件 | 职责 |
|------|------|
| `Configuration/ApiServiceCollectionExtensions.cs` | 数据库 / 缓存 / 业务服务 / MVC / 校验 / OpenAPI / CORS 注册 |
| `Configuration/WebApplicationExtensions.cs` | 数据库初始化、中间件顺序、端点映射 |
| `Endpoints/HealthEndpoints.cs` | `/health` |
| `OpenApi/*` | 文档信息、Bearer 方案、错误响应 |

新增依赖时优先扩展 `AddHospiKeeperApi`，不要继续堆到 `Program.cs`。

## 自研 UI 控件库

`Sylvie233.UI` 不依赖任何第三方 UI 组件，包含：

- `AntButton`：Primary / Default / Danger / Text / Link 变体，Small / Middle / Large 尺寸
- `AntTag`：默认 / 成功 / 处理中 / 警告 / 失败 / 紫色 / 青色
- `AntCard`、`AntStatCard`、`AntPageHeader`、`AntSectionTitle`
- `AntFormItem`：标签、必填星号、错误信息
- `AntModal`：主窗口内模态弹窗容器
- `AntRoomCard`：六种房态渐变卡片与交互态
- `AntTable` / `AntTableColumn`：替代废弃 DataGrid 的轻量数据表格，内置分页（默认 10 条/页，不足一页自动隐藏）
- `AntBarChart`：自绘柱状图，无图表库依赖
- `AntEmpty`、`AntToast`（淡入淡出、最多 4 条）、`AntIcons`
- `Theme/`：Ant Design 色彩 Token、控件主题、输入框 / 下拉 / 菜单 / 票据等统一样式
- 交互动效：页面切换淡入、弹窗 / 遮罩淡入、Toast 淡入淡出、按钮与表格行 hover 过渡

## 快速开始

### 环境要求

- .NET 10 SDK
- Docker Desktop（MySQL 8）

### 1. 启动 MySQL / Redis

```bash
docker compose -f Scripts/docker-compose.yml up -d --wait
```

连接信息（与 compose 一致）：

```text
MySQL   Host=127.0.0.1 Port=3306 Database=hospikeeper User=hospikeeper Password=123456
Redis   localhost:6379（无密码，缓存 + 系统配置持久 key）
```

> 数据库表结构与演示数据由 API 启动时的 EF Core Migrations 自动创建；
> `Docs/民宿管理系统-数据库设计-v1.sql` 保留为设计参考，不再由 compose 自动执行。
> Redis 以 `--appendonly yes --maxmemory 256mb --maxmemory-policy volatile-lru` 运行：
> 普通缓存 key 带 TTL，可被淘汰；系统配置 `config:site` 无 TTL，并通过 AOF 持久化。
> 从旧版本升级时需重建 Redis 容器以应用 AOF：`docker compose -f Scripts/docker-compose.yml up -d --force-recreate redis`。

### 2. 启动 API

```bash
dotnet run --project src/HospiKeeper.Api
```

- API：`http://localhost:5080`
- Scalar 文档：`http://localhost:5080/scalar/v1`
- OpenAPI JSON：`http://localhost:5080/openapi/v1.json`
- 首次启动自动执行 `Database.MigrateAsync()`，并在空库中生成演示数据。

### 3. 启动桌面客户端

```bash
dotnet run --project src/HospiKeeper.Desktop
```

### 4. 启动游客预订 Web

先确保 API 已启动，然后：

```bash
dotnet run --project src/HospiKeeper.Web
```

- 游客门户：`http://localhost:5081`
- 页面：首页 `/`、客房预订 `/rooms`、我的预订 `/mine`
- Web 使用 Blazor Web App（Interactive Server 服务端渲染），只通过 `HttpClient` 调用 API，不连接数据库。

或者使用根目录运维脚本一键启动 MySQL、API 后打开桌面端：

```powershell
pwsh ./manage.ps1 dev
```

常用运维命令：

```powershell
pwsh ./manage.ps1 up              # 启动 MySQL + Redis + API 并等待就绪
pwsh ./manage.ps1 up -Target redis # 仅启动 Redis
pwsh ./manage.ps1 status          # 查看 MySQL / Redis / API 状态与地址
pwsh ./manage.ps1 logs -Target api -Follow
pwsh ./manage.ps1 logs -Target redis -Follow
pwsh ./manage.ps1 cli -Target redis
pwsh ./manage.ps1 build           # 构建解决方案
pwsh ./manage.ps1 test            # 运行测试
pwsh ./manage.ps1 migrate         # 应用 EF Core 迁移
pwsh ./manage.ps1 reset -Force    # 清空数据卷后重建环境
pwsh ./manage.ps1 down            # 停止本地 API 与全部容器
pwsh ./manage.ps1 image:build     # 构建 API / Web 容器镜像
pwsh ./manage.ps1 image:up        # 启动 API / Web 容器
pwsh ./manage.ps1 image:down      # 移除 API / Web 容器
pwsh ./manage.ps1 image:logs -Target web -Follow
```

## 接口文档（OpenAPI / Scalar）

API 使用 ASP.NET Core 10 内置的 `Microsoft.AspNetCore.OpenApi` 生成 **OpenAPI 3.1** 文档，界面使用 **Scalar**：

| 地址 | 说明 |
|------|------|
| `http://localhost:5080/scalar/v1` | Scalar 交互式文档 |
| `http://localhost:5080/openapi/v1.json` | OpenAPI JSON（可导入 Postman / Apifox / 代码生成器） |

特性：

- **Bearer 鉴权**：文档内置 `Bearer` 安全方案，Scalar 右侧可直接填入登录返回的 Token；
  全局接口默认要求鉴权，仅 `POST /api/auth/login` 与 `/health` 例外；
- **XML 注释**：控制器与接口的中文 `<summary>` 会进入文档，不需要手写 schema 描述；
- **Tag 分组**：认证、房态看板、入住、退房、在住、预定、记录查询、统计报表、房间 / 房型 / 账号管理；
- **统一错误响应**：每个接口补充 400 / 401 / 403 / 500 说明，错误体引用 `ApiErrorResponse`；
- **仅开发环境开放**：`Program.cs` 中通过 `app.Environment.IsDevelopment()` 控制，
  生产环境不会暴露 `/scalar` 与 `/openapi`；`manage.ps1 up` 启动本地 API 时会自动设置 `ASPNETCORE_ENVIRONMENT=Development`。

新增接口时：

1. 控制器加 `[Tags("...")]` 分组；
2. 接口与控制器补 `<summary>` 中文注释；
3. 公开接口需要在 `HospiKeeperOpenApiOperationTransformer` 中取消 Bearer 要求。

## 游客预订 Web（Blazor）

`HospiKeeper.Web` 是面向游客的在线预订门户，与桌面端共用同一套 API 与数据库：

- 技术形态：**Blazor Web App + Interactive Server 渲染**，首屏由服务端输出，交互在 Blazor Server 电路内完成；
- 数据访问：只调用 `HospiKeeper.Api` 的公开接口，不引用 EF Core / MySQL；
- 预订模型：**只订房型 + 日期段，不锁具体房间**，到店由前台在桌面端分配房间并转入住；
- 库存口径：`可售房数 − 日期段重叠的待到店预订数`，提交时在服务端事务内二次校验；
- 页面：首页营销与查询、客房预订、我的预订（手机号查询 / 在线取消）。

公开 API：

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/public/summary` | 首页统计 |
| GET | `/api/public/room-types?checkIn=&checkOut=` | 房型实时可订量 |
| POST | `/api/public/reservations` | 提交预订 |
| GET | `/api/public/reservations?phone=` | 手机号查询 |
| POST | `/api/public/reservations/{resNo}/cancel` | 游客取消 |
| GET | `/api/public/settings` | 游客 Web 静态配置 |

配置：`src/HospiKeeper.Web/appsettings.json` → `Api:BaseUrl`（默认 `http://localhost:5080`）。

> 数据库 v1.1：`reservation` 增加 `type_id / check_in_date / check_out_date / channel / guest_id_card / unit_price`，
> `room_id` 与 `room_no` 允许为空。按项目约定，本次结构变更**直接维护 `Docs/民宿管理系统-数据库设计-v1.sql`**，
> 不生成 EF 迁移；API 启动时由 `ReservationSchemaUpgrader` 做幂等结构检查与旧数据回填。

## 容器化运行（API / Web）

API 与游客 Web 都提供多阶段 Dockerfile，使用本机已有的 .NET 10 基础镜像：

- `mcr.microsoft.com/dotnet/sdk:10.0`（构建阶段）
- `mcr.microsoft.com/dotnet/aspnet:10.0`（运行阶段）
- 镜像标签：`hospikeeper-api:local`、`hospikeeper-web:local`
- 容器端口统一 `8080`，宿主机默认映射 `5080`（API）/ `5081`（Web）
- Compose 使用 `apps` profile，不干扰默认的 MySQL / Redis 启动

```powershell
# 构建两个镜像（-Target api / web 可只构建其中一个；-Force 无缓存重建）
pwsh ./manage.ps1 image:build

# 启动 MySQL / Redis + API / Web 容器
pwsh ./manage.ps1 image:up

# 查看状态 / 日志
pwsh ./manage.ps1 image:status
pwsh ./manage.ps1 image:logs -Target web -Follow

# 只移除 API / Web 容器，保留 MySQL / Redis
pwsh ./manage.ps1 image:down
```

等价 Compose 命令：

```bash
docker compose -f Scripts/docker-compose.yml --profile apps up -d --wait api web
```

> 修改 API / Web 源码后：先执行 `image:build`，再执行 `image:up`。
> `image:up` 会强制重建运行中的 API / Web 容器，确保使用最新镜像。

容器内环境：

| 变量 | 值 |
|------|----|
| `HOSPITKEEPER_DB_HOST` | `mysql` |
| `Cache__RedisConnectionString` | `redis:6379` |
| `Api__BaseUrl`（Web） | `http://api:8080` |
| `SYLVIE233_LOG_FILE` | `false`（日志输出到容器 stdout） |

镜像构建上下文由根目录 `.dockerignore` 控制，排除 `bin/`、`obj/`、`logs/`、`tests/` 等。

## 初始账号

| 账号 | 密码 | 角色 | 状态 |
|------|------|------|------|
| `admin` | `123456` | 管理员 | 启用 |
| `front01` | `123456` | 前台 | 启用 |
| `front02` | `123456` | 前台 | 停用（用于验证停用拦截） |

> 页面上不展示演示账号信息；以上仅用于本地启动。

## 配置

### API 数据库连接

`src/HospiKeeper.Api/appsettings.json`：

```json
{
  "Database": {
    "Host": "127.0.0.1",
    "Port": 3306,
    "Name": "hospikeeper",
    "User": "hospikeeper",
    "Password": "123456"
  }
}
```

支持环境变量覆盖，查找顺序为：

1. 业务前缀（当前 API 使用 `HOSPITKEEPER_`）
2. `SYLVIE233_`
3. Docker MySQL 约定（`MYSQL_`）

| 配置 | 可用变量（任一即可） |
|------|----------------------|
| Host | `HOSPITKEEPER_DB_HOST` / `SYLVIE233_DB_HOST` / `MYSQL_HOST` |
| Port | `HOSPITKEEPER_DB_PORT` / `SYLVIE233_DB_PORT` / `MYSQL_PORT` |
| Name | `HOSPITKEEPER_DB_NAME` / `SYLVIE233_DB_NAME` / `MYSQL_DATABASE` |
| User | `HOSPITKEEPER_DB_USER` / `SYLVIE233_DB_USER` / `MYSQL_USER` |
| Password | `HOSPITKEEPER_DB_PASSWORD` / `SYLVIE233_DB_PASSWORD` / `MYSQL_PASSWORD` |

### 缓存配置

`src/HospiKeeper.Api/appsettings.json`：

```json
{
  "Cache": {
    "Provider": "Redis",
    "InstanceName": "hospikeeper:",
    "DefaultExpirationSeconds": 60,
    "RedisConnectionString": "localhost:6379",
    "RedisDatabase": 0
  }
}
```

切换为进程内缓存只需把 `Provider` 改为 `Memory`（或设置环境变量 `Cache__Provider=Memory`），无需改动业务代码；
Redis 连接可通过 `Cache__RedisConnectionString` / `REDIS_PORT`（compose 端口）调整。

### 桌面端 API 地址

`src/HospiKeeper.Desktop/appsettings.json`：

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5080"
  }
}
```

可通过 `HOSPITKEEPER_API_URL` 覆盖。

### 游客 Web API 地址

`src/HospiKeeper.Web/appsettings.json`：

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5080"
  }
}
```

## EF Core 迁移

本地工具清单已包含 `dotnet-ef 10.0.12`：

```bash
dotnet tool restore
```

新增迁移：

```bash
dotnet ef migrations add <MigrationName> \
  --project src/HospiKeeper.Infrastructure \
  --startup-project src/HospiKeeper.Api \
  --output-dir Persistence/Migrations
```

手动更新数据库：

```bash
dotnet ef database update \
  --project src/HospiKeeper.Infrastructure \
  --startup-project src/HospiKeeper.Api
```

重置本地数据库（会删除全部数据，重启 API 后自动迁移并重新生成演示数据）：

```bash
docker exec hospikeeper-mysql mysql -uroot -p123456 -e "
DROP DATABASE IF EXISTS hospikeeper;
CREATE DATABASE hospikeeper CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
GRANT ALL PRIVILEGES ON hospikeeper.* TO 'hospikeeper'@'%';
FLUSH PRIVILEGES;"
```

## 缓存模块

`Sylvie233.Caching` 对上层暴露统一的 `ICacheService`，业务服务只依赖接口：

- **Memory**：`IMemoryCache` + key 注册表，适合单进程调试；
- **Redis**：`StackExchange.Redis` 直连，支持多实例共享、前缀清理、TTL；
- 统一的 `GetAsync / SetAsync / GetOrCreateAsync / RemoveAsync / RemoveByPrefixAsync`；
- `GetOrCreateAsync` 内置进程内防击穿锁；Redis 异常时降级为回源，不阻断业务；
- `SetPersistentAsync` 写入无 TTL 的持久项（系统配置使用），返回是否写入成功；
- 业务 key 统一由 `CacheKeys` 定义，TTL 统一由 `CacheDurations` 定义；
- `IHotelCacheInvalidator` 集中处理房间 / 前台 / 报表相关缓存失效，写操作成功后统一调用。

当前缓存范围：

| 数据 | 缓存 key | TTL | 失效触发 |
|------|----------|-----|----------|
| 房态看板 | `board` | 15s | 房间 / 入住 / 退房 / 预定变更 |
| 房态汇总 | `counts` | 15s | 同上 |
| 在住列表 | `stays:all` | 15s | 入住 / 退房 / 追加消费 |
| 预定列表 | `reservations:*` | 30s | 预定创建 / 取消 / 转入住 |
| 房间列表 / 空闲房 / 楼层 | `rooms:*` | 60s | 房间 / 房型 / 房态变更 |
| 房型列表 | `roomtypes:all` | 60s | 房型变更 |
| 报表快照 / 区间营收 | `reports:*` | 60s | 房间或前台业务变更 |

## 日志模块

`Sylvie233.Logging` 基于 **Serilog 4**，对 API 与桌面端提供统一的日志能力，不依赖任何业务模块：

- 输出：控制台（可选）+ 按天滚动文件（`logs/<prefix>-yyyyMMdd.log`），文件写入使用异步 sink；
- 结构化：默认附带 `Application`、`MachineName`、`EnvironmentName`、`ThreadId`，配合 `SourceContext` 定位模块；
- 滚动策略：按天 + 单文件上限（默认 20MB），保留 30 个历史文件；
- 启动兜底：宿主构建前初始化静态 `Log.Logger`，数据库等启动失败也能落盘；
- 宿主集成：`builder.Host.UseSylvie233Logging(options)` 接管 `ILogger`，并启用 HTTP 请求日志；
- 桌面端：启动、未处理异常、未观察任务异常与页面操作失败统一写入 `logs/desktop-*.log`；
- 框架日志降噪：`Microsoft` / EF Core 默认压制到 Warning，业务日志保持 Information。

配置位置（API 与桌面端各自的 `appsettings.json`）：

```json
{
  "Logging": {
    "Sylvie233": {
      "ApplicationName": "HospiKeeper.Api",
      "FileNamePrefix": "api",
      "EnableConsole": true,
      "EnableFile": true,
      "MinimumLevel": "Information",
      "RetainedFileCountLimit": 30
    }
  }
}
```

环境变量覆盖（无需改配置）：

| 变量 | 作用 |
|------|------|
| `SYLVIE233_LOG_DIRECTORY` | 日志目录（默认自动定位仓库根目录下的 `logs/`） |
| `SYLVIE233_LOG_LEVEL` | 最低级别（Debug / Information / Warning / Error ...） |
| `SYLVIE233_LOG_CONSOLE` | 是否输出控制台（true / false） |
| `SYLVIE233_LOG_FILE` | 是否写文件（true / false） |

查看日志：

```powershell
pwsh ./manage.ps1 logs -Target api -Follow   # 优先读取 logs/api-*.log
Get-Content logs/desktop-*.log -Tail 100
```

## 数据库模块

`Sylvie233.Database` 只承载“连接 + 注册 + 迁移执行”，不感知任何业务表：

- `DatabaseOptions`：Host / Port / Name / User / Password、连接串构造、`SafeDisplay`；
- `DatabaseEnvironmentOverrides`：按“业务前缀 → `SYLVIE233_` → `MYSQL_`”解析环境变量；
- `AddSylvie233MySql<TContext>(options)`：以 MySQL 提供程序注册任意 DbContext；
- `IDatabaseMigrator` / `MySqlDatabaseMigrator<TContext>`：记录待应用迁移并执行 `MigrateAsync`。

业务侧仍由 `HospiKeeper.Infrastructure` 负责：`HotelDbContext`、表映射、仓储、`Migrations`、演示数据种子，以及组合各部分的 `AddInfrastructure`。启动流程为：

```text
HotelDataInitializer
  ├─ IDatabaseMigrator.MigrateAsync()   # 共享模块
  └─ DemoDataSeeder.SeedAsync()          # 业务种子
```

## 系统配置

桌面端管理员可在「基础信息 → 系统配置」中动态调整游客 Web 与桌面端的静态信息：

- 品牌：品牌名称、英文名、Logo 标识字、位置标语；
- 首页：Hero 标题前缀 / 高亮词 / 后缀、副标题、评分、步行时间；
- 联系与入住：前台热线、地址、邮箱、入住 / 退房时间、到店截止、支付与押金说明；
- 页脚与桌面端：页脚介绍、页脚文案、桌面端系统名称 / 版本 / 状态栏、关于描述；
- 内容卡片：特色介绍、入住须知（支持增删行）。

保存后：

- 游客 Web 刷新即可看到新内容（导航、首页、页脚、预订提示、成功页）；
- 桌面端标题栏、状态栏与「关于本系统」即时刷新；
- 配置存储于 Redis 的 `config:site` key（无 TTL），Redis 通过 AOF 持久化；
  普通缓存 key 仍带 TTL，`volatile-lru` 只淘汰带过期时间的缓存项。

API：

| 方法 | 路径 | 权限 |
|------|------|------|
| GET | `/api/public/settings` | 公开，游客 Web / 桌面端展示 |
| GET | `/api/settings` | 管理员 |
| PUT | `/api/settings` | 管理员 |

## 权限约定

- API 使用 Bearer Token（内存会话，12 小时有效）；每个请求由服务端重新加载用户角色与状态。
- 前台：可办理入住 / 退房 / 追加消费 / 打扫；记录与日志只能查看本人经办。
- 管理员：额外拥有房间、房型、操作员账号管理权限，并可查看全部记录。
- 服务端同样做权限校验，避免仅依赖 UI 隐藏。

## 业务要点

- 计费：按天（不足一晚按一晚）或加钟（小时数向上取整，房费 = ceil(小时 × 单价 ÷ 24)）。
- 结算差额：`应收 − 押金`；正数应补收、负数应退款。
- 入住 `I+yyyyMMdd+序号`、结算 `S+…`、预定 `R+…`，序号由 `doc_sequence` 表维护。
- 退房后房间自动进入「打扫中」；保洁完成后恢复「空闲」。
- 房型改名会同步所有房间的冗余房型名；改价写入「改价」日志。

## 测试

```bash
dotnet test
```

当前覆盖：计费口径、登录 / 停用 / 越权、账号唯一性、改密、入住校验、预定 / 取消 / 转入住、追加消费、退房结算、房间历史保护、前台数据隔离、报表指标与区间统计、缓存读写 / 防击穿 / 前缀失效 / 入住后房态缓存失效、契约 DTO 反序列化、游客房型库存 / 免锁房下单 / 取消 / 转入住衔接。

## 后续 Blazor 模块

新增 `src/HospiKeeper.Blazor` 时建议：

- 引用 `HospiKeeper.Contracts` 与 `HospiKeeper.Client`，通过现有 API 访问业务，不在 Web 端直连数据库；
- 继续由 `HospiKeeper.Api` 承担鉴权、权限与业务规则；
- 若需要共享视觉规范，可将 Ant Design Token 移植为 Blazor CSS 变量，业务契约无需改动。
