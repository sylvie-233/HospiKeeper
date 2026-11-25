using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Mapping;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Services;

/// <summary>入住 / 账单 / 日志查询实现，含前台数据隔离。</summary>
public sealed class RecordService : IRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;

    public RecordService(IUnitOfWork unitOfWork, SessionContext session)
    {
        _unitOfWork = unitOfWork;
        _session = session;
    }

    public async Task<IReadOnlyList<CheckInRecordDto>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var records = await _unitOfWork.CheckIns.QueryAsync(
            query,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
        return records.Select(r => r.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<CheckoutBillDto>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var bills = await _unitOfWork.Bills.QueryAsync(
            query,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
        return bills.Select(b => b.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<OperationLogDto>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var logs = await _unitOfWork.Logs.QueryAsync(
            action,
            keyword,
            _session.IsAdmin ? null : user.Id,
            _session.IsAdmin,
            cancellationToken);
        return logs.Select(l => l.ToDto()).ToList();
    }

    public async Task<CheckInRecordDto?> GetCheckInAsync(
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
            throw new ForbiddenException("无权查看其他操作员经办的单据。");
        }

        return record.ToDto();
    }

    public async Task<CheckoutBillDto?> GetBillAsync(int billId, CancellationToken cancellationToken = default)
    {
        var user = _session.RequireUser();
        var bill = await _unitOfWork.Bills.GetByIdAsync(billId, cancellationToken);
        if (bill is null)
        {
            return null;
        }

        if (!_session.IsAdmin && bill.OpUserId != user.Id)
        {
            throw new ForbiddenException("无权查看其他操作员经办的账单。");
        }

        return bill.ToDto();
    }
}