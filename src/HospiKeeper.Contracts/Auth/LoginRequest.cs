using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>登录请求。字段约束会在模型绑定阶段先行校验。</summary>
public sealed record LoginRequest(
    [param: Required(ErrorMessage = "请输入账号。")]
    [param: StringLength(32, ErrorMessage = "账号不能超过 32 个字符。")]
    string Username,
    [param: Required(ErrorMessage = "请输入密码。")]
    [param: StringLength(100, ErrorMessage = "密码不能超过 100 个字符。")]
    string Password);
