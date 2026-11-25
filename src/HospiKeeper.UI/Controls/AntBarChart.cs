using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace HospiKeeper.UI.Controls;

public sealed record ChartBar(string Label, double Value);

/// <summary>轻量自绘柱状图，避免引入额外图表依赖。</summary>
public class AntBarChart : Control
{
    public static readonly StyledProperty<IReadOnlyList<ChartBar>?> ItemsProperty =
        AvaloniaProperty.Register<AntBarChart, IReadOnlyList<ChartBar>?>(nameof(Items));

    public static readonly StyledProperty<IBrush?> BarBrushProperty =
        AvaloniaProperty.Register<AntBarChart, IBrush?>(nameof(BarBrush));

    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<AntBarChart, IBrush?>(nameof(LabelBrush));

    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<AntBarChart, IBrush?>(nameof(ValueBrush));

    public static readonly StyledProperty<IBrush?> GridBrushProperty =
        AvaloniaProperty.Register<AntBarChart, IBrush?>(nameof(GridBrush));

    private static readonly Typeface Typeface =
        new(new FontFamily("Microsoft YaHei UI, Microsoft YaHei, Segoe UI"));

    static AntBarChart()
    {
        AffectsRender<AntBarChart>(ItemsProperty, BarBrushProperty, LabelBrushProperty,
            ValueBrushProperty, GridBrushProperty);
    }

    public IReadOnlyList<ChartBar>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public IBrush? BarBrush
    {
        get => GetValue(BarBrushProperty);
        set => SetValue(BarBrushProperty, value);
    }

    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public IBrush? ValueBrush
    {
        get => GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    public IBrush? GridBrush
    {
        get => GetValue(GridBrushProperty);
        set => SetValue(GridBrushProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var items = Items;
        var labelBrush = LabelBrush ?? Brushes.Gray;
        var valueBrush = ValueBrush ?? Brushes.DimGray;
        var barBrush = BarBrush ?? Brushes.DodgerBlue;
        var gridBrush = GridBrush ?? Brushes.Gainsboro;

        if (items is null || items.Count == 0)
        {
            DrawCenteredText(context, "暂无数据", bounds, labelBrush);
            return;
        }

        const double left = 10;
        const double right = 10;
        const double top = 26;
        const double bottom = 26;
        var chartWidth = Math.Max(1, bounds.Width - left - right);
        var chartHeight = Math.Max(1, bounds.Height - top - bottom);
        var baselineY = top + chartHeight;

        context.DrawLine(new Pen(gridBrush, 1), new Point(left, baselineY), new Point(left + chartWidth, baselineY));

        var max = Math.Max(1d, items.Max(i => i.Value));
        var slot = chartWidth / items.Count;
        var barWidth = Math.Min(42d, slot * 0.55);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var height = Math.Max(item.Value <= 0 ? 0 : 3, item.Value / max * (chartHeight - 28));
            var x = left + slot * i + (slot - barWidth) / 2;
            var y = baselineY - height;

            if (height > 0)
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.Parse("#6FAEFF"), 0),
                        new GradientStop((barBrush as ISolidColorBrush)?.Color ?? Color.Parse("#1677FF"), 1)
                    }
                };
                context.DrawRectangle(brush, null,
                    new Rect(x, y, barWidth, height), 3, 3);
            }

            var valueText = FormatNumber(item.Value);
            var formattedValue = new FormattedText(valueText, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, Typeface, 10, valueBrush);
            context.DrawText(formattedValue,
                new Point(x + barWidth / 2 - formattedValue.Width / 2, Math.Max(2, y - 16)));

            var formattedLabel = new FormattedText(item.Label, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, Typeface, 10, labelBrush);
            context.DrawText(formattedLabel,
                new Point(x + barWidth / 2 - formattedLabel.Width / 2, baselineY + 7));
        }
    }

    private static void DrawCenteredText(DrawingContext context, string text, Rect bounds, IBrush brush)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, Typeface, 12, brush);
        context.DrawText(formatted,
            new Point((bounds.Width - formatted.Width) / 2, (bounds.Height - formatted.Height) / 2));
    }

    private static string FormatNumber(double value)
        => value >= 10000 ? $"{value / 10000:0.#}w" : value.ToString("0.#");
}
