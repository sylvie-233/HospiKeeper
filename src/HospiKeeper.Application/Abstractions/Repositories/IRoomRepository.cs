using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetFreeAsync(CancellationToken cancellationToken = default);

    Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetByTypeAsync(int roomTypeId, CancellationToken cancellationToken = default);

    Task<bool> RoomNoExistsAsync(string roomNo, int? excludeRoomId = null, CancellationToken cancellationToken = default);

    Task<bool> HasBusinessHistoryAsync(int roomId, CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(RoomStatus status, CancellationToken cancellationToken = default);

    void Add(Room room);

    void Remove(Room room);
}
