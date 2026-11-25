using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>操作员账号管理（仅管理员）。</summary>
[ApiController]
[Route("api/users")]
[Tags("账号管理")]
public sealed class UsersController : ControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth) => _auth = auth;

/// <summary>查询全部账号。</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _auth.GetUsersAsync(cancellationToken);
        return Ok(users.Select(u => u.ToDto()).ToList());
    }

/// <summary>创建账号。</summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] AccountInput input,
        CancellationToken cancellationToken)
    {
        var user = await _auth.CreateUserAsync(input, cancellationToken);
        return Ok(user.ToDto());
    }

/// <summary>修改账号。</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AccountInput input,
        CancellationToken cancellationToken)
    {
        await _auth.UpdateUserAsync(id, input, cancellationToken);
        return NoContent();
    }

/// <summary>启用或停用账号。</summary>
    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] AccountStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _auth.SetUserStatusAsync(id, request.Status, cancellationToken);
        return NoContent();
    }

/// <summary>删除账号。</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _auth.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }
}
