using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomsController(IRoomService rooms) => _rooms = rooms;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Room>>> GetAll(
        [FromQuery] int? floor,
        [FromQuery] int? typeId,
        [FromQuery] RoomStatus? status,
        CancellationToken cancellationToken)
        => Ok(await _rooms.GetRoomsAsync(floor, typeId, status, cancellationToken));

    [HttpGet("free")]
    public async Task<ActionResult<IReadOnlyList<Room>>> GetFree(CancellationToken cancellationToken)
        => Ok(await _rooms.GetFreeRoomsAsync(cancellationToken));

    [HttpGet("floors")]
    public async Task<ActionResult<IReadOnlyList<int>>> GetFloors(CancellationToken cancellationToken)
        => Ok(await _rooms.GetFloorsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Room>> Get(int id, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetRoomAsync(id, cancellationToken);
        return room is null ? NotFound() : Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<Room>> Create(
        [FromBody] RoomInput input,
        CancellationToken cancellationToken)
        => Ok(await _rooms.CreateRoomAsync(input, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RoomInput input,
        CancellationToken cancellationToken)
    {
        await _rooms.UpdateRoomAsync(id, input, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _rooms.DeleteRoomAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cleaning-complete")]
    public async Task<IActionResult> CleaningComplete(int id, CancellationToken cancellationToken)
    {
        await _rooms.MarkCleaningCompleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await _rooms.RestoreRoomAsync(id, cancellationToken);
        return NoContent();
    }

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
