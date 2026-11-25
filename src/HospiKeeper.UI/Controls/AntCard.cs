using Avalonia;
using Avalonia.Controls;

namespace HospiKeeper.UI.Controls;

public class AntCard : ContentControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<AntCard, object?>(nameof(Header));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AntCard, object?>(nameof(Extra));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
}
