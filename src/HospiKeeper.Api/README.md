# HospiKeeper.Api · Web API

ASP.NET Core 10 Web API，是桌面端、游客 Web 与 Android 管理端共用的业务入口。控制器保持轻薄，业务规则集中在 `HospiKeeper.Application`，数据访问通过 EF Core。

## 启动

```bash
# 先启动 MySQL + Redis
docker compose -f Scripts/docker-compose.yml up -d --wait

dotnet run --project src/HospiKeeper.Api
```

| 地址 | 说明 |
| --- | --- |
| `http://localhost:5080` | API |
| `http://localhost:5080/health` | 健康检查 |
| `http://localhost:5080/scalar/v1` | Scalar 交互式文档 |
| `http://localhost:5080/openapi/v1.json` | OpenAPI 3.1 JSON |

首次启动执行 `Database.MigrateAsync()`，空库会调用 `DemoDataSeeder` 写入演示数据。环境变量 `ASPNETCORE_ENVIRONMENT=Development` 时开放 Scalar / OpenAPI。

## 鉴权与权限

- 登录：`POST /api/auth/login`，返回内存 Bearer Token（12 小时）。
- 除 `/health`、`/scalar`、`/openapi`、登录接口、`/api/public/*` 外均需 `Authorization: Bearer <token>`。
- 服务端每个请求重新加载用户角色和启用状态，停用 / 改角色即时生效。
- 前台：入住、退房、追加消费、打扫；记录与日志只能查看本人经办。
- 管理员：房间 / 房型 / 账号管理，查看全部记录与日志。
- 服务端校验权限，不依赖 UI 隐藏。

## 分层与契约

```text
Api → Application → Contracts → Domain
Api → Infrastructure → Sylvie233.Database
Application → Sylvie233.Caching
```

- 控制器只做参数绑定 → 调用 Application 服务 → 返回 DTO。
- `HospiKeeper.Contracts` 按 Auth / Rooms / FrontDesk / Reservations / Records / Reports / Common 组织，请求以 `Request` / `Input` 结尾，响应以 `Dto` 结尾。
- DataAnnotations 校验放在 record 构造参数上：`[param: Required(...)]`，不要标 `[property:]`。
- 跨字段、唯一性、状态流转由 Application 的 `InputGuard` 校验。
- `DomainException` → 400，`ForbiddenException` → 403，未处理异常 → 500。
- 实体 → DTO 映射集中在 `Application/Mapping/ContractMappings.cs`。

## OpenAPI / Scalar

`Program.cs` 只保留启动骨架，OpenAPI 扩展在 `OpenApi/`：

- `HospiKeeperOpenApiDocumentTransformer`：标题、描述、Bearer 安全方案；
- `HospiKeeperOpenApiOperationTransformer`：统一错误响应，公开接口取消鉴权；
- 控制器使用 `[Tags("中文分组")]`，接口补中文 `<summary>`；
- 新增公开接口时同步更新公开路径与鉴权中间件。

## 配置

### 数据库

`appsettings.json`：

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

支持环境变量：`HOSPITKEEPER_DB_*` → `SYLVIE233_DB_*` → `MYSQL_*`。容器内使用 `HOSPITKEEPER_DB_HOST=mysql`。

### 缓存

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

`Cache:Provider` 可切换 `Redis` / `Memory`，也可用 `Cache__Provider` 覆盖。缓存端口与实现见 [Sylvie233.Caching](../Sylvie233.Caching/README.md)。

### 日志

使用 [Sylvie233.Logging](../Sylvie233.Logging/README.md)，配置在 `Logging:Sylvie233`。容器内设置 `SYLVIE233_LOG_FILE=false`，由 `docker logs` 查看。

## 本地运维

```powershell
pwsh ./manage.ps1 up
pwsh ./manage.ps1 status
pwsh ./manage.ps1 logs -Target api -Follow
pwsh ./manage.ps1 image:build   # 构建 API / Web 镜像
pwsh ./manage.ps1 image:up      # 启动 API / Web 容器
```

## 容器化

API 与游客 Web 使用多阶段 Dockerfile，基础镜像为本机已有的 .NET 10 SDK / ASP.NET 运行时：

- 镜像：`hospikeeper-api:local`、`hospikeeper-web:local`
- 容器端口 `8080`，宿主机映射 `5080`（API）/ `5081`（Web）
- Compose 使用 `apps` profile，服务名 `api` / `web`
- 容器内 `SYLVIE233_LOG_FILE=false`，日志走 stdout
- Web 通过 `Api__BaseUrl=http://api:8080` 访问 API

```powershell
pwsh ./manage.ps1 image:build
pwsh ./manage.ps1 image:up
pwsh ./manage.ps1 image:status
pwsh ./manage.ps1 image:logs -Target api -Follow
pwsh ./manage.ps1 image:down
```

修改 API / Web 源码后先 `image:build`，再 `image:up`；`image:up` 会强制重建运行中的容器。

## 初始账号

| 账号 | 密码 | 角色 | 状态 |
| --- | --- | --- | --- |
| `admin` | `123456` | 管理员 | 启用 |
| `front01` | `123456` | 前台 | 启用 |
| `front02` | `123456` | 前台 | 停用（验证停用拦截） |

> 页面不展示演示账号；以上仅用于本地启动。

## 测试

```bash
dotnet test
```

测试使用 EF Core InMemory + `ServiceTestContext` 构造完整服务栈（含 `DemoDataSeeder`），覆盖登录 / 权限、计费、入住 / 退房 / 预定、追加消费、报表、缓存、契约序列化与游客库存等 38 个用例。

## 相关文档

- [EF Core 持久化](../HospiKeeper.Infrastructure/README.md)
- [自研缓存](../Sylvie233.Caching/README.md)
- [统一日志](../Sylvie233.Logging/README.md)
- [数据库共享模块](../Sylvie233.Database/README.md)
- [根目录开发约定](../../CLAUDE.md)
