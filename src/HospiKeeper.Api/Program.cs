using HospiKeeper.Api.Configuration;
using Sylvie233.Logging;

// 1. 日志先于宿主构建初始化：配置 / 数据库错误也能落盘。
var builder = WebApplication.CreateBuilder(args);
var loggingOptions = new LoggingOptions
{
    ApplicationName = "HospiKeeper.Api",
    FileNamePrefix = "api"
};
builder.Configuration.GetSection("Logging:Sylvie233").Bind(loggingOptions);
Sylvie233Log.Initialize(loggingOptions);
builder.Host.UseSylvie233Logging(loggingOptions);

try
{
    // 2. 组合依赖：数据库 / 缓存 / 业务服务 / 鉴权 / MVC / OpenAPI / CORS。
    builder.Services.AddHospiKeeperApi(builder.Configuration);

    // 3. 构建宿主、初始化数据库、配置请求管道。
    var app = builder.Build();
    await app.InitializeHospiKeeperDatabaseAsync();
    app.UseHospiKeeperApi();
    app.Run();
}
catch (Exception ex)
{
    Sylvie233Log.Logger.Fatal(ex, "HospiKeeper.Api 启动失败");
    throw;
}
finally
{
    await Sylvie233Log.CloseAndFlushAsync();
}
