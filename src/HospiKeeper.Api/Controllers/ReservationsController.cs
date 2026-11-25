using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>预定管理。</summary>
[ApiController]
[Route("api/reservations")]
[Tags("预定")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IBookingService _booking;

    public ReservationsController(IBookingService booking) => _booking = booking;

/// <summary>查询预定列表。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Reservation>>> Get(
        [FromQuery] ReservationStatus? status,
        CancellationToken cancellationToken)
        => Ok(await _booking.GetReservationsAsync(status, cancellationToken));

/// <summary>获取预定详情。</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reservation>> Get(int id, CancellationToken cancellationToken)
    {
        var reservation = await _booking.GetReservationAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

/// <summary>创建预定。</summary>
    [HttpPost]
    public async Task<ActionResult<Reservation>> Create(
        [FromBody] ReservationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CreateReservationAsync(request, cancellationToken));

/// <summary>取消预定。</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _booking.CancelReservationAsync(id, cancellationToken);
        return NoContent();
    }
}
