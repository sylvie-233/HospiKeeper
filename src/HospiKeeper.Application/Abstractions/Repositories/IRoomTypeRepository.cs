using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IRoomTypeRepository
{
    Task<IReadOnlyList<RoomType>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RoomType?> GetByIdAsync(int roomTypeId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, int? excludeRoomTypeId = null, CancellationToken cancellationToken = default);

    Task<int> CountRoomsAsync(int roomTypeId, CancellationToken cancellationToken = default);

    void Add(RoomType roomType);

    void Remove(RoomType roomType);
}
