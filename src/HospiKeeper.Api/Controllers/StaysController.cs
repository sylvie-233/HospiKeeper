using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/stays")]
public sealed class StaysController : ControllerBase
{
    private readonly IBookingService _booking;

    public StaysController(IBookingService booking) => _booking = booking;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StayInfo>>> Get(
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
        => Ok(await _booking.GetLivingStaysAsync(keyword, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StayInfo>> Get(int id, CancellationToken cancellationToken)
    {
        var stay = await _booking.GetStayAsync(id, cancellationToken);
        return stay is null ? NotFound() : Ok(stay);
    }
}
