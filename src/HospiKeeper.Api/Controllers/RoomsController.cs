using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>房间管理、房态维护与保洁操作。</summary>
[ApiController]
[Route("api/rooms")]
[Tags("房间管理")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomsController(IRoomService rooms) => _rooms = rooms;

/// <summary>按楼层、房型、房态查询房间。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetAll(
        [FromQuery] int? floor,
        [FromQuery] int? typeId,
        [FromQuery] RoomStatus? status,
        CancellationToken cancellationToken)
        => Ok(await _rooms.GetRoomsAsync(floor, typeId, status, cancellationToken));

/// <summary>查询空闲房间。</summary>
    [HttpGet("free")]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetFree(CancellationToken cancellationToken)
        => Ok(await _rooms.GetFreeRoomsAsync(cancellationToken));

/// <summary>查询已有楼层。</summary>
    [HttpGet("floors")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetFloors(CancellationToken cancellationToken)
        => Ok(await _rooms.GetFloorsAsync(cancellationToken));

/// <summary>获取房间详情。</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> Get(int id, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetRoomAsync(id, cancellationToken);
        return room is null ? NotFound() : Ok(room);
    }

/// <summary>创建房间。</summary>
    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(
        [FromBody] RoomInput input,
        CancellationToken cancellationToken)
        => Ok(await _rooms.CreateRoomAsync(input, cancellationToken));

/// <summary>修改房间。</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RoomInput input,
        CancellationToken cancellationToken)
    {
        await _rooms.UpdateRoomAsync(id, input, cancellationToken);
        return NoContent();
    }

/// <summary>删除房间（存在历史单据时禁止删除）。</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _rooms.DeleteRoomAsync(id, cancellationToken);
        return NoContent();
    }

/// <summary>标记保洁完成，房间恢复空闲。</summary>
    [HttpPost("{id:int}/cleaning-complete")]
    public async Task<IActionResult> CleaningComplete(int id, CancellationToken cancellationToken)
    {
        await _rooms.MarkCleaningCompleteAsync(id, cancellationToken);
        return NoContent();
    }

/// <summary>将维修 / 停用房间恢复为空闲。</summary>
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await _rooms.RestoreRoomAsync(id, cancellationToken);
        return NoContent();
    }

/// <summary>设置维修或停用状态。</summary>
    [HttpPost("{id:int}/maintenance")]
    public async Task<IActionResult> Maintenance(
        int id,
        [FromBody] MaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        await _rooms.SetMaintenanceAsync(id, request.Status, request.Note, cancellationToken);
        return NoContent();
    }
}
