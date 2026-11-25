using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>房间快捷操作定义。</summary>
public sealed record RoomActionItem(string Title, string Description, bool IsDanger, Func<Task> Execute);

/// <summary>房间快捷操作展示项。</summary>
public sealed class RoomActionItemViewModel
{
    public RoomActionItemViewModel(RoomActionDialogViewModel owner, RoomActionItem item)
    {
        Title = item.Title;
        Description = item.Description;
        IsDanger = item.IsDanger;
        ExecuteCommand = new AsyncRelayCommand(async () =>
        {
            owner.Host.Close();
            try
            {
                await item.Execute();
            }
            catch (Exception ex)
            {
                owner.Toast.Error(ex.Message);
            }
        });
    }

    public string Title { get; }
    public string Description { get; }
    public bool IsDanger { get; }
    public IAsyncRelayCommand ExecuteCommand { get; }
}

/// <summary>房间快捷操作弹窗。</summary>
public sealed class RoomActionDialogViewModel : DialogViewModelBase
{
    public RoomActionDialogViewModel(
        DialogService host,
        ToastService toast,
        string heading,
        string subheading,
        IReadOnlyList<RoomActionItem> actions) : base(host)
    {
        Toast = toast;
        Heading = heading;
        Subheading = subheading;
        Actions = actions.Select(a => new RoomActionItemViewModel(this, a)).ToList();
        CancelCommand = new RelayCommand(Host.Close);
    }

    public ToastService Toast { get; }

    public override string Title => "房间快捷操作";
    public override double Width => 440;

    public string Heading { get; }
    public string Subheading { get; }
    public IReadOnlyList<RoomActionItemViewModel> Actions { get; }
    public IRelayCommand CancelCommand { get; }
}
