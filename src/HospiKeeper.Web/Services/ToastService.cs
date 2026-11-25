namespace HospiKeeper.Web.Services;

/// <summary>轻提示类型。</summary>
public enum ToastKind
{
    Success,
    Error,
    Info
}

/// <summary>一条轻提示。</summary>
public sealed record ToastMessage(Guid Id, string Text, ToastKind Kind);

/// <summary>全局轻提示队列，供各页面与弹窗注入使用。</summary>
public sealed class ToastService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(2.4);
    private readonly List<ToastMessage> _messages = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<ToastMessage> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToList();
            }
        }
    }

    public event Action? Changed;

    public void Success(string message) => Show(message, ToastKind.Success);

    public void Error(string message) => Show(message, ToastKind.Error);

    public void Info(string message) => Show(message, ToastKind.Info);

    public void Show(string message, ToastKind kind)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var item = new ToastMessage(Guid.NewGuid(), message, kind);
        lock (_gate)
        {
            _messages.Add(item);
        }

        Changed?.Invoke();
        _ = RemoveLaterAsync(item.Id);
    }

    private async Task RemoveLaterAsync(Guid id)
    {
        await Task.Delay(Lifetime);
        lock (_gate)
        {
            _messages.RemoveAll(x => x.Id == id);
        }

        Changed?.Invoke();
    }
}
