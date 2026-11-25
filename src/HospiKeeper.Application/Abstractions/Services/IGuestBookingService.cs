using HospiKeeper.Contracts;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>游客在线预订用例：房型库存、下单、手机号查询与取消。</summary>
public interface IGuestBookingService
{
    /// <summary>查询指定日期段各房型的实时可订量。</summary>
    Task<GuestAvailabilityResult> GetAvailabilityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        CancellationToken cancellationToken = default);

    /// <summary>首页统计数据。</summary>
    Task<HotelSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>按手机号查询游客预订记录。</summary>
    Task<IReadOnlyList<GuestReservationDto>> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default);

    /// <summary>提交游客预订：校验库存、生成 R 单号、写入预订（不锁具体房间）。</summary>
    Task<GuestReservationDto> CreateAsync(
        GuestBookingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>游客取消待到店预订（手机号校验，不释放具体房间）。</summary>
    Task CancelAsync(
        string resNo,
        string phone,
        CancellationToken cancellationToken = default);
}
