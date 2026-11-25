using HospiKeeper.Contracts;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HospiKeeper.Api.OpenApi;

/// <summary>补充 API 基本信息与 Bearer 安全方案，供 Scalar 与外部工具使用。</summary>
public sealed class HospiKeeperOpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string SecuritySchemeName = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();
        document.Info.Title = "HospiKeeper API";
        document.Info.Version = "v1";
        document.Info.Description =
            "云栖民宿管理系统 API。除登录、健康检查与文档页外，所有接口都需要先调用 " +
            "`POST /api/auth/login` 获取 Bearer Token，并在请求头携带 `Authorization: Bearer <token>`。";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SecuritySchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "Opaque",
            Description = "登录接口返回的不透明访问令牌（内存会话，默认 12 小时有效）。"
        };

        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas[nameof(ApiErrorResponse)] = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Description = "统一错误响应。",
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["message"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "面向用户的错误信息。"
                }
            },
            Required = new HashSet<string> { "message" }
        };

        var schemeReference = new OpenApiSecuritySchemeReference(SecuritySchemeName, document);
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [schemeReference] = []
        });

        return Task.CompletedTask;
    }
}
