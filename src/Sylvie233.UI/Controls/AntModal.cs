using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

public class AntModal : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AntModal, string?>(nameof(Title));

    public static readonly StyledProperty<double> ModalWidthProperty =
        AvaloniaProperty.Register<AntModal, double>(nameof(ModalWidth), 520d);

    public static readonly StyledProperty<System.Windows.Input.ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<AntModal, System.Windows.Input.ICommand?>(nameof(CloseCommand));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double ModalWidth
    {
        get => GetValue(ModalWidthProperty);
        set => SetValue(ModalWidthProperty, value);
    }

    public System.Windows.Input.ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}
