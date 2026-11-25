using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Common;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Services;

/// <summary>操作日志写入实现，复用当前工作单元保证原子性。</summary>
public sealed class OperationLogger : IOperationLogger
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;
    private readonly IClock _clock;

    public OperationLogger(IUnitOfWork unitOfWork, SessionContext session, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _session = session;
        _clock = clock;
    }

    public void Append(string action, string? target, string? detail, string? bizNo = null)
    {
        var user = _session.CurrentUser;
        _unitOfWork.Logs.Add(new OperationLog
        {
            UserId = user?.Id,
            UserName = user?.DisplayName ?? "系统",
            Action = action,
            Target = target,
            Detail = detail,
            BizNo = bizNo,
            CreatedAt = _clock.Now
        });
    }

    public async Task LogAsync(
        string action,
        string? target,
        string? detail,
        string? bizNo = null,
        CancellationToken cancellationToken = default)
    {
        Append(action, target, detail, bizNo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
