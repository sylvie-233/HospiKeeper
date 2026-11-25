# HospiKeeper · 云栖民宿管理系统

面向中小型民宿 / 小旅馆的桌面经营系统，覆盖「登录与权限、房态看板、入住登记、预定、在住、追加消费、退房结算、记录查询、统计报表、房间 / 房型 / 账号管理」的完整闭环。

项目采用 **Avalonia 桌面客户端 → ASP.NET Core API → EF Core / MySQL** 的分层架构，并内置一套参考 Ant Design 规范实现的可复用控件库。当前代码主要用于验证 .NET 桌面 + Web API + EF Core 的主流技术栈组合，同时为后续新增 Blazor 模块预留了清晰的模块边界。

## 技术栈

| 层 | 技术 |
|----|------|
| 运行时 | .NET 10（LTS） |
| 桌面客户端 | Avalonia 12.2 · CommunityToolkit.Mvvm 8.4 |
| UI 设计系统 | 自研 Ant 风格控件库 `Sylvie233.UI`（TemplatedControl / ControlTheme） |
| API | ASP.NET Core 10 · Controllers · Swagger |
| 持久化 | EF Core 10.0.12 · MySql.EntityFrameworkCore 10.0.12（官方 MySQL Provider） |
| 缓存 | 自研 `Sylvie233.Caching`：Memory / Redis 可切换 · StackExchange.Redis 3.4 |
| 基础设施 | MySQL 8 · Redis 7（`Scripts/docker-compose.yml`，3306 / 6379） |
| 测试 | xUnit · EF Core InMemory（29 个用例） |

## 解决方案结构

```
HospiKeeper.slnx
├─ src/
│  ├─ HospiKeeper.Domain/          # 实体、枚举、计费领域服务（无框架依赖）
│  ├─ HospiKeeper.Contracts/       # 客户端/API 共享请求、响应与 API DTO
│  ├─ HospiKeeper.Application/     # 业务服务 + 仓储/工作单元 + 缓存失效协调
│  ├─ Sylvie233.Caching/         # ICacheService 端口 + Memory / Redis 实现（无业务依赖）
│  ├─ HospiKeeper.Infrastructure/  # EF Core DbContext、映射、仓储、迁移、演示数据
│  ├─ HospiKeeper.Api/             # 独立 Web API（Token 鉴权 + REST + Swagger）
│  ├─ HospiKeeper.Client/          # 桌面端 HTTP 客户端（IHospiKeeperApi）
│  ├─ Sylvie233.UI/              # 可复用 UI 设计系统与自研控件
│  └─ HospiKeeper.Desktop/         # Avalonia 桌面应用（MVVM 页面 + 弹窗）
├─ tests/
│  └─ HospiKeeper.Tests/           # 领域与业务流程测试（EF Core InMemory）
├─ Docs/                           # 原型、功能设计、MySQL 数据库设计参考
├─ Scripts/                        # docker compose 与开发启动脚本
├─ CLAUDE.md                       # 项目开发规范与 Claude Code 指引
└─ dotnet-tools.json               # 本地 dotnet-ef 工具清单
```

依赖方向：`Desktop → Client → Contracts → Domain`，并引用共享控件库 `Sylvie233.UI`；`Api → Application → Contracts → Domain`；`Infrastructure → Application/Contracts`；`Application → Sylvie233.Caching`（使用共享缓存端口）。`Sylvie233.UI` 与 `Sylvie233.Caching` 均不依赖业务模块，Blazor 等新客户端可直接复用 `Client` / `Contracts` / `Sylvie233.UI` 设计规范 / `Api`。

## 自研 UI 控件库

`Sylvie233.UI` 不依赖任何第三方 UI 组件，包含：

- `AntButton`：Primary / Default / Danger / Text / Link 变体，Small / Middle / Large 尺寸
- `AntTag`：默认 / 成功 / 处理中 / 警告 / 失败 / 紫色 / 青色
- `AntCard`、`AntStatCard`、`AntPageHeader`、`AntSectionTitle`
- `AntFormItem`：标签、必填星号、错误信息
- `AntModal`：主窗口内模态弹窗容器
- `AntRoomCard`：六种房态渐变卡片与交互态
- `AntTable` / `AntTableColumn`：替代废弃 DataGrid 的轻量数据表格
- `AntBarChart`：自绘柱状图，无图表库依赖
- `AntEmpty`、`AntToast`、`AntIcons`
- `Theme/`：Ant Design 色彩 Token、控件主题、输入框 / 下拉 / 菜单 / 票据等统一样式

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
Redis   localhost:6379（无密码，仅作为缓存）
```

> 数据库表结构与演示数据由 API 启动时的 EF Core Migrations 自动创建；
> `Docs/民宿管理系统-数据库设计-v1.sql` 保留为设计参考，不再由 compose 自动执行。
> Redis 以 `--maxmemory 256mb --maxmemory-policy allkeys-lru` 运行，不持久化缓存数据。

### 2. 启动 API

```bash
dotnet run --project src/HospiKeeper.Api
```

- API：`http://localhost:5080`
- Swagger：`http://localhost:5080/swagger`
- 首次启动自动执行 `Database.MigrateAsync()`，并在空库中生成演示数据。

### 3. 启动桌面客户端

```bash
dotnet run --project src/HospiKeeper.Desktop
```

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
pwsh ./manage.ps1 down            # 停止 API 与容器
```

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

支持环境变量覆盖：

- `HOSPITKEEPER_DB_HOST` / `MYSQL_HOST`
- `HOSPITKEEPER_DB_PORT` / `MYSQL_PORT`
- `HOSPITKEEPER_DB_NAME` / `MYSQL_DATABASE`
- `HOSPITKEEPER_DB_USER` / `MYSQL_USER`
- `HOSPITKEEPER_DB_PASSWORD` / `MYSQL_PASSWORD`

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

当前覆盖：计费口径、登录 / 停用 / 越权、账号唯一性、改密、入住校验、预定 / 取消 / 转入住、追加消费、退房结算、房间历史保护、前台数据隔离、报表指标与区间统计、缓存读写 / 防击穿 / 前缀失效 / 入住后房态缓存失效。

## 后续 Blazor 模块

新增 `src/HospiKeeper.Blazor` 时建议：

- 引用 `HospiKeeper.Contracts` 与 `HospiKeeper.Client`，通过现有 API 访问业务，不在 Web 端直连数据库；
- 继续由 `HospiKeeper.Api` 承担鉴权、权限与业务规则；
- 若需要共享视觉规范，可将 Ant Design Token 移植为 Blazor CSS 变量，业务契约无需改动。
