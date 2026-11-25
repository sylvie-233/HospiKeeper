using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>房型表格行。</summary>
public sealed class RoomTypeRowViewModel
{
    public RoomTypeRowViewModel(RoomType type, int roomCount, RoomTypesPageViewModel page)
    {
        Type = type;
        RoomCount = roomCount;
        EditCommand = new RelayCommand(() => page.ShowEdit(this));
        DeleteCommand = new AsyncRelayCommand(() => page.DeleteAsync(this));
    }

    public RoomType Type { get; }
    public int RoomCount { get; }
    public string Name => Type.Name;
    public string PriceText => $"¥{Type.BasePrice:0.00}";
    public string NoteText => string.IsNullOrWhiteSpace(Type.Note) ? "—" : Type.Note;
    public string RoomCountText => $"{RoomCount} 间";
    public IRelayCommand EditCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
}

/// <summary>房型管理页面。</summary>
public sealed partial class RoomTypesPageViewModel : PageViewModelBase
{
    public RoomTypesPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        AddCommand = new AsyncRelayCommand(AddAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public override string Key => PageKeys.RoomTypes;
    public override string Title => "房型管理";
    public override Geometry Icon => AntIcons.Tag;
    public override string Group => "基础信息（管理员）";
    public override bool AdminOnly => true;
    public override int GroupOrder => 2;
    public override int PageOrder => 90;

    public ObservableCollection<RoomTypeRowViewModel> Rows { get; } = new();

    [ObservableProperty]
    private string _countText = "共 0 种房型";

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();


    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var types = await Api.GetRoomTypesAsync();
            var rooms = await Api.GetRoomsAsync();
            Rows.Clear();
            foreach (var type in types)
            {
                Rows.Add(new RoomTypeRowViewModel(
                    type,
                    rooms.Count(r => r.TypeId == type.Id),
                    this));
            }

            CountText = $"共 {Rows.Count} 种房型";
        });
    }

    private Task AddAsync()
    {
        Dialog.Show(new RoomTypeEditDialogViewModel(
            Dialog, Toast, Api, null, async () =>
            {
                await LoadAsync();
                NotifyDataChanged();
            }));
        return Task.CompletedTask;
    }

    internal void ShowEdit(RoomTypeRowViewModel row)
        => Dialog.Show(new RoomTypeEditDialogViewModel(
            Dialog, Toast, Api, row.Type, async () =>
            {
                await LoadAsync();
                NotifyDataChanged();
            }));

    internal async Task DeleteAsync(RoomTypeRowViewModel row)
    {
        var confirmed = await Dialog.ConfirmAsync(
            "删除房型",
            $"确定删除房型「{row.Name}」吗？该房型下若仍有房间将无法删除。",
            "删除",
            "取消");
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.DeleteRoomTypeAsync(row.Type.Id);
            await LoadAsync();
        }, $"房型「{row.Name}」已删除");
    }
}
