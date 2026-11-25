namespace HospiKeeper.Domain.Common;

/// <summary>
/// 业务规则被违反时抛出的异常。UI 层捕获后直接展示 Message。
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
