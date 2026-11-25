using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth) => _auth = auth;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _auth.GetUsersAsync(cancellationToken);
        return Ok(users.Select(u => u.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] AccountInput input,
        CancellationToken cancellationToken)
    {
        var user = await _auth.CreateUserAsync(input, cancellationToken);
        return Ok(user.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AccountInput input,
        CancellationToken cancellationToken)
    {
        await _auth.UpdateUserAsync(id, input, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] AccountStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _auth.SetUserStatusAsync(id, request.Status, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _auth.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }
}
