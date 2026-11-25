using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>房型管理（仅管理员）。</summary>
[ApiController]
[Route("api/room-types")]
[Tags("房型管理")]
public sealed class RoomTypesController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomTypesController(IRoomService rooms) => _rooms = rooms;

/// <summary>查询全部房型。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomType>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _rooms.GetRoomTypesAsync(cancellationToken));

/// <summary>创建房型。</summary>
    [HttpPost]
    public async Task<ActionResult<RoomType>> Create(
        [FromBody] RoomTypeInput input,
        CancellationToken cancellationToken)
        => Ok(await _rooms.CreateRoomTypeAsync(input, cancellationToken));

/// <summary>修改房型（改名会同步房间冗余名称）。</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RoomTypeInput input,
        CancellationToken cancellationToken)
    {
        await _rooms.UpdateRoomTypeAsync(id, input, cancellationToken);
        return NoContent();
    }

/// <summary>删除房型。</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _rooms.DeleteRoomTypeAsync(id, cancellationToken);
        return NoContent();
    }
}
