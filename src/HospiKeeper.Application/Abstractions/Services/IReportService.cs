using HospiKeeper.Contracts;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>经营统计报表。</summary>
public interface IReportService
{
    /// <summary>报表概览：今日 / 月度营收、入住率、近 30 日趋势与房型统计。</summary>
    Task<ReportSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>指定区间的营收统计（含日环比与占比）。</summary>
    Task<RevenueRangeResult> GetRevenueRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
