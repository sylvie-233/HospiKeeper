namespace HospiKeeper.Contracts;

/// <summary>健康检查响应。</summary>
/// <param name="Status">服务状态，正常时为 <c>ok</c>。</param>
/// <param name="Storage">当前持久化类型，例如 <c>mysql</c>。</param>
/// <param name="Time">服务器当前时间。</param>
public sealed record HealthResponse(string Status, string Storage, DateTime Time);
