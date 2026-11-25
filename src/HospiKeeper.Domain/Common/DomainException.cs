namespace HospiKeeper.Domain.Common;

/// <summary>
/// 业务规则被违反时抛出的异常。UI 层捕获后直接展示 Message。
/// </summary>
public class DomainException : Exception
{
    /// <summary>使用可直接展示给操作员的中文消息创建异常。</summary>
    public DomainException(string message) : base(message)
    {
    }
}
