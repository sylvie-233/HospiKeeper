using System.Text.Json;

namespace HospiKeeper.Desktop.Services;

/// <summary>桌面端本地配置（API 地址）。</summary>
public sealed class AppSettings
{
    public string ApiBaseUrl { get; private init; } = "http://localhost:5080";

    public static AppSettings Load()
    {
        var fromEnv = Environment.GetEnvironmentVariable("HOSPITKEEPER_API_URL");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return new AppSettings { ApiBaseUrl = fromEnv.TrimEnd('/') };
        }

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                if (document.RootElement.TryGetProperty("Api", out var api)
                    && api.TryGetProperty("BaseUrl", out var baseUrl))
                {
                    var value = baseUrl.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return new AppSettings { ApiBaseUrl = value.TrimEnd('/') };
                    }
                }
            }
        }
        catch
        {
            // 配置损坏时回退默认地址
        }

        return new AppSettings();
    }
}
