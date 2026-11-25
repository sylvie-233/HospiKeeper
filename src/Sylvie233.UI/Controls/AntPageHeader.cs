using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

/// <summary>页面标题栏：标题、副标题与右侧操作区。</summary>
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
