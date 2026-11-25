using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>操作日志查询。</summary>
[ApiController]
[Route("api/logs")]
[Tags("操作日志")]
public sealed class LogsController : ControllerBase
{
    private readonly IRecordService _records;

    public LogsController(IRecordService records) => _records = records;

/// <summary>按动作与关键字查询操作日志（前台仅返回本人经办）。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OperationLogDto>>> Get(
        [FromQuery] string? action,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryLogsAsync(action, keyword, cancellationToken));
}
