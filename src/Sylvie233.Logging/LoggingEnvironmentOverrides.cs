using Serilog.Events;

namespace Sylvie233.Logging;

/// <summary>通过环境变量覆盖日志配置，便于容器 / 运维脚本调整而无需改代码。</summary>
public static class LoggingEnvironmentOverrides
{
    public const string LevelVariable = "SYLVIE233_LOG_LEVEL";
    public const string ConsoleVariable = "SYLVIE233_LOG_CONSOLE";
    public const string FileVariable = "SYLVIE233_LOG_FILE";

    public static void Apply(LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var level = Environment.GetEnvironmentVariable(LevelVariable);
        if (!string.IsNullOrWhiteSpace(level)
            && Enum.TryParse<LogEventLevel>(level, ignoreCase: true, out var parsedLevel))
        {
            options.MinimumLevel = parsedLevel;
        }

        if (TryParseBoolean(ConsoleVariable, out var enableConsole))
        {
            options.EnableConsole = enableConsole;
        }

        if (TryParseBoolean(FileVariable, out var enableFile))
        {
            options.EnableFile = enableFile;
        }
    }

    private static bool TryParseBoolean(string variable, out bool value)
    {
        value = false;
        var raw = Environment.GetEnvironmentVariable(variable);
        return !string.IsNullOrWhiteSpace(raw) && bool.TryParse(raw.Trim(), out value);
    }
}
