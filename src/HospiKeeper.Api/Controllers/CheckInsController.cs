using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>办理入住、追加消费与退房报价。</summary>
[ApiController]
[Route("api/checkins")]
[Tags("入住")]
public sealed class CheckInsController : ControllerBase
{
    private readonly IBookingService _booking;

    public CheckInsController(IBookingService booking) => _booking = booking;

/// <summary>办理入住。</summary>
    [HttpPost]
    public async Task<ActionResult<CheckInRecord>> Create(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CheckInAsync(request, cancellationToken));

/// <summary>为在住记录追加消费。</summary>
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

/// <summary>按实际退房时间试算账单。</summary>
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
