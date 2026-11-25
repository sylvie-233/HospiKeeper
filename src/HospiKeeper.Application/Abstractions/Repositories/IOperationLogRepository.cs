using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>操作日志查询与写入。</summary>
public interface IOperationLogRepository
{
    /// <summary>按动作、关键字查询日志；非管理员只能查看本人日志。</summary>
    Task<IReadOnlyList<OperationLog>> QueryAsync(
        string? action,
        string? keyword,
        int? userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(OperationLog log);
}
