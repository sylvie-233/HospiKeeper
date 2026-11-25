using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;

namespace HospiKeeper.Api.Endpoints;

/// <summary>无需鉴权的进程与存储健康检查。</summary>
public static class HealthEndpoints
{
    /// <summary>映射 <c>GET /health</c>。</summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", (IClock clock) => Results.Ok(new HealthResponse(
                Status: "ok",
                Storage: "mysql",
                Time: clock.Now)))
            .WithTags("健康检查")
            .WithSummary("健康检查")
            .WithDescription("返回 API 与存储状态，无需鉴权。");

        return endpoints;
    }
}
