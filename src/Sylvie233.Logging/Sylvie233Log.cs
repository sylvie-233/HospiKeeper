using Serilog;

namespace Sylvie233.Logging;

/// <summary>
/// 静态日志门面，便于非 DI 场景（桌面端启动、未处理异常）使用；
/// 业务服务优先使用构造注入的 ILogger。
/// </summary>
public static class Sylvie233Log
{
    public static Serilog.ILogger Logger => Log.Logger;

    /// <summary>初始化静态 Log.Logger；启动早期调用，可捕获宿主构建前的异常。</summary>
    public static void Initialize(LoggingOptions options) => Log.Logger = Sylvie233Logging.CreateLogger(options);

    public static Serilog.ILogger ForContext<T>() => Log.ForContext<T>();

    public static Serilog.ILogger ForContext(Type type) => Log.ForContext(type);

    public static void CloseAndFlush() => Log.CloseAndFlush();
}
