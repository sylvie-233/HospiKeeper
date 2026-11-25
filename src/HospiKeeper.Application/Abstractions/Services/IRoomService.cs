using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>房间、房型与房态维护用例。</summary>
public interface IRoomService
{
    /// <summary>按楼层、房型、房态查询房间。</summary>
    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>查询全部空闲房间。</summary>
    Task<IReadOnlyList<RoomDto>> GetFreeRoomsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取单个房间。</summary>
    Task<RoomDto?> GetRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>查询已有楼层。</summary>
    Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default);

    /// <summary>查询全部房型。</summary>
    Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>创建房间。</summary>
    Task<RoomDto> CreateRoomAsync(RoomInput input, CancellationToken cancellationToken = default);

    /// <summary>修改房间。</summary>
    Task UpdateRoomAsync(int roomId, RoomInput input, CancellationToken cancellationToken = default);

    /// <summary>删除房间；存在历史单据时禁止删除。</summary>
    Task DeleteRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>保洁完成，打扫中房间恢复为空闲。</summary>
    Task MarkCleaningCompleteAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>把维修 / 停用房间恢复为空闲。</summary>
    Task RestoreRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>设置维修或停用状态。</summary>
    Task SetMaintenanceAsync(
        int roomId,
        RoomStatus status,
        string? note,
        CancellationToken cancellationToken = default);

    /// <summary>创建房型。</summary>
    Task<RoomTypeDto> CreateRoomTypeAsync(RoomTypeInput input, CancellationToken cancellationToken = default);

    /// <summary>修改房型；改名会同步房间冗余名称，改价会写操作日志。</summary>
    Task UpdateRoomTypeAsync(
        int roomTypeId,
        RoomTypeInput input,
        CancellationToken cancellationToken = default);

    /// <summary>删除房型；仍被房间引用时禁止删除。</summary>
    Task DeleteRoomTypeAsync(int roomTypeId, CancellationToken cancellationToken = default);
}
