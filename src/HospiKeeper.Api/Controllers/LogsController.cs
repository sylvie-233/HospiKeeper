using HospiKeeper.Application.Services;
using HospiKeeper.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly IRecordService _records;

    public LogsController(IRecordService records) => _records = records;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OperationLog>>> Get(
        [FromQuery] string? action,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
        => Ok(await _records.QueryLogsAsync(action, keyword, cancellationToken));
}
