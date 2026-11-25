using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/checkouts")]
public sealed class CheckoutsController : ControllerBase
{
    private readonly IBookingService _booking;

    public CheckoutsController(IBookingService booking) => _booking = booking;

    [HttpPost]
    public async Task<ActionResult<CheckoutBill>> Create(
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken)
        => Ok(await _booking.CheckoutAsync(request, cancellationToken));
}
