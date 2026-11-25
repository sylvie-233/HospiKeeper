using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>房型查询与变更。</summary>
public interface IRoomTypeRepository
{
    /// <summary>查询全部房型。</summary>
    Task<IReadOnlyList<RoomType>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询房型。</summary>
    Task<RoomType?> GetByIdAsync(int roomTypeId, CancellationToken cancellationToken = default);

    /// <summary>判断房型名称是否已存在。</summary>
    Task<bool> NameExistsAsync(
        string name,
        int? excludeRoomTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>统计引用该房型的房间数。</summary>
    Task<int> CountRoomsAsync(int roomTypeId, CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(RoomType roomType);

    /// <summary>标记删除，等待统一保存。</summary>
    void Remove(RoomType roomType);
}
