using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/room-types")]
public sealed class RoomTypesController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomTypesController(IRoomService rooms) => _rooms = rooms;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomType>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _rooms.GetRoomTypesAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RoomType>> Create(
        [FromBody] RoomTypeInput input,
        CancellationToken cancellationToken)
        => Ok(await _rooms.CreateRoomTypeAsync(input, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RoomTypeInput input,
        CancellationToken cancellationToken)
    {
        await _rooms.UpdateRoomTypeAsync(id, input, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _rooms.DeleteRoomTypeAsync(id, cancellationToken);
        return NoContent();
    }
}
