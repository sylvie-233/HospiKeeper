using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IOperationLogRepository
{
    Task<IReadOnlyList<OperationLog>> QueryAsync(
        string? action,
        string? keyword,
        int? userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    void Add(OperationLog log);
}
