using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

/// <summary>标签的语义色变体。</summary>
public enum AntTagVariant
{
    Default,
    Success,
    Processing,
    Warning,
    Error,
    Purple,
    Cyan
}

/// <summary>Ant Design 风格标签。</summary>
public class AntTag : ContentControl
{
    public static readonly StyledProperty<AntTagVariant> VariantProperty =
        AvaloniaProperty.Register<AntTag, AntTagVariant>(nameof(Variant));

    static AntTag()
    {
        VariantProperty.Changed.AddClassHandler<AntTag>((tag, _) => tag.UpdateVariant());
    }

    public AntTag() => UpdateVariant();

    public AntTagVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private void UpdateVariant()
    {
        PseudoClasses.Set(":success", Variant == AntTagVariant.Success);
        PseudoClasses.Set(":processing", Variant == AntTagVariant.Processing);
        PseudoClasses.Set(":warning", Variant == AntTagVariant.Warning);
        PseudoClasses.Set(":error", Variant == AntTagVariant.Error);
        PseudoClasses.Set(":purple", Variant == AntTagVariant.Purple);
        PseudoClasses.Set(":cyan", Variant == AntTagVariant.Cyan);
    }
}
