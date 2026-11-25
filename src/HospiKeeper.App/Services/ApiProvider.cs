using HospiKeeper.Client;

namespace HospiKeeper.App.Services;

/// <summary>持有当前 API 客户端，并支持在登录页切换服务器地址。</summary>
public sealed class ApiProvider
{
    private IHospiKeeperApi _api;

    public ApiProvider()
    {
        BaseUrl = Normalize(AppConfig.ApiBaseUrl);
        _api = CreateClient(BaseUrl);
    }

    public string BaseUrl { get; private set; }

    public IHospiKeeperApi Api => _api;

    /// <summary>保存并切换 API 地址；切换后会重建客户端，需要重新登录。</summary>
    public bool SetBaseUrl(string value)
    {
        var normalized = Normalize(value);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        if (string.Equals(normalized, BaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        AppConfig.ApiBaseUrl = normalized;
        BaseUrl = normalized;
        _api = CreateClient(normalized);
        return true;
    }

    private static string Normalize(string value)
    {
        var text = string.IsNullOrWhiteSpace(value)
            ? AppConfig.DefaultApiBaseUrl
            : value.Trim();
        return text.TrimEnd('/');
    }

    private static IHospiKeeperApi CreateClient(string baseUrl)
        => new HospiKeeperApiClient(new HttpClient
        {
            BaseAddress = new Uri(baseUrl + "/"),
            Timeout = TimeSpan.FromSeconds(25)
        });
}
