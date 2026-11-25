using System.Text.Json.Serialization;
using HospiKeeper.Api.Middleware;
using HospiKeeper.Api.Security;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var dbOptions = new DatabaseOptions();
builder.Configuration.GetSection("Database").Bind(dbOptions);
ApplyEnvironmentOverrides(dbOptions);

builder.Services.AddSingleton(dbOptions);
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddInfrastructure(dbOptions);
builder.Services.AddSingleton<ApiTokenStore>();

builder.Services.AddScoped<SessionContext>();
builder.Services.AddScoped<IOperationLogger, OperationLogger>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRecordService, RecordService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// 启动时执行 EF Core 迁移与首次演示数据灌入；数据库不可用时快速失败并打印明确信息。
try
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
    app.Logger.LogInformation("HospiKeeper EF Core / MySQL ready: {Database}", dbOptions.SafeDisplay);
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "HospiKeeper 数据库初始化失败，请检查 Scripts/docker-compose.yml 中的 MySQL 配置");
    throw;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HospiKeeper API v1");
    options.RoutePrefix = "swagger";
});
app.UseMiddleware<ApiAuthMiddleware>();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    storage = "mysql",
    time = DateTime.Now
}));

app.MapControllers();
app.Run();

static void ApplyEnvironmentOverrides(DatabaseOptions options)
{
    options.Host = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_HOST")
                   ?? Environment.GetEnvironmentVariable("MYSQL_HOST")
                   ?? options.Host;

    var port = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_PORT")
               ?? Environment.GetEnvironmentVariable("MYSQL_PORT");
    if (int.TryParse(port, out var parsedPort))
    {
        options.Port = parsedPort;
    }

    options.Name = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_NAME")
                   ?? Environment.GetEnvironmentVariable("MYSQL_DATABASE")
                   ?? options.Name;

    options.User = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_USER")
                   ?? Environment.GetEnvironmentVariable("MYSQL_USER")
                   ?? options.User;

    options.Password = Environment.GetEnvironmentVariable("HOSPITKEEPER_DB_PASSWORD")
                       ?? Environment.GetEnvironmentVariable("MYSQL_PASSWORD")
                       ?? options.Password;
}
