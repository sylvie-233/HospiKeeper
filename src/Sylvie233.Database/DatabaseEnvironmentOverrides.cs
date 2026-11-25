using System.Globalization;

namespace Sylvie233.Database;

/// <summary>
/// 通过环境变量覆盖数据库配置。
/// 查找顺序：业务前缀（如 <c>HOSPITKEEPER_DB_HOST</c>）→ <c>SYLVIE233_DB_HOST</c> → <c>MYSQL_HOST</c>。
/// </summary>
public static class DatabaseEnvironmentOverrides
{
    public const string DefaultPrefix = "SYLVIE233";

    public static void Apply(DatabaseOptions options, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Host = Resolve(prefix, "HOST", "MYSQL_HOST") ?? options.Host;
        options.Name = Resolve(prefix, "NAME", "MYSQL_DATABASE") ?? options.Name;
        options.User = Resolve(prefix, "USER", "MYSQL_USER") ?? options.User;
        options.Password = Resolve(prefix, "PASSWORD", "MYSQL_PASSWORD") ?? options.Password;

        var port = Resolve(prefix, "PORT", "MYSQL_PORT");
        if (!string.IsNullOrWhiteSpace(port)
            && int.TryParse(port, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort)
            && parsedPort is > 0 and <= 65535)
        {
            options.Port = parsedPort;
        }
    }

    private static string? Resolve(string? prefix, string suffix, string legacyVariable)
    {
        foreach (var variable in BuildVariableNames(prefix, suffix))
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        var legacy = Environment.GetEnvironmentVariable(legacyVariable);
        return string.IsNullOrWhiteSpace(legacy) ? null : legacy.Trim();
    }

    private static IEnumerable<string> BuildVariableNames(string? prefix, string suffix)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var normalized = prefix.Trim().TrimEnd('_').ToUpperInvariant();
            if (normalized.Length > 0)
            {
                yield return $"{normalized}_DB_{suffix}";
            }
        }

        yield return $"{DefaultPrefix}_DB_{suffix}";
    }
}
