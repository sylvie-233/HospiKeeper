using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface ICheckoutBillRepository
{
    Task<IReadOnlyList<CheckoutBill>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckoutBill>> GetByCheckoutRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckoutBill>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<CheckoutBill?> GetByIdAsync(int billId, CancellationToken cancellationToken = default);

    void Add(CheckoutBill bill);
}
