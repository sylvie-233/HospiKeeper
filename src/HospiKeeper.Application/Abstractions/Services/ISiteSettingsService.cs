using HospiKeeper.Contracts;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>游客 Web / 桌面端静态配置的读取与保存。</summary>
public interface ISiteSettingsService
{
    /// <summary>公开读取配置；未配置时返回默认值并写入 Redis。</summary>
    Task<SiteSettings> GetPublicAsync(CancellationToken cancellationToken = default);

    /// <summary>管理员读取配置。</summary>
    Task<SiteSettings> GetForAdminAsync(CancellationToken cancellationToken = default);

    /// <summary>管理员保存配置到 Redis 持久 key。</summary>
    Task<SiteSettings> UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default);
}
