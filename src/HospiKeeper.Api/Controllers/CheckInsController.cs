using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/checkins")]
public sealed class CheckInsController : ControllerBase
{
    private readonly IBookingService _booking;

    public CheckInsController(IBookingService booking) => _booking = booking;

    [HttpPost]
    public async Task<ActionResult<CheckInRecord>> Create(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CheckInAsync(request, cancellationToken));

    [HttpPost("{id:int}/extras")]
    public async Task<IActionResult> AddExtra(
        int id,
        [FromBody] ExtraChargeInput input,
        CancellationToken cancellationToken)
    {
        await _booking.AddExtraChargeAsync(
            new ExtraChargeRequest(id, input.Item, input.Amount),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/quote")]
    public async Task<ActionResult<BillingResult>> Quote(
        int id,
        [FromBody] CheckoutQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var stay = await _booking.GetStayAsync(id, cancellationToken);
        if (stay is null)
        {
            return NotFound();
        }

        return Ok(_booking.QuoteCheckout(stay.CheckIn, request.ActualOutTime, request.Damage, request.Mode));
    }
}
