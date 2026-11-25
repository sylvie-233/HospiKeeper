using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>入住与账单记录查询。</summary>
[ApiController]
[Route("api/records")]
[Tags("记录查询")]
public sealed class RecordsController : ControllerBase
{
    private readonly IRecordService _records;

    public RecordsController(IRecordService records) => _records = records;

/// <summary>查询入住记录。</summary>
    [HttpGet("checkins")]
    public async Task<ActionResult<IReadOnlyList<CheckInRecord>>> GetCheckIns(
        [FromQuery] string? roomNo,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryCheckInsAsync(
            new RecordQuery(roomNo, keyword, from, to), cancellationToken));

/// <summary>获取入住记录详情。</summary>
    [HttpGet("checkins/{id:int}")]
    public async Task<ActionResult<CheckInRecord>> GetCheckIn(
        int id,
        CancellationToken cancellationToken)
    {
        var record = await _records.GetCheckInAsync(id, cancellationToken);
        return record is null ? NotFound() : Ok(record);
    }

/// <summary>查询结算账单。</summary>
    [HttpGet("bills")]
    public async Task<ActionResult<IReadOnlyList<CheckoutBill>>> GetBills(
        [FromQuery] string? roomNo,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryBillsAsync(
            new RecordQuery(roomNo, keyword, from, to), cancellationToken));

/// <summary>获取账单详情。</summary>
    [HttpGet("bills/{id:int}")]
    public async Task<ActionResult<CheckoutBill>> GetBill(int id, CancellationToken cancellationToken)
    {
        var bill = await _records.GetBillAsync(id, cancellationToken);
        return bill is null ? NotFound() : Ok(bill);
    }
}
