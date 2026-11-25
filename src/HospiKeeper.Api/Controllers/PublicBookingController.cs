using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>游客在线预订公开接口（免登录，供 Blazor Web 门户调用）。</summary>
[ApiController]
[Route("api/public")]
[Tags("游客预订")]
public sealed class PublicBookingController : ControllerBase
{
    private readonly IGuestBookingService _guest;

    public PublicBookingController(IGuestBookingService guest) => _guest = guest;

    /// <summary>首页统计数据。</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<HotelSummaryDto>> Summary(CancellationToken cancellationToken)
        => Ok(await _guest.GetSummaryAsync(cancellationToken));

    /// <summary>查询日期段内各房型的实时可订量。</summary>
    [HttpGet("room-types")]
    public async Task<ActionResult<GuestAvailabilityResult>> RoomTypes(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        CancellationToken cancellationToken)
        => Ok(await _guest.GetAvailabilityAsync(checkIn, checkOut, cancellationToken));

    /// <summary>按手机号查询游客预订记录。</summary>
    [HttpGet("reservations")]
    public async Task<ActionResult<IReadOnlyList<GuestReservationDto>>> MyReservations(
        [FromQuery] string phone,
        CancellationToken cancellationToken)
        => Ok(await _guest.GetByPhoneAsync(phone, cancellationToken));

    /// <summary>提交在线预订（只锁房型日期段库存，不指定房间）。</summary>
    [HttpPost("reservations")]
    public async Task<ActionResult<GuestReservationDto>> Create(
        [FromBody] GuestBookingRequest request,
        CancellationToken cancellationToken)
        => Ok(await _guest.CreateAsync(request, cancellationToken));

    /// <summary>游客取消待到店预订（手机号校验）。</summary>
    [HttpPost("reservations/{resNo}/cancel")]
    public async Task<IActionResult> Cancel(
        string resNo,
        [FromBody] CancelGuestBookingRequest request,
        CancellationToken cancellationToken)
    {
        await _guest.CancelAsync(resNo, request.Phone, cancellationToken);
        return NoContent();
    }
}
