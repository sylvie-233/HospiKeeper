namespace HospiKeeper.App.Services;

/// <summary>App 本地配置。默认地址适用于标准 Android 模拟器访问宿主机。</summary>
public static class AppConfig
{
    public const string DefaultApiBaseUrl = "http://10.0.2.2:5080";

    private const string ApiBaseUrlKey = "api_base_url";

    public static string ApiBaseUrl
    {
        get => Preferences.Default.Get(ApiBaseUrlKey, DefaultApiBaseUrl);
        set => Preferences.Default.Set(ApiBaseUrlKey, value);
    }
}
