using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

public class AntSectionTitle : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntSectionTitle, string?>(nameof(Title));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AntSectionTitle, object?>(nameof(Extra));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
}
