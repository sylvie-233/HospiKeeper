using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed class AboutDialogViewModel : DialogViewModelBase
{
    public AboutDialogViewModel(DialogService host) : base(host)
    {
        CloseCommand = new RelayCommand(Host.Close);
    }

    public override string Title => "关于本系统";

    public override double Width => 460;

    public string Version => "v1.0";

    public string Description => "面向中小型民宿 / 小旅馆的单机经营管理软件，聚焦房态、入住、预定、结算与报表。";

    public string Architecture => "Avalonia 12 · .NET 10 · ASP.NET Core API · MySQL";

    public IRelayCommand CloseCommand { get; }
}
