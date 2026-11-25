using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>退房结算账单查询与变更。</summary>
public interface ICheckoutBillRepository
{
    /// <summary>查询全部账单，按退房时间倒序。</summary>
    Task<IReadOnlyList<CheckoutBill>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>查询指定退房时间区间的账单。</summary>
    Task<IReadOnlyList<CheckoutBill>> GetByCheckoutRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>按条件查询账单；非管理员只能查询本人经办的数据。</summary>
    Task<IReadOnlyList<CheckoutBill>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询账单。</summary>
    Task<CheckoutBill?> GetByIdAsync(int billId, CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(CheckoutBill bill);
}
