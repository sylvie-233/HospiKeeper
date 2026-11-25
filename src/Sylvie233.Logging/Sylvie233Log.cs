using Serilog;

namespace Sylvie233.Logging;

/// <summary>
/// 静态日志门面，便于非 DI 场景（桌面端启动、未处理异常）使用；
/// 业务服务优先使用构造注入的 ILogger。
/// </summary>
public static class Sylvie233Log
{
    /// <summary>当前静态日志记录器。</summary>
    public static Serilog.ILogger Logger => Log.Logger;

    /// <summary>初始化静态 Log.Logger；启动早期调用，可捕获宿主构建前的异常。</summary>
    public static void Initialize(LoggingOptions options) => Log.Logger = Sylvie233Logging.CreateLogger(options);

    /// <summary>创建带类型 SourceContext 的记录器。</summary>
    public static Serilog.ILogger ForContext<T>() => Log.ForContext<T>();

    /// <summary>创建带指定类型 SourceContext 的记录器。</summary>
    public static Serilog.ILogger ForContext(Type type) => Log.ForContext(type);

    /// <summary>刷新并关闭日志（同步）。</summary>
    public static void CloseAndFlush() => Log.CloseAndFlush();

    /// <summary>刷新并关闭日志（异步，应用退出前调用）。</summary>
    public static ValueTask CloseAndFlushAsync() => Log.CloseAndFlushAsync();
}
