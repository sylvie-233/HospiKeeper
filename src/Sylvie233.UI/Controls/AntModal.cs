using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Sylvie233.UI.Controls;

/// <summary>主窗口内的模态弹窗容器。</summary>
public class AntModal : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntModal, string?>(nameof(Title));

    public static readonly StyledProperty<double> ModalWidthProperty =
        AvaloniaProperty.Register<AntModal, double>(nameof(ModalWidth), 520d);

    public static readonly StyledProperty<System.Windows.Input.ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<AntModal, System.Windows.Input.ICommand?>(nameof(CloseCommand));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public AntModal()
    {
        Opacity = 0;
        Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(160)
            }
        };
    }

    public double ModalWidth
    {
        get => GetValue(ModalWidthProperty);
        set => SetValue(ModalWidthProperty, value);
    }

    public System.Windows.Input.ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Dispatcher.UIThread.Post(() => SetCurrentValue(OpacityProperty, 1d), DispatcherPriority.Loaded);
    }
}
