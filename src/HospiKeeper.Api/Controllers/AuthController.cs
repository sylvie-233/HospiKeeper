using HospiKeeper.Api.Security;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HospiKeeper.Api.Controllers;

/// <summary>登录、当前用户、退出与修改密码。</summary>
[ApiController]
[Route("api/auth")]
[Tags("认证")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ApiTokenStore _tokens;

    public AuthController(IAuthService auth, ApiTokenStore tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

/// <summary>登录并签发 Bearer 令牌。</summary>
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

/// <summary>获取当前登录用户。</summary>
    [HttpGet("me")]
    public ActionResult<UserDto> Me([FromServices] SessionContext session)
        => Ok(session.RequireUser().ToDto());

/// <summary>退出登录并吊销当前令牌。</summary>
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

/// <summary>修改当前账号密码。</summary>
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
