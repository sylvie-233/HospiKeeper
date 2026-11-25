using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IBookingService _booking;

    public ReservationsController(IBookingService booking) => _booking = booking;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Reservation>>> Get(
        [FromQuery] ReservationStatus? status,
        CancellationToken cancellationToken)
        => Ok(await _booking.GetReservationsAsync(status, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reservation>> Get(int id, CancellationToken cancellationToken)
    {
        var reservation = await _booking.GetReservationAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<Reservation>> Create(
        [FromBody] ReservationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CreateReservationAsync(request, cancellationToken));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _booking.CancelReservationAsync(id, cancellationToken);
        return NoContent();
    }
}
