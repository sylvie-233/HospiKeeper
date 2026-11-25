using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>桌面端系统配置（仅管理员）。</summary>
[ApiController]
[Route("api/settings")]
[Tags("系统配置")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISiteSettingsService _settings;

    public SettingsController(ISiteSettingsService settings) => _settings = settings;

    /// <summary>读取系统配置。</summary>
    [HttpGet]
    public async Task<ActionResult<SiteSettings>> Get(CancellationToken cancellationToken)
        => Ok(await _settings.GetForAdminAsync(cancellationToken));

    /// <summary>保存系统配置到 Redis。</summary>
    [HttpPut]
    public async Task<ActionResult<SiteSettings>> Update(
        [FromBody] SiteSettings settings,
        CancellationToken cancellationToken)
        => Ok(await _settings.UpdateAsync(settings, cancellationToken));
}
