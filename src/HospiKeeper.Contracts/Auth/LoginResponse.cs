namespace HospiKeeper.Contracts;

/// <summary>登录成功后的令牌与当前用户信息。</summary>
/// <param name="Token">Bearer 访问令牌。</param>
/// <param name="ExpiresAt">令牌过期时间。</param>
/// <param name="User">当前登录用户。</param>
public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);
