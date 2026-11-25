using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>入住记录、账单与操作日志查询。前台只能看到本人经办的数据。</summary>
public interface IRecordService
{
    /// <summary>查询入住记录。</summary>
    Task<IReadOnlyList<CheckInRecordDto>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>查询结算账单。</summary>
    Task<IReadOnlyList<CheckoutBillDto>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>查询操作日志。</summary>
    Task<IReadOnlyList<OperationLogDto>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    /// <summary>获取入住记录详情。</summary>
    Task<CheckInRecordDto?> GetCheckInAsync(int checkInId, CancellationToken cancellationToken = default);

    /// <summary>获取账单详情。</summary>
    Task<CheckoutBillDto?> GetBillAsync(int billId, CancellationToken cancellationToken = default);
}
