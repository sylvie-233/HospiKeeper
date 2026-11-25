using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface ICheckInRepository
{
    Task<IReadOnlyList<CheckInRecord>> GetLivingAsync(CancellationToken cancellationToken = default);

    Task<CheckInRecord?> GetByIdAsync(
        int checkInId,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckInRecord>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task<int> CountLivingAsync(CancellationToken cancellationToken = default);

    Task<int> CountByCheckInDateAsync(DateTime date, CancellationToken cancellationToken = default);

    void Add(CheckInRecord record);
}
