using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>房间查询与变更。</summary>
public interface IRoomRepository
{
    /// <summary>按楼层、房型、房态查询房间。</summary>
    Task<IReadOnlyList<Room>> GetAllAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>查询全部空闲房间。</summary>
    Task<IReadOnlyList<Room>> GetFreeAsync(CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询房间。</summary>
    Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>查询已有楼层并升序返回。</summary>
    Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default);

    /// <summary>查询某房型下的全部房间。</summary>
    Task<IReadOnlyList<Room>> GetByTypeAsync(int roomTypeId, CancellationToken cancellationToken = default);

    /// <summary>判断房号是否已被占用。</summary>
    Task<bool> RoomNoExistsAsync(
        string roomNo,
        int? excludeRoomId = null,
        CancellationToken cancellationToken = default);

    /// <summary>判断房间是否存在入住 / 预定 / 账单等历史业务数据。</summary>
    Task<bool> HasBusinessHistoryAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>统计指定房态的房间数。</summary>
    Task<int> CountByStatusAsync(RoomStatus status, CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(Room room);

    /// <summary>标记删除，等待统一保存。</summary>
    void Remove(Room room);
}
