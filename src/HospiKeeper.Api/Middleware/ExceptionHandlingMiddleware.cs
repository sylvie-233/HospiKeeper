using System.Text.Json;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;

namespace HospiKeeper.Api.Middleware;

/// <summary>把业务异常转换为统一的 <c>ApiErrorResponse</c>：403 / 400 / 500。</summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(
                "权限校验失败 {Method} {Path} -> 403: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await WriteAsync(context, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                "业务校验失败 {Method} {Path} -> 400: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteAsync(context, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API exception for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteAsync(context, "服务器内部错误，请稍后重试。");
        }
    }

    private static async Task WriteAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiErrorResponse(message), JsonOptions));
    }
}
