using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HospiKeeper.Desktop.ViewModels;

/// <summary>侧边导航项。</summary>
public sealed partial class NavigationItemViewModel : ObservableObject
{
    public NavigationItemViewModel(string key, string title, Geometry icon, Action select)
    {
        Key = key;
        Title = title;
        Icon = icon;
        SelectCommand = new RelayCommand(select);
    }

    public string Key { get; }
    public string Title { get; }
    public Geometry Icon { get; }
    public IRelayCommand SelectCommand { get; }

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private int _badge;

    public bool HasBadge => Badge > 0;

    partial void OnBadgeChanged(int value) => OnPropertyChanged(nameof(HasBadge));
}

/// <summary>侧边导航分组。</summary>
public sealed class NavigationGroupViewModel
{
    public NavigationGroupViewModel(string title, int order)
    {
        Title = title;
        Order = order;
    }

    public string Title { get; }
    public int Order { get; }
    public ObservableCollection<NavigationItemViewModel> Items { get; } = new();
}
