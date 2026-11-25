using HospiKeeper.Contracts;

namespace HospiKeeper.Application.Abstractions.Services;

/// <summary>
/// 登录用例的返回值。属于应用层内部结果，不直接作为 API 契约暴露，
/// 由控制器转换为 <c>LoginResponse</c> 或 <c>ApiErrorResponse</c>。
/// </summary>
/// <param name="Success">是否登录成功。</param>
/// <param name="Error">失败原因，成功时为 <c>null</c>。</param>
/// <param name="User">登录成功的用户视图。</param>
public sealed record LoginResult(bool Success, string? Error, UserDto? User)
{
    /// <summary>构造失败结果。</summary>
    public static LoginResult Fail(string message) => new(false, message, null);

    /// <summary>构造成功结果。</summary>
    public static LoginResult Ok(UserDto user) => new(true, null, user);
}
