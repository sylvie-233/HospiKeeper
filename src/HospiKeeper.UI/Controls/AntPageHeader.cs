using Avalonia;
using Avalonia.Controls;

namespace HospiKeeper.UI.Controls;

public class AntPageHeader : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntPageHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<AntPageHeader, string?>(nameof(Subtitle));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<AntPageHeader, object?>(nameof(Extra));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }
}
