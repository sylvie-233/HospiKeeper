using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet("snapshot")]
    public async Task<ActionResult<ReportSnapshot>> GetSnapshot(CancellationToken cancellationToken)
        => Ok(await _reports.GetSnapshotAsync(cancellationToken));

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueRangeResult>> GetRevenue(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        CancellationToken cancellationToken)
        => Ok(await _reports.GetRevenueRangeAsync(start, end, cancellationToken));
}
