using Avalonia;
using Avalonia.Controls;

namespace HospiKeeper.UI.Controls;

/// <summary>带标签与校验信息的表单项容器（Ant Design Form.Item 的轻量实现）。</summary>
public class AntFormItem : ContentControl
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<AntFormItem, string?>(nameof(Label));

    public static readonly StyledProperty<bool> RequiredProperty =
        AvaloniaProperty.Register<AntFormItem, bool>(nameof(Required));

    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<AntFormItem, string?>(nameof(ErrorText));

    public static readonly StyledProperty<string?> HelpTextProperty =
        AvaloniaProperty.Register<AntFormItem, string?>(nameof(HelpText));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool Required
    {
        get => GetValue(RequiredProperty);
        set => SetValue(RequiredProperty, value);
    }

    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public string? HelpText
    {
        get => GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value);
    }
}
