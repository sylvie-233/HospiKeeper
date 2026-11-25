using HospiKeeper.Client;
using HospiKeeper.Web.Components;
using HospiKeeper.Web.Services;
using Sylvie233.Logging;

var builder = WebApplication.CreateBuilder(args);

// 与 API / 桌面端一致：启动早期初始化 Serilog，日志写入 logs/web-*.log。
var loggingOptions = new LoggingOptions
{
    ApplicationName = "HospiKeeper.Web",
    FileNamePrefix = "web"
};
builder.Configuration.GetSection("Logging:Sylvie233").Bind(loggingOptions);
Sylvie233Log.Initialize(loggingOptions);
builder.Host.UseSylvie233Logging(loggingOptions);

try
{
    var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5080";

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Web 模块不直连数据库，只通过公开 API 访问业务数据。
    builder.Services.AddHttpClient<IGuestBookingApi, GuestBookingApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(20);
    });
    builder.Services.AddScoped<ToastService>();
    builder.Services.AddScoped<SiteSettingsState>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Sylvie233Log.Logger.Fatal(ex, "HospiKeeper.Web 启动失败");
    throw;
}
finally
{
    await Sylvie233Log.CloseAndFlushAsync();
}
