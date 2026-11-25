using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

/// <summary>按钮视觉变体，对应 Ant Design 的 type。</summary>
public enum AntButtonVariant
{
    Default,
    Primary,
    Danger,
    Text,
    Link
}

/// <summary>按钮尺寸。</summary>
public enum AntButtonSize
{
    Small,
    Middle,
    Large
}

/// <summary>Ant Design 风格按钮，支持变体、尺寸与加载态。</summary>
public class AntButton : Button
{
    public static readonly StyledProperty<AntButtonVariant> VariantProperty =
        AvaloniaProperty.Register<AntButton, AntButtonVariant>(nameof(Variant));

    public static readonly StyledProperty<AntButtonSize> SizeProperty =
        AvaloniaProperty.Register<AntButton, AntButtonSize>(nameof(Size));

    static AntButton()
    {
        VariantProperty.Changed.AddClassHandler<AntButton>((button, _) => button.UpdateVariant());
        SizeProperty.Changed.AddClassHandler<AntButton>((button, _) => button.UpdateSize());
    }

    public AntButton()
    {
        UpdateVariant();
        UpdateSize();
    }

    public AntButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public AntButtonSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private void UpdateVariant()
    {
        PseudoClasses.Set(":primary", Variant == AntButtonVariant.Primary);
        PseudoClasses.Set(":danger", Variant == AntButtonVariant.Danger);
        PseudoClasses.Set(":text", Variant == AntButtonVariant.Text);
        PseudoClasses.Set(":link", Variant == AntButtonVariant.Link);
    }

    private void UpdateSize()
    {
        PseudoClasses.Set(":small", Size == AntButtonSize.Small);
        PseudoClasses.Set(":large", Size == AntButtonSize.Large);
    }
}
