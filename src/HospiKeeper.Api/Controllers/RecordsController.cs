using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/records")]
public sealed class RecordsController : ControllerBase
{
    private readonly IRecordService _records;

    public RecordsController(IRecordService records) => _records = records;

    [HttpGet("checkins")]
    public async Task<ActionResult<IReadOnlyList<CheckInRecord>>> GetCheckIns(
        [FromQuery] string? roomNo,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryCheckInsAsync(
            new RecordQuery(roomNo, keyword, from, to), cancellationToken));

    [HttpGet("checkins/{id:int}")]
    public async Task<ActionResult<CheckInRecord>> GetCheckIn(
        int id,
        CancellationToken cancellationToken)
    {
        var record = await _records.GetCheckInAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpGet("bills")]
    public async Task<ActionResult<IReadOnlyList<CheckoutBill>>> GetBills(
        [FromQuery] string? roomNo,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryBillsAsync(
            new RecordQuery(roomNo, keyword, from, to), cancellationToken));

    [HttpGet("bills/{id:int}")]
    public async Task<ActionResult<CheckoutBill>> GetBill(int id, CancellationToken cancellationToken)
    {
        var bill = await _records.GetBillAsync(id, cancellationToken);
        return bill is null ? NotFound() : Ok(bill);
    }
}
