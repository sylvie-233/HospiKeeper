namespace HospiKeeper.App.Services;

/// <summary>轻提示类型。</summary>
public enum AppToastKind
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>一条 App 轻提示。</summary>
public sealed record AppToastMessage(string Text, AppToastKind Kind);

/// <summary>全局轻提示事件，由页面内的 ToastHost 负责展示。</summary>
public sealed class ToastService
{
    public event Action<AppToastMessage>? Shown;

    public void Success(string message) => Show(message, AppToastKind.Success);

    public void Error(string message) => Show(message, AppToastKind.Error);

    public void Warning(string message) => Show(message, AppToastKind.Warning);

    public void Info(string message) => Show(message, AppToastKind.Info);

    private void Show(string message, AppToastKind kind)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            Shown?.Invoke(new AppToastMessage(message, kind));
        }
    }
}
