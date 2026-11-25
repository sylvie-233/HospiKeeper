using System.Text.Json;
using HospiKeeper.Api.Security;
using HospiKeeper.Application.Common;
using HospiKeeper.Application.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Api.Middleware;

/// <summary>
/// Bearer 令牌鉴权：校验令牌 → 还原数据库中的用户 → 写入本次请求的 SessionContext。
/// 登录 / 健康检查 / Swagger 无需令牌。
/// </summary>
public sealed class ApiAuthMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;

    public ApiAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ApiTokenStore tokens,
        IAuthService auth,
        SessionContext session,
        ILogger<ApiAuthMiddleware> logger)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsPublic(path) || HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var header = context.Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("未携带访问令牌 {Method} {Path}", context.Request.Method, context.Request.Path);
            await UnauthorizedAsync(context, "缺少登录令牌，请重新登录。");
            return;
        }

        var token = header["Bearer ".Length..].Trim();
        if (!tokens.TryValidate(token, out var userId))
        {
            logger.LogWarning("访问令牌无效或已过期 {Method} {Path}", context.Request.Method, context.Request.Path);
            await UnauthorizedAsync(context, "登录已过期，请重新登录。");
            return;
        }

        var user = await auth.FindUserByIdAsync(userId, context.RequestAborted);
        if (user is null || user.Status == AccountStatus.Disabled)
        {
            tokens.Revoke(token);
            logger.LogWarning("账号不可用（不存在或已停用）UserId={UserId}", userId);
            await UnauthorizedAsync(context, "账号不存在或已停用。");
            return;
        }

        session.SignIn(user);
        context.Items["ApiToken"] = token;
        await _next(context);
    }

    private static bool IsPublic(string path)
        => path.Equals("/", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase);

    private static async Task UnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse(message), JsonOptions));
    }
}
