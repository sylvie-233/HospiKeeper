namespace HospiKeeper.Contracts;

/// <summary>操作日志的对外视图。</summary>
public sealed record OperationLogDto(
    long Id,
    int? UserId,
    string UserName,
    string Action,
    string? Target,
    string? Detail,
    string? BizNo,
    DateTime CreatedAt);
