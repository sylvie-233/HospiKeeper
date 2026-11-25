using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>房态看板与前台统计。</summary>
[ApiController]
[Route("api/board")]
[Tags("房态看板")]
public sealed class BoardController : ControllerBase
{
    private readonly IBookingService _booking;

    public BoardController(IBookingService booking) => _booking = booking;

/// <summary>获取房间看板列表。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardRoom>>> Get(CancellationToken cancellationToken)
        => Ok(await _booking.GetBoardAsync(cancellationToken));

/// <summary>获取前台房态汇总计数。</summary>
    [HttpGet("counts")]
    public async Task<ActionResult<FrontDeskCounts>> GetCounts(CancellationToken cancellationToken)
        => Ok(await _booking.GetCountsAsync(cancellationToken));
}
