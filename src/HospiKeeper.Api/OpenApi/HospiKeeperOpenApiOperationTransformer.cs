using HospiKeeper.Contracts;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HospiKeeper.Api.OpenApi;

/// <summary>
/// 统一补充错误响应；并对登录 / 健康检查等公开接口取消文档级 Bearer 要求。
/// </summary>
public sealed class HospiKeeperOpenApiOperationTransformer : IOpenApiOperationTransformer
{
    private static readonly string[] PublicPaths =
    [
        "api/auth/login"
    ];

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var relativePath = context.Description.RelativePath?.TrimStart('/') ?? string.Empty;
        if (relativePath.Equals("health", StringComparison.OrdinalIgnoreCase))
        {
            operation.Security = [];
            return Task.CompletedTask;
        }

        if (PublicPaths.Contains(relativePath, StringComparer.OrdinalIgnoreCase))
        {
            operation.Security = [];
            AddResponse(operation, context, "400", "请求参数或业务校验失败。");
            AddResponse(operation, context, "401", "账号或密码错误。");
            return Task.CompletedTask;
        }

        AddResponse(operation, context, "400", "请求参数或业务校验失败。");
        AddResponse(operation, context, "401", "缺少令牌、令牌过期或账号不可用。");
        AddResponse(operation, context, "403", "当前账号无权执行该操作。");
        AddResponse(operation, context, "500", "服务器内部错误。");

        return Task.CompletedTask;
    }

    private static void AddResponse(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        string statusCode,
        string description)
    {
        operation.Responses ??= new OpenApiResponses();
        if (operation.Responses.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Schema = new OpenApiSchemaReference(
                        nameof(ApiErrorResponse),
                        context.Document)
                }
            }
        };
    }
}
