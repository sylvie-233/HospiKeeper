using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>预定查询与变更。</summary>
public interface IReservationRepository
{
    /// <summary>按状态查询预定；状态为空时返回全部。</summary>
    Task<IReadOnlyList<Reservation>> GetAllAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询预定。</summary>
    Task<Reservation?> GetByIdAsync(int reservationId, CancellationToken cancellationToken = default);

    /// <summary>查询全部待到店预定。</summary>
    Task<IReadOnlyList<Reservation>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>按手机号查询预订（游客「我的预订」）。</summary>
    Task<IReadOnlyList<Reservation>> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    /// <summary>按预订号查询。</summary>
    Task<Reservation?> GetByResNoAsync(string resNo, CancellationToken cancellationToken = default);

    /// <summary>查询与指定房型、日期段重叠的待到店预订（库存扣减口径）。</summary>
    Task<IReadOnlyList<Reservation>> GetOverlappingPendingAsync(
        int typeId,
        DateTime checkInDate,
        DateTime checkOutDate,
        CancellationToken cancellationToken = default);

    /// <summary>判断房间是否已有待到店预定。</summary>
    Task<bool> HasPendingForRoomAsync(
        int roomId,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>判断房间是否存在任意历史预定。</summary>
    Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>统计待到店预定数量。</summary>
    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(Reservation reservation);
}
