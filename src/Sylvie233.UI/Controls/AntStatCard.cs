using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Sylvie233.UI.Controls;

/// <summary>统计指标卡：图标 + 数值 + 说明 + 趋势。</summary>
public class AntStatCard : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntStatCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<AntStatCard, string?>(nameof(Value));

    public static readonly StyledProperty<string?> SubTextProperty =
        AvaloniaProperty.Register<AntStatCard, string?>(nameof(SubText));

    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<AntStatCard, Geometry?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> IconBackgroundProperty =
        AvaloniaProperty.Register<AntStatCard, IBrush?>(nameof(IconBackground));

    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<AntStatCard, IBrush?>(nameof(IconForeground));

    public static readonly StyledProperty<string?> TrendTextProperty =
        AvaloniaProperty.Register<AntStatCard, string?>(nameof(TrendText));

    public static readonly StyledProperty<IBrush?> TrendBrushProperty =
        AvaloniaProperty.Register<AntStatCard, IBrush?>(nameof(TrendBrush));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? SubText
    {
        get => GetValue(SubTextProperty);
        set => SetValue(SubTextProperty, value);
    }

    public Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? IconBackground
    {
        get => GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    public IBrush? IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public string? TrendText
    {
        get => GetValue(TrendTextProperty);
        set => SetValue(TrendTextProperty, value);
    }

    public IBrush? TrendBrush
    {
        get => GetValue(TrendBrushProperty);
        set => SetValue(TrendBrushProperty, value);
    }
}
