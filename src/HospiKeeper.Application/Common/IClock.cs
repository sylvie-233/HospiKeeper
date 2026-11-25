namespace HospiKeeper.Application.Common;

/// <summary>可替换的时钟，业务层通过它获取当前时间以便测试。</summary>
public interface IClock
{
    /// <summary>当前本地时间。</summary>
    DateTime Now { get; }
}

/// <summary>默认实现，直接返回 <see cref="DateTime.Now"/>。</summary>
public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
