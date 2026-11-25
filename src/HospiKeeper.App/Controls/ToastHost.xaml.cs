using HospiKeeper.App.Services;

namespace HospiKeeper.App.Controls;

/// <summary>页面底部轻提示宿主，订阅全局 ToastService。</summary>
public partial class ToastHost : ContentView
{
    private ToastService? _service;
    private int _version;

    public ToastHost()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_service is not null)
        {
            return;
        }

        _service = ServiceHelper.Get<ToastService>();
        _service.Shown += OnShown;
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_service is null)
        {
            return;
        }

        _service.Shown -= OnShown;
        _service = null;
    }

    private void OnShown(AppToastMessage message)
    {
        var version = Interlocked.Increment(ref _version);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (version != _version)
            {
                return;
            }

            (KindLabel.Text, KindLabel.TextColor) = message.Kind switch
            {
                AppToastKind.Success => ("✓", Color.FromArgb("#4ADE80")),
                AppToastKind.Error => ("!", Color.FromArgb("#F87171")),
                AppToastKind.Warning => ("!", Color.FromArgb("#FBBF24")),
                _ => ("i", Color.FromArgb("#60A5FA"))
            };
            MessageLabel.Text = message.Text;

            ToastBorder.IsVisible = true;
            ToastBorder.Opacity = 0;
            ToastBorder.TranslationY = 10;
            await Task.WhenAll(
                ToastBorder.FadeToAsync(1, 160, Easing.CubicOut),
                ToastBorder.TranslateToAsync(0, 0, 160, Easing.CubicOut));

            await Task.Delay(message.Kind == AppToastKind.Error ? 4200 : 2600);
            if (version != _version)
            {
                return;
            }

            await Task.WhenAll(
                ToastBorder.FadeToAsync(0, 140, Easing.CubicIn),
                ToastBorder.TranslateToAsync(0, 8, 140, Easing.CubicIn));
            ToastBorder.IsVisible = false;
        });
    }
}
