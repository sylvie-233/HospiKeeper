using HospiKeeper.Contracts;

namespace HospiKeeper.Client;

/// <summary>游客预订门户访问 HospiKeeper.Api 公开接口的契约。</summary>
public interface IGuestBookingApi
{
    /// <summary>游客 Web 静态配置（品牌、首页文案、联系方式）。</summary>
    Task<SiteSettings> GetPublicSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>首页统计数据。</summary>
    Task<HotelSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>查询日期段内各房型可订量。</summary>
    Task<GuestAvailabilityResult> GetAvailabilityAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default);

    /// <summary>按手机号查询预订。</summary>
    Task<IReadOnlyList<GuestReservationDto>> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default);

    /// <summary>提交在线预订。</summary>
    Task<GuestReservationDto> CreateAsync(
        GuestBookingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>取消待到店预订。</summary>
    Task CancelAsync(
        string resNo,
        string phone,
        CancellationToken cancellationToken = default);
}
