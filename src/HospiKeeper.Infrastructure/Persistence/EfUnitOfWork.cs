using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Infrastructure.Persistence.Repositories;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>基于 EF Core 的工作单元：复用同一个 DbContext，一次 SaveChanges 提交全部变更。</summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly HotelDbContext _db;

    public EfUnitOfWork(HotelDbContext db)
    {
        _db = db;
        Users = new UserRepository(db);
        Rooms = new RoomRepository(db);
        RoomTypes = new RoomTypeRepository(db);
        Reservations = new ReservationRepository(db);
        CheckIns = new CheckInRepository(db);
        Bills = new CheckoutBillRepository(db);
        Logs = new OperationLogRepository(db);
        DocumentNumbers = new DocumentNumberRepository(db);
    }

    public IUserRepository Users { get; }

    public IRoomRepository Rooms { get; }

    public IRoomTypeRepository RoomTypes { get; }

    public IReservationRepository Reservations { get; }

    public ICheckInRepository CheckIns { get; }

    public ICheckoutBillRepository Bills { get; }

    public IOperationLogRepository Logs { get; }

    public IDocumentNumberRepository DocumentNumbers { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
