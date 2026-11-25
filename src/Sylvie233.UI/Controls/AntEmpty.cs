using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Sylvie233.UI.Controls;

public class AntEmpty : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntEmpty, string?>(nameof(Title), "暂无数据");

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<AntEmpty, string?>(nameof(Description));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AntEmpty, Geometry?>(nameof(Icon));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
