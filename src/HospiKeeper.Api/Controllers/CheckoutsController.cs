using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>退房结算。</summary>
[ApiController]
[Route("api/checkouts")]
[Tags("退房")]
public sealed class CheckoutsController : ControllerBase
{
    private readonly IBookingService _booking;

    public CheckoutsController(IBookingService booking) => _booking = booking;

/// <summary>办理退房并生成结算单。</summary>
    [HttpPost]
    public async Task<ActionResult<CheckoutBillDto>> Create(
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CheckoutAsync(request, cancellationToken));
}
