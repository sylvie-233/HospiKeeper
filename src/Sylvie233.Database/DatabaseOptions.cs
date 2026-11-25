namespace Sylvie233.Database;

/// <summary>
/// MySQL 连接配置。默认值面向本地开发（与 docker-compose 常用端口一致），
/// 生产环境请通过 appsettings 或环境变量覆盖。
/// </summary>
public sealed class DatabaseOptions
{
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 3306;

    public string Name { get; set; } = "hospikeeper";

    public string User { get; set; } = "hospikeeper";

    public string Password { get; set; } = "123456";

    /// <summary>用于日志输出的安全描述（不含密码）。</summary>
    public string SafeDisplay => $"{User}@{Host}:{Port}/{Name}";

    public string BuildConnectionString() =>
        $"Server={Host};Port={Port};Database={Name};User ID={User};Password={Password};" +
        "CharSet=utf8mb4;SslMode=Disabled;AllowPublicKeyRetrieval=True;" +
        "Connection Timeout=8;Default Command Timeout=30;";

    /// <summary>构造配置并从环境变量覆盖，适合设计时工厂等无配置宿主场景。</summary>
    public static DatabaseOptions FromEnvironment(string? prefix = null)
    {
        var options = new DatabaseOptions();
        DatabaseEnvironmentOverrides.Apply(options, prefix);
        return options;
    }
}
