using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetAllAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<bool> HasPendingForRoomAsync(
        int roomId,
        int? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);

    void Add(Reservation reservation);
}
