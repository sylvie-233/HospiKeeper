using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>在住查询。</summary>
[ApiController]
[Route("api/stays")]
[Tags("在住")]
public sealed class StaysController : ControllerBase
{
    private readonly IBookingService _booking;

    public StaysController(IBookingService booking) => _booking = booking;

/// <summary>查询在住列表。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StayInfo>>> Get(
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
        => Ok(await _booking.GetLivingStaysAsync(keyword, cancellationToken));

/// <summary>获取在住详情。</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StayInfo>> Get(int id, CancellationToken cancellationToken)
    {
        var stay = await _booking.GetStayAsync(id, cancellationToken);
        return stay is null ? NotFound() : Ok(stay);
    }
}
