# Sylvie233.Logging · Serilog 统一日志

与业务无关的 Serilog 4 配置，为 API、桌面端与游客 Web 提供统一的控制台 / 滚动文件日志。

## 能力

- 控制台（可选）+ 按天滚动文件：`logs/<prefix>-yyyyMMdd.log`；
- 异步文件 sink，默认保留 30 个文件、单文件上限 20MB；
- 结构化字段：`Application`、`MachineName`、`EnvironmentName`、`ThreadId`；
- 宿主构建前初始化静态 `Log.Logger` 作为兜底，数据库启动失败也能落盘；
- API 通过 `builder.Host.UseSylvie233Logging(options)` 接管 `ILogger` 并启用请求日志；
- 桌面端 / Web 在启动与未处理异常时写入各自日志；
- `Microsoft` / EF Core 默认压制到 Warning。

## 配置

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

环境变量：

| 变量 | 作用 |
| --- | --- |
| `SYLVIE233_LOG_DIRECTORY` | 日志目录 |
| `SYLVIE233_LOG_LEVEL` | 最低级别 |
| `SYLVIE233_LOG_CONSOLE` | 是否输出控制台 |
| `SYLVIE233_LOG_FILE` | 是否写文件 |

容器内设置 `SYLVIE233_LOG_FILE=false`，由 `docker logs` 查看。

## 使用约定

- 业务服务构造注入 `ILogger<T>`；
- 启动兜底 / 桌面端使用 `Sylvie233Log.ForContext<T>()` / `Sylvie233Log.Logger`；
- 不要记录密码、令牌、身份证号等敏感明文；
- HTTP 请求日志必须在 `ExceptionHandlingMiddleware` 外层，模板为 `HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0} ms`；
- 不使用 `Console.WriteLine` / `Debug.WriteLine`。
