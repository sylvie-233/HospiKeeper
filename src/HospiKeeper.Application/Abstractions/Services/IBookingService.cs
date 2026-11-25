using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>前台核心流程：看板、在住、入住、退房与预定。</summary>
public interface IBookingService
{
    /// <summary>房态看板：每个房间及其在住 / 待入住预定。</summary>
    Task<IReadOnlyList<BoardRoom>> GetBoardAsync(CancellationToken cancellationToken = default);

    /// <summary>在住列表，可按客人 / 房号关键字过滤。</summary>
    Task<IReadOnlyList<StayInfo>> GetLivingStaysAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取单条在住详情。</summary>
    Task<StayInfo?> GetStayAsync(int checkInId, CancellationToken cancellationToken = default);

    /// <summary>查询预定列表。</summary>
    Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取预定详情。</summary>
    Task<ReservationDto?> GetReservationAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    /// <summary>前台汇总计数：在住、待入住预定、打扫中。</summary>
    Task<FrontDeskCounts> GetCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>办理入住；可关联预定并登记同住人。</summary>
    Task<CheckInRecordDto> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>按实际退房时间试算房费、消费与押金差额。</summary>
    BillingResult QuoteCheckout(
        CheckInRecordDto record,
        DateTime actualOutTime,
        decimal damage,
        BillingMode mode);

    /// <summary>办理退房并生成结算单快照。</summary>
    Task<CheckoutBillDto> CheckoutAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>为在住记录追加消费。</summary>
    Task AddExtraChargeAsync(ExtraChargeRequest request, CancellationToken cancellationToken = default);

    /// <summary>创建预定并将房间置为已预定。</summary>
    Task<ReservationDto> CreateReservationAsync(
        ReservationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>取消预定并释放房间。</summary>
    Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default);
}
