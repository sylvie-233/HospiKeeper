namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>MySQL 连接配置。默认值与 Scripts/docker-compose.yml 保持一致。</summary>
public sealed class DatabaseOptions
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 3306;
    public string Name { get; set; } = "hospikeeper";
    public string User { get; set; } = "hospikeeper";
    public string Password { get; set; } = "123456";

    public string BuildConnectionString() =>
        $"Server={Host};Port={Port};Database={Name};User ID={User};Password={Password};" +
        "CharSet=utf8mb4;SslMode=Disabled;AllowPublicKeyRetrieval=True;" +
        "Connection Timeout=8;Default Command Timeout=30;";

    public string SafeDisplay => $"{User}@{Host}:{Port}/{Name}";
}
