using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>经营统计报表。</summary>
[ApiController]
[Route("api/reports")]
[Tags("统计报表")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

/// <summary>获取报表概览指标与近 7 日趋势。</summary>
    [HttpGet("snapshot")]
    public async Task<ActionResult<ReportSnapshot>> GetSnapshot(CancellationToken cancellationToken)
        => Ok(await _reports.GetSnapshotAsync(cancellationToken));

/// <summary>查询指定区间的营收统计。</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueRangeResult>> GetRevenue(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        CancellationToken cancellationToken)
        => Ok(await _reports.GetRevenueRangeAsync(start, end, cancellationToken));
}
