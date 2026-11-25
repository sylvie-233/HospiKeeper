using HospiKeeper.Application.Abstractions.Repositories;

namespace HospiKeeper.Application.Abstractions;

/// <summary>
/// 应用层工作单元：聚合仓储并在一次保存中提交全部业务变更。
/// </summary>
public interface IUnitOfWork
{
    /// <summary>操作员账号仓储。</summary>
    IUserRepository Users { get; }

    /// <summary>房间仓储。</summary>
    IRoomRepository Rooms { get; }

    /// <summary>房型仓储。</summary>
    IRoomTypeRepository RoomTypes { get; }

    /// <summary>预定仓储。</summary>
    IReservationRepository Reservations { get; }

    /// <summary>入住记录仓储。</summary>
    ICheckInRepository CheckIns { get; }

    /// <summary>结算账单仓储。</summary>
    ICheckoutBillRepository Bills { get; }

    /// <summary>操作日志仓储。</summary>
    IOperationLogRepository Logs { get; }

    /// <summary>业务单号序号仓储。</summary>
    IDocumentNumberRepository DocumentNumbers { get; }

    /// <summary>提交当前工作单元内的全部变更，返回受影响行数。</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
