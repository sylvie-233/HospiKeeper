using HospiKeeper.Client;
using HospiKeeper.Contracts;

namespace HospiKeeper.Web.Services;

/// <summary>同一 Blazor 电路内共享一份站点配置，避免每个组件重复请求。</summary>
public sealed class SiteSettingsState
{
    private readonly IGuestBookingApi _api;
    private Task<SiteSettings>? _loadTask;

    public SiteSettingsState(IGuestBookingApi api) => _api = api;

    public Task<SiteSettings> GetAsync()
        => _loadTask ??= LoadAsync();

    private async Task<SiteSettings> LoadAsync()
    {
        try
        {
            return await _api.GetPublicSettingsAsync();
        }
        catch (Exception)
        {
            // API 暂不可用时使用 DTO 默认值，保证门户仍可渲染。
            return new SiteSettings();
        }
    }
}
