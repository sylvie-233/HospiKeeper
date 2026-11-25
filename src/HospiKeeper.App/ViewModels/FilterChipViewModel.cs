using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HospiKeeper.App.ViewModels;

/// <summary>胶囊筛选条中的单个选项。</summary>
public sealed partial class FilterChipViewModel : ObservableObject
{
    public FilterChipViewModel(string text, Func<Task> select)
    {
        Text = text;
        Command = new AsyncRelayCommand(select);
    }

    [ObservableProperty]
    private bool _isSelected;

    public string Text { get; }

    public IAsyncRelayCommand Command { get; }

    public Color BackgroundColor => IsSelected ? Color.FromArgb("#2563EB") : Colors.White;

    public Color TextColor => IsSelected ? Colors.White : Color.FromArgb("#6B7280");

    public Color BorderColor => IsSelected ? Color.FromArgb("#2563EB") : Color.FromArgb("#E5E7EB");

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
        OnPropertyChanged(nameof(BorderColor));
    }
}
