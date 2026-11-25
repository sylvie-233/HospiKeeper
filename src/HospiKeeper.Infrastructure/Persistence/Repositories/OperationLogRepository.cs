using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

/// <summary>操作日志仓储：按动作 / 关键字查询并写入。</summary>
internal sealed class OperationLogRepository : IOperationLogRepository
{
    private readonly HotelDbContext _db;

    public OperationLogRepository(HotelDbContext db) => _db = db;

    public async Task<IReadOnlyList<OperationLog>> QueryAsync(
        string? action,
        string? keyword,
        int? userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var kw = keyword?.Trim().ToLowerInvariant();
        var source = _db.OperationLogs.AsNoTracking().AsQueryable();

        if (!isAdmin && userId is not null)
        {
            source = source.Where(l => l.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(action) && action != "全部")
        {
            source = source.Where(l => l.Action == action);
        }

        if (!string.IsNullOrEmpty(kw))
        {
            source = source.Where(l =>
                (l.UserName != null && l.UserName.ToLower().Contains(kw))
                || l.Action.ToLower().Contains(kw)
                || (l.Target != null && l.Target.ToLower().Contains(kw))
                || (l.Detail != null && l.Detail.ToLower().Contains(kw))
                || (l.BizNo != null && l.BizNo.ToLower().Contains(kw)));
        }

        return await source
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(OperationLog log) => _db.OperationLogs.Add(log);
}
