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
