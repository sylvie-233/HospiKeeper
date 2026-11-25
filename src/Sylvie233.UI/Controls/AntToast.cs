using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Sylvie233.UI.Icons;

namespace Sylvie233.UI.Controls;

/// <summary>轻提示类型。</summary>
public enum AntToastKind
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>轻提示条，用于成功 / 失败 / 警告反馈。</summary>
public class AntToast : ContentControl
{
    public static readonly StyledProperty<AntToastKind> KindProperty =
        AvaloniaProperty.Register<AntToast, AntToastKind>(nameof(Kind));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AntToast, Geometry?>(nameof(Icon), AntIcons.Check);

    public static readonly StyledProperty<bool> IsLeavingProperty =
        AvaloniaProperty.Register<AntToast, bool>(nameof(IsLeaving));

    static AntToast()
    {
        KindProperty.Changed.AddClassHandler<AntToast>((toast, _) => toast.UpdateKind());
        IsLeavingProperty.Changed.AddClassHandler<AntToast>((toast, _) => toast.UpdateLeaving());
    }

    public AntToast()
    {
        Opacity = 0;
        Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(180)
            }
        };
        UpdateKind();
    }

    public AntToastKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>离开状态：淡出后再由队列移除。</summary>
    public bool IsLeaving
    {
        get => GetValue(IsLeavingProperty);
        set => SetValue(IsLeavingProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!IsLeaving)
        {
            Dispatcher.UIThread.Post(() => SetCurrentValue(OpacityProperty, 1d), DispatcherPriority.Loaded);
        }
    }

    private void UpdateLeaving()
    {
        if (IsLeaving)
        {
            SetCurrentValue(OpacityProperty, 0d);
        }
    }

    private void UpdateKind()
    {
        PseudoClasses.Set(":success", Kind == AntToastKind.Success);
        PseudoClasses.Set(":error", Kind == AntToastKind.Error);
        PseudoClasses.Set(":warning", Kind == AntToastKind.Warning);
        PseudoClasses.Set(":info", Kind == AntToastKind.Info);
        SetCurrentValue(IconProperty, Kind switch
        {
            AntToastKind.Success => AntIcons.Check,
            AntToastKind.Error => AntIcons.Close,
            AntToastKind.Warning => AntIcons.Warning,
            _ => AntIcons.Info
        });
    }
}
