using System.Text.Json;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;

namespace HospiKeeper.Api.Middleware;

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
        catch (DomainException ex)
        {
            var forbidden = ex.Message.Contains("仅管理员", StringComparison.Ordinal)
                            || ex.Message.Contains("无权", StringComparison.Ordinal)
                            || ex.Message.Contains("不能停用当前", StringComparison.Ordinal);
            _logger.LogWarning("业务校验失败 {Method} {Path} -> {StatusCode}: {Message}",
                context.Request.Method, context.Request.Path,
                forbidden ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest,
                ex.Message);

            context.Response.StatusCode = forbidden ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest;
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
