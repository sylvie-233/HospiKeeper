namespace HospiKeeper.Application.Abstractions;

public interface IOperationLogger
{
    /// <summary>把日志加入当前工作单元，由调用方统一保存。</summary>
    void Append(string action, string? target, string? detail, string? bizNo = null);

    /// <summary>独立写入一条日志（用于登录 / 退出等无其他变更的场景）。</summary>
    Task LogAsync(
        string action,
        string? target,
        string? detail,
        string? bizNo = null,
        CancellationToken cancellationToken = default);
}
