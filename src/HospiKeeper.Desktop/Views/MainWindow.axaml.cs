using Avalonia.Controls;
using Avalonia.Input;
using HospiKeeper.Desktop.ViewModels;

namespace HospiKeeper.Desktop.Views;

/// <summary>主窗口：侧边导航、页面宿主与状态栏。</summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = null!;
    private bool _initialized;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnOpened(object? sender, System.EventArgs e) => WindowCentering.Center(this);

    private void OnNavigateMenuClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: string pageKey })
        {
            _viewModel.NavigateCommand.Execute(pageKey);
        }
    }

    private void OnChangePasswordClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _viewModel.ChangePasswordCommand.Execute(null);

    private void OnLogoutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _viewModel.LogoutCommand.Execute(null);

    private void OnAboutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _viewModel.AboutCommand.Execute(null);

    private void OnShortcutsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _viewModel.ShortcutsCommand.Execute(null);

    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _viewModel.InitializeAsync();
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Button)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        BeginMoveDrag(e);
    }

    private void OnMinimizeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => ToggleMaximize();

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => Close();

    private void ToggleMaximize()
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
}
