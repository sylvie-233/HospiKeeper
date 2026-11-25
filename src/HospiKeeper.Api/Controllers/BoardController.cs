using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/board")]
public sealed class BoardController : ControllerBase
{
    private readonly IBookingService _booking;

    public BoardController(IBookingService booking) => _booking = booking;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardRoom>>> Get(CancellationToken cancellationToken)
        => Ok(await _booking.GetBoardAsync(cancellationToken));

    [HttpGet("counts")]
    public async Task<ActionResult<FrontDeskCounts>> GetCounts(CancellationToken cancellationToken)
        => Ok(await _booking.GetCountsAsync(cancellationToken));
}
