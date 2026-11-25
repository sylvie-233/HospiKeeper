using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Sylvie233.UI.Controls;

namespace HospiKeeper.Desktop.Services;

/// <summary>一条轻提示。</summary>
public sealed partial class ToastItem : ObservableObject
{
    public ToastItem(string message, AntToastKind kind)
    {
        Message = message;
        Kind = kind;
    }

    public string Message { get; }
    public AntToastKind Kind { get; }

    /// <summary>由 ToastService 在移除前设置为 true，用于播放淡出动画。</summary>
    [ObservableProperty]
    private bool _isLeaving;
}

/// <summary>全局轻提示队列，线程安全地切换到 UI 线程。</summary>
public sealed class ToastService : ObservableObject
{
    public ObservableCollection<ToastItem> Items { get; } = new();

    public void Success(string message) => Show(message, AntToastKind.Success);

    public void Error(string message) => Show(message, AntToastKind.Error);

    public void Warning(string message) => Show(message, AntToastKind.Warning);

    public void Info(string message) => Show(message, AntToastKind.Info);

    private void Show(string message, AntToastKind kind)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        // 允许从后台线程调用；统一回到 UI 线程更新集合。
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Show(message, kind));
            return;
        }

        // 避免错误连续堆叠遮挡界面，最多保留 4 条。
        while (Items.Count >= 4)
        {
            Items.RemoveAt(0);
        }

        var item = new ToastItem(message, kind);
        Items.Add(item);
        _ = RemoveLaterAsync(item);
    }

    private async Task RemoveLaterAsync(ToastItem item)
    {
        await Task.Delay(kindDelay(item.Kind));
        await Dispatcher.UIThread.InvokeAsync(() => item.IsLeaving = true);
        await Task.Delay(180);
        await Dispatcher.UIThread.InvokeAsync(() => Items.Remove(item));
    }

    private static int kindDelay(AntToastKind kind) => kind == AntToastKind.Error ? 4200 : 2600;
}
