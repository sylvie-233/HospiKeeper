namespace HospiKeeper.Contracts;

/// <summary>统一错误响应，客户端只需读取 <see cref="Message"/> 即可提示用户。</summary>
/// <param name="Message">可直接展示给操作员的错误信息。</param>
public sealed record ApiErrorResponse(string Message);
