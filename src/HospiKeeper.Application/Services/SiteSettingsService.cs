using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using Sylvie233.Caching;

namespace HospiKeeper.Application.Services;

/// <summary>系统配置服务：Redis 持久化，未配置时使用 SiteSettings 默认值。</summary>
public sealed class SiteSettingsService : ISiteSettingsService
{
    private const int MaxFeatures = 8;
    private const int MaxNotices = 8;

    private readonly ICacheService _cache;
    private readonly IClock _clock;
    private readonly SessionContext _session;

    public SiteSettingsService(ICacheService cache, IClock clock, SessionContext session)
    {
        _cache = cache;
        _clock = clock;
        _session = session;
    }

    public async Task<SiteSettings> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _cache.GetAsync<SiteSettings>(CacheKeys.SiteSettings, cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        // 首次访问：写入默认配置，便于管理员打开设置页时直接编辑。
        settings = new SiteSettings();
        await _cache.SetPersistentAsync(CacheKeys.SiteSettings, settings, cancellationToken);
        return settings;
    }

    public async Task<SiteSettings> GetForAdminAsync(CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        return await GetPublicAsync(cancellationToken);
    }

    public async Task<SiteSettings> UpdateAsync(
        SiteSettings settings,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var normalized = Normalize(settings);
        normalized.UpdatedAt = _clock.Now;
        normalized.UpdatedBy = _session.DisplayName;

        var saved = await _cache.SetPersistentAsync(
            CacheKeys.SiteSettings,
            normalized,
            cancellationToken);
        if (!saved)
        {
            throw new DomainException("系统配置保存失败，请检查 Redis 服务是否可用。");
        }

        return normalized;
    }

    private static SiteSettings Normalize(SiteSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.Rating is < 0 or > 5)
        {
            throw new DomainException("评分需在 0 - 5 之间。");
        }

        if (settings.WalkToLakeMinutes is < 1 or > 60)
        {
            throw new DomainException("步行时间需在 1 - 60 分钟之间。");
        }

        settings.BrandName = Required(settings.BrandName, "品牌名称", 40);
        settings.BrandEnglishName = Required(settings.BrandEnglishName, "英文品牌名", 40);
        settings.BrandMark = Required(settings.BrandMark, "品牌标识字", 2);
        settings.LocationTagline = Optional(settings.LocationTagline, "位置标语", 60);
        settings.HeroTitlePrefix = Required(settings.HeroTitlePrefix, "首页主标题前缀", 40);
        settings.HeroAccent = Required(settings.HeroAccent, "首页高亮词", 20);
        settings.HeroTitleSuffix = Required(settings.HeroTitleSuffix, "首页主标题后缀", 20);
        settings.HeroSubtitle = Required(settings.HeroSubtitle, "首页副标题", 120);
        settings.Hotline = Required(settings.Hotline, "前台热线", 32);
        settings.Address = Required(settings.Address, "地址", 100);
        settings.Email = Required(settings.Email, "联系邮箱", 80);
        settings.CheckInTime = Required(settings.CheckInTime, "入住时间", 40);
        settings.CheckOutTime = Required(settings.CheckOutTime, "退房时间", 40);
        settings.ArrivalDeadline = Required(settings.ArrivalDeadline, "到店截止", 40);
        settings.PaymentNote = Required(settings.PaymentNote, "支付说明", 120);
        settings.DepositNote = Required(settings.DepositNote, "押金说明", 120);
        settings.FooterDescription = Required(settings.FooterDescription, "页脚品牌介绍", 300);
        settings.FooterText = Required(settings.FooterText, "页脚文案", 120);
        settings.DesktopAppName = Required(settings.DesktopAppName, "桌面端系统名称", 40);
        settings.DesktopVersion = Required(settings.DesktopVersion, "桌面端版本号", 20);
        settings.DesktopFooterText = Required(settings.DesktopFooterText, "桌面端状态栏文案", 80);
        settings.AboutDescription = Required(settings.AboutDescription, "关于描述", 300);

        settings.Features = (settings.Features ?? [])
            .Where(f => f is not null
                        && (!string.IsNullOrWhiteSpace(f.Title) || !string.IsNullOrWhiteSpace(f.Description)))
            .Take(MaxFeatures)
            .Select(f => new SiteFeature
            {
                Icon = Required(f.Icon, "特色图标", 8),
                Title = Required(f.Title, "特色标题", 20),
                Description = Required(f.Description, "特色说明", 80)
            })
            .ToList();

        settings.Notices = (settings.Notices ?? [])
            .Where(n => n is not null
                        && (!string.IsNullOrWhiteSpace(n.Title) || !string.IsNullOrWhiteSpace(n.Line1)))
            .Take(MaxNotices)
            .Select(n => new SiteNotice
            {
                Title = Required(n.Title, "须知标题", 20),
                Line1 = Required(n.Line1, "须知内容", 80),
                Line2 = Optional(n.Line2, "须知内容", 80)
            })
            .ToList();

        return settings;
    }

    private static string Required(string? value, string field, int maxLength)
        => InputGuard.Text(value, field, maxLength);

    private static string Optional(string? value, string field, int maxLength)
        => InputGuard.OptionalText(value, field, maxLength) ?? string.Empty;
}
