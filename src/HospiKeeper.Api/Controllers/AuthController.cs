using HospiKeeper.Api.Security;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ApiTokenStore _tokens;

    public AuthController(IAuthService auth, ApiTokenStore tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(request.Username, request.Password, cancellationToken);
        if (!result.Success || result.User is null)
        {
            return Unauthorized(new ApiErrorResponse(result.Error ?? "登录失败。"));
        }

        var (token, expiresAt) = _tokens.Issue(result.User.Id);
        return Ok(new LoginResponse(token, expiresAt, result.User.ToDto()));
    }

    [HttpGet("me")]
    public ActionResult<UserDto> Me([FromServices] SessionContext session)
        => Ok(session.RequireUser().ToDto());

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromServices] SessionContext session,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ApiToken"] is string token)
        {
            _tokens.Revoke(token);
        }

        await _auth.LogoutAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _auth.ChangeOwnPasswordAsync(
            request.OldPassword,
            request.NewPassword,
            request.ConfirmPassword,
            cancellationToken);
        return NoContent();
    }
}
