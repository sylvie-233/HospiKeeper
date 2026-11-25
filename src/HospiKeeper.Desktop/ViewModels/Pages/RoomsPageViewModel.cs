using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Domain.Enums;
using Sylvie233.UI.Controls;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>房间表格行。</summary>
public sealed class RoomRowViewModel
{
    public RoomRowViewModel(Room room, RoomsPageViewModel page)
    {
        Room = room;
        EditCommand = new RelayCommand(() => page.ShowEdit(this));
        DeleteCommand = new AsyncRelayCommand(() => page.DeleteAsync(this));
        RepairCommand = new AsyncRelayCommand(() => page.MarkRepairAsync(this));
        DisableCommand = new AsyncRelayCommand(() => page.DisableAsync(this));
        RestoreCommand = new AsyncRelayCommand(() => page.RestoreAsync(this));
    }

    public Room Room { get; }
    public string RoomNo => Room.RoomNo;
    public string FloorText => $"{Room.Floor} 楼";
    public string TypeName => Room.TypeName;
    public string BedsText => $"{Room.Beds} 床";
    public string PriceText => $"¥{Room.Price:0.00}";
    public string MinPriceText => $"¥{Room.MinPrice:0.00}";
    public string DepositText => $"¥{Room.Deposit:0.00}";
    public string FeatureText => $"{(Room.HasWindow ? "带窗" : "无窗")} · {(Room.HasBath ? "独卫" : "公卫")}";
    public string NoteText => string.IsNullOrWhiteSpace(Room.Note) ? "—" : Room.Note;

    public string StatusText => Room.Status switch
    {
        RoomStatus.Free => "空闲",
        RoomStatus.Occupied => "已入住",
        RoomStatus.Reserved => "预定",
        RoomStatus.Cleaning => "打扫中",
        RoomStatus.Repair => "维修",
        _ => "停用"
    };

    public AntTagVariant StatusVariant => Room.Status switch
    {
        RoomStatus.Free => AntTagVariant.Success,
        RoomStatus.Occupied => AntTagVariant.Processing,
        RoomStatus.Reserved => AntTagVariant.Purple,
        RoomStatus.Cleaning => AntTagVariant.Cyan,
        RoomStatus.Repair => AntTagVariant.Warning,
        _ => AntTagVariant.Default
    };

    public bool CanRepair => Room.Status is RoomStatus.Free or RoomStatus.Cleaning;
    public bool CanDisable => Room.Status is RoomStatus.Free or RoomStatus.Cleaning;
    public bool CanRestore => Room.Status is RoomStatus.Repair or RoomStatus.Disabled;
    public bool CanDelete => Room.Status is not (RoomStatus.Occupied or RoomStatus.Reserved);

    public IRelayCommand EditCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand RepairCommand { get; }
    public IAsyncRelayCommand DisableCommand { get; }
    public IAsyncRelayCommand RestoreCommand { get; }
}

/// <summary>房间管理页面。</summary>
public sealed partial class RoomsPageViewModel : PageViewModelBase
{
    private bool _filtersLoaded;
    private bool _suppressFilterReload;
    private IReadOnlyList<RoomType> _roomTypes = Array.Empty<RoomType>();

    public RoomsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        AddCommand = new AsyncRelayCommand(AddAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public override string Key => PageKeys.Rooms;
    public override string Title => "房间管理";
    public override Geometry Icon => AntIcons.Door;
    public override string Group => "基础信息（管理员）";
    public override bool AdminOnly => true;
    public override int GroupOrder => 2;
    public override int PageOrder => 80;

    public ObservableCollection<RoomRowViewModel> Rows { get; } = new();
    public ObservableCollection<FloorFilterOption> FloorOptions { get; } = new();
    public ObservableCollection<RoomTypeFilterOption> RoomTypeOptions { get; } = new();

    [ObservableProperty]
    private FloorFilterOption? _selectedFloor;

    [ObservableProperty]
    private RoomTypeFilterOption? _selectedRoomType;

    [ObservableProperty]
    private string _countText = "共 0 间房";

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    partial void OnSelectedFloorChanged(FloorFilterOption? value)
    {
        if (!_suppressFilterReload)
        {
            _ = LoadAsync();
        }
    }

    partial void OnSelectedRoomTypeChanged(RoomTypeFilterOption? value)
    {
        if (!_suppressFilterReload)
        {
            _ = LoadAsync();
        }
    }

    public override async Task OnNavigatedToAsync(object? argument)
    {
        await EnsureFiltersAsync();
        await LoadAsync();
    }


    private async Task EnsureFiltersAsync()
    {
        if (_filtersLoaded)
        {
            return;
        }

        await TryLoadAsync(async () =>
        {
            var floors = await Api.GetFloorsAsync();
            _roomTypes = await Api.GetRoomTypesAsync();

            _suppressFilterReload = true;
            FloorOptions.Clear();
            FloorOptions.Add(new FloorFilterOption(null, "全部楼层"));
            foreach (var floor in floors)
            {
                FloorOptions.Add(new FloorFilterOption(floor, $"{floor} 楼"));
            }

            RoomTypeOptions.Clear();
            RoomTypeOptions.Add(new RoomTypeFilterOption(null, "全部房型"));
            foreach (var type in _roomTypes)
            {
                RoomTypeOptions.Add(new RoomTypeFilterOption(type.Id, type.Name));
            }

            SelectedFloor = FloorOptions[0];
            SelectedRoomType = RoomTypeOptions[0];
            _suppressFilterReload = false;
            _filtersLoaded = true;
        });
    }

    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var rooms = await Api.GetRoomsAsync(SelectedFloor?.Value, SelectedRoomType?.Value);
            Rows.Clear();
            foreach (var room in rooms)
            {
                Rows.Add(new RoomRowViewModel(room, this));
            }

            CountText = $"共 {Rows.Count} 间房";
        });
    }

    private async Task AddAsync()
    {
        if (!_filtersLoaded)
        {
            await EnsureFiltersAsync();
        }

        Dialog.Show(new RoomEditDialogViewModel(
            Dialog, Toast, Api, null, _roomTypes, async () =>
            {
                await LoadAsync();
                NotifyDataChanged();
            }));
    }

    internal void ShowEdit(RoomRowViewModel row)
        => Dialog.Show(new RoomEditDialogViewModel(
            Dialog, Toast, Api, row.Room, _roomTypes, async () =>
            {
                await LoadAsync();
                NotifyDataChanged();
            }));

    internal async Task DeleteAsync(RoomRowViewModel row)
    {
        var confirmed = await Dialog.ConfirmAsync(
            "删除房间",
            $"确定删除房间 {row.RoomNo} 吗？该操作不可恢复，有历史单据的房间将无法删除。",
            "删除",
            "取消");
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.DeleteRoomAsync(row.Room.Id);
            await LoadAsync();
        }, $"房间 {row.RoomNo} 已删除");
    }

    internal async Task MarkRepairAsync(RoomRowViewModel row)
    {
        var confirmed = await Dialog.ConfirmAsync(
            "标记维修",
            $"将房间 {row.RoomNo} 标记为维修？期间不参与售卖与入住率统计。",
            "标记维修",
            "取消",
            danger: false);
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.SetMaintenanceAsync(row.Room.Id, RoomStatus.Repair, null);
            await LoadAsync();
        }, $"房间 {row.RoomNo} 已标记维修");
    }

    internal async Task DisableAsync(RoomRowViewModel row)
    {
        var confirmed = await Dialog.ConfirmAsync(
            "停用房间",
            $"确定停用房间 {row.RoomNo} 吗？停用后不参与入住与统计。",
            "停用",
            "取消");
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.SetMaintenanceAsync(row.Room.Id, RoomStatus.Disabled, null);
            await LoadAsync();
        }, $"房间 {row.RoomNo} 已停用");
    }

    internal async Task RestoreAsync(RoomRowViewModel row)
    {
        await RunAsync(async () =>
        {
            await Api.RestoreRoomAsync(row.Room.Id);
            await LoadAsync();
        }, $"房间 {row.RoomNo} 已恢复空闲");
    }
}
