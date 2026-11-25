using Avalonia.Controls;
using Avalonia.Input;
using HospiKeeper.Desktop.ViewModels;

namespace HospiKeeper.Desktop.Views;

/// <summary>登录窗口。</summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnOpened(object? sender, System.EventArgs e) => WindowCentering.Center(this);

    private void OnHeroPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Button)
        {
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
