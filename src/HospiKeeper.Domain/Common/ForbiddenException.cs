namespace HospiKeeper.Domain.Common;

/// <summary>
/// 当前操作员无权执行该动作。API 层据此返回 403，而不是把它当成普通参数错误。
/// </summary>
public sealed class ForbiddenException : DomainException
{
    /// <summary>使用可直接展示给操作员的中文消息创建异常。</summary>
    public ForbiddenException(string message) : base(message)
    {
    }
}
