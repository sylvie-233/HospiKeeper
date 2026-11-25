using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>关于弹窗。</summary>
public sealed class AboutDialogViewModel : DialogViewModelBase
{
    private readonly SiteSettings _settings;

    public AboutDialogViewModel(DialogService host, SiteSettings settings) : base(host)
    {
        _settings = settings;
        CloseCommand = new RelayCommand(Host.Close);
    }

    public override string Title => "关于本系统";

    public override double Width => 460;

    public string AppName => _settings.DesktopAppName;

    public string Version => _settings.DesktopVersion;

    public string Description => _settings.AboutDescription;

    public string Architecture => "Avalonia 12 · .NET 10 · ASP.NET Core API · MySQL";

    public IRelayCommand CloseCommand { get; }
}
