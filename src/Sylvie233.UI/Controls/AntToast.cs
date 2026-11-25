using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Sylvie233.UI.Icons;

namespace Sylvie233.UI.Controls;

public enum AntToastKind
{
    Success,
    Error,
    Warning,
    Info
}

public class AntToast : ContentControl
{
    public static readonly StyledProperty<AntToastKind> KindProperty =
        AvaloniaProperty.Register<AntToast, AntToastKind>(nameof(Kind));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AntToast, Geometry?>(nameof(Icon), AntIcons.Check);

    static AntToast()
    {
        KindProperty.Changed.AddClassHandler<AntToast>((toast, _) => toast.UpdateKind());
    }

    public AntToast() => UpdateKind();

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
