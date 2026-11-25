using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Services;

public interface IRecordService
{
    Task<IReadOnlyList<CheckInRecord>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckoutBill>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OperationLog>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default);

    Task<CheckInRecord?> GetCheckInAsync(int checkInId, CancellationToken cancellationToken = default);

    Task<CheckoutBill?> GetBillAsync(int billId, CancellationToken cancellationToken = default);
}

public sealed class RecordService : IRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;

    public RecordService(IUnitOfWork unitOfWork, SessionContext session)
    {
        _unitOfWork = unitOfWork;
        _session = session;
    }

    public Task<IReadOnlyList<CheckInRecord>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        return _unitOfWork.CheckIns.QueryAsync(
            query,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
    }

    public Task<IReadOnlyList<CheckoutBill>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        return _unitOfWork.Bills.QueryAsync(
            query,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
    }

    public Task<IReadOnlyList<OperationLog>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        return _unitOfWork.Logs.QueryAsync(
            action,
            keyword,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
    }

    public async Task<CheckInRecord?> GetCheckInAsync(
        int checkInId,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var record = await _unitOfWork.CheckIns.GetByIdAsync(checkInId, true, cancellationToken);
        if (record is null)
        {
            return null;
        }

        if (!_session.IsAdmin && record.OpUserId != user.Id)
        {
            throw new DomainException("无权查看其他操作员经办的单据。");
        }

        return record;
    }

    public async Task<CheckoutBill?> GetBillAsync(int billId, CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var bill = await _unitOfWork.Bills.GetByIdAsync(billId, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        if (!_session.IsAdmin && bill.OpUserId != user.Id)
        {
            throw new DomainException("无权查看其他操作员经办的账单。");
        }

        return bill;
    }
}
