using System.Text.Json;
using Sylvie233.Logging;

namespace HospiKeeper.Desktop.Services;

/// <summary>读取 appsettings.json 中的 Logging:Sylvie233 配置。</summary>
internal static class LoggingSettings
{
    private static readonly LoggingOptions Defaults = new()
    {
        ApplicationName = "HospiKeeper.Desktop",
        FileNamePrefix = "desktop"
    };

    public static LoggingOptions Load()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                return Defaults;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (!document.RootElement.TryGetProperty("Logging", out var logging)
                || !logging.TryGetProperty("Sylvie233", out var section))
            {
                return Defaults;
            }

            var options = new LoggingOptions
            {
                ApplicationName = ReadString(section, "ApplicationName") ?? Defaults.ApplicationName,
                FileNamePrefix = ReadString(section, "FileNamePrefix") ?? Defaults.FileNamePrefix,
                LogDirectory = ReadString(section, "LogDirectory"),
                EnableConsole = ReadBool(section, "EnableConsole") ?? Defaults.EnableConsole,
                EnableFile = ReadBool(section, "EnableFile") ?? Defaults.EnableFile,
                RetainedFileCountLimit = ReadInt(section, "RetainedFileCountLimit")
                                         ?? Defaults.RetainedFileCountLimit
            };

            if (ReadString(section, "MinimumLevel") is { } level
                && Enum.TryParse<Serilog.Events.LogEventLevel>(level, ignoreCase: true, out var parsed))
            {
                options.MinimumLevel = parsed;
            }

            return options;
        }
        catch
        {
            return Defaults;
        }
    }

    private static string? ReadString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? ReadBool(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    private static int? ReadInt(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var parsed)
                ? parsed
                : null;
}
