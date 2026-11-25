using Serilog.Events;

namespace Sylvie233.Logging;

/// <summary>通用日志配置：应用名、输出目标、级别、滚动与保留策略。</summary>
public sealed class LoggingOptions
{
    /// <summary>写入日志事件的 Application 属性，便于集中检索。</summary>
    public string ApplicationName { get; set; } = "App";

    /// <summary>日志文件名前缀，文件形如 <c>api-20261125.log</c>。</summary>
    public string FileNamePrefix { get; set; } = "app";

    /// <summary>
    /// 日志目录。为空时自动定位仓库根目录下的 logs/；
    /// 相对路径按程序所在目录解析；也可用环境变量 SYLVIE233_LOG_DIRECTORY 覆盖。
    /// </summary>
    public string? LogDirectory { get; set; }

    public bool EnableConsole { get; set; } = true;

    public bool EnableFile { get; set; } = true;

    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>滚动的日志文件保留数量。</summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>单文件大小上限，超出后滚动。</summary>
    public long FileSizeLimitBytes { get; set; } = 20 * 1024 * 1024;

    /// <summary>是否追加机器名 / 线程信息，便于排查多实例与并发问题。</summary>
    public bool EnrichEnvironment { get; set; } = true;

    public bool EnrichThread { get; set; } = true;

    /// <summary>默认控制台输出格式。</summary>
    public const string ConsoleTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

    /// <summary>默认文件输出格式。</summary>
    public const string FileTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj} " +
        "{Properties:j}{NewLine}{Exception}";
}
