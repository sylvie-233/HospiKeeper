using HospiKeeper.Application.Abstractions.Repositories;

namespace HospiKeeper.Application.Abstractions;

/// <summary>
/// 应用层工作单元：聚合仓储并在一次保存中提交全部业务变更。
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }

    IRoomRepository Rooms { get; }

    IRoomTypeRepository RoomTypes { get; }

    IReservationRepository Reservations { get; }

    ICheckInRepository CheckIns { get; }

    ICheckoutBillRepository Bills { get; }

    IOperationLogRepository Logs { get; }

    IDocumentNumberRepository DocumentNumbers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
