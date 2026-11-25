using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Models;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Domain.Enums;
using Sylvie233.UI;
using Sylvie233.UI.Controls;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>房态看板页面。</summary>
public sealed partial class DashboardPageViewModel : PageViewModelBase
{
    private bool _suppressFilterReload;
    private IReadOnlyList<BoardRoom> _lastBoard = Array.Empty<BoardRoom>();

    public DashboardPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        RefreshCommand = new AsyncRelayCommand(LoadBoardAsync);
    }

    public override string Key => PageKeys.Dashboard;
    public override string Title => "房态看板";
    public override Geometry Icon => AntIcons.Dashboard;
    public override int PageOrder => 10;

    public ObservableCollection<FloorFilterOption> FloorOptions { get; } = new();
    public ObservableCollection<RoomTypeFilterOption> RoomTypeOptions { get; } = new();
    public ObservableCollection<LegendItemViewModel> Legends { get; } = new();
    public ObservableCollection<FloorGroupViewModel> FloorGroups { get; } = new();

    [ObservableProperty]
    private FloorFilterOption? _selectedFloor;

    [ObservableProperty]
    private RoomTypeFilterOption? _selectedRoomType;

    [ObservableProperty]
    private string _summaryText = "共 0 间房";

    [ObservableProperty]
    private bool _isEmpty;

    public IAsyncRelayCommand RefreshCommand { get; }

    partial void OnSelectedFloorChanged(FloorFilterOption? value)
    {
        if (!_suppressFilterReload)
        {
            _ = LoadBoardAsync();
        }
    }

    partial void OnSelectedRoomTypeChanged(RoomTypeFilterOption? value)
    {
        if (!_suppressFilterReload)
        {
            _ = LoadBoardAsync();
        }
    }

    public override async Task OnNavigatedToAsync(object? argument)
    {
        // 每次进入看板都刷新筛选来源，确保新增楼层 / 房型立即可见。
        await RefreshFiltersAsync();
        await LoadBoardAsync();
    }

    private async Task RefreshFiltersAsync()
    {
        await TryLoadAsync(async () =>
        {
            var floors = await Api.GetFloorsAsync();
            var types = await Api.GetRoomTypesAsync();
            var previousFloor = SelectedFloor?.Value;
            var previousType = SelectedRoomType?.Value;

            _suppressFilterReload = true;
            try
            {
                FloorOptions.Clear();
                FloorOptions.Add(new FloorFilterOption(null, "全部楼层"));
                foreach (var floor in floors)
                {
                    FloorOptions.Add(new FloorFilterOption(floor, $"{floor} 楼"));
                }

                RoomTypeOptions.Clear();
                RoomTypeOptions.Add(new RoomTypeFilterOption(null, "全部房型"));
                foreach (var type in types)
                {
                    RoomTypeOptions.Add(new RoomTypeFilterOption(type.Id, type.Name));
                }

                SelectedFloor = FloorOptions.FirstOrDefault(option => option.Value == previousFloor)
                                ?? FloorOptions[0];
                SelectedRoomType = RoomTypeOptions.FirstOrDefault(option => option.Value == previousType)
                                   ?? RoomTypeOptions[0];
            }
            finally
            {
                _suppressFilterReload = false;
            }
        });
    }

    private async Task LoadBoardAsync()
    {
        await TryLoadAsync(async () =>
        {
            _lastBoard = await Api.GetBoardAsync();
            ApplyBoard();
        });
    }

    private void ApplyBoard()
    {
        var floor = SelectedFloor?.Value;
        var typeId = SelectedRoomType?.Value;
        var filtered = _lastBoard
            .Where(b => (floor is null || b.Room.Floor == floor)
                        && (typeId is null || b.Room.TypeId == typeId))
            .ToList();

        FloorGroups.Clear();
        foreach (var group in filtered
                     .GroupBy(b => b.Room.Floor)
                     .OrderBy(g => g.Key))
        {
            var cards = group
                .OrderBy(b => b.Room.RoomNo, StringComparer.OrdinalIgnoreCase)
                .Select(BuildCard)
                .ToList();
            FloorGroups.Add(new FloorGroupViewModel(group.Key, cards));
        }

        SummaryText = $"共 {filtered.Count} 间房 · {filtered.Count(b => b.Room.Status == RoomStatus.Free)} 间空闲";
        IsEmpty = FloorGroups.Count == 0;
        BuildLegend();
    }

    private void BuildLegend()
    {
        var defs = new (string Label, RoomStatus Status, AntRoomCardState State, IBrush Brush)[]
        {
            ("空闲", RoomStatus.Free, AntRoomCardState.Free, AntPalette.RoomFree),
            ("已入住", RoomStatus.Occupied, AntRoomCardState.Occupied, AntPalette.RoomOccupied),
            ("预定", RoomStatus.Reserved, AntRoomCardState.Reserved, AntPalette.RoomReserved),
            ("打扫中", RoomStatus.Cleaning, AntRoomCardState.Cleaning, AntPalette.RoomCleaning),
            ("维修", RoomStatus.Repair, AntRoomCardState.Repair, AntPalette.RoomRepair),
            ("停用", RoomStatus.Disabled, AntRoomCardState.Disabled, AntPalette.RoomDisabled)
        };

        Legends.Clear();
        foreach (var def in defs)
        {
            var count = _lastBoard.Count(b => b.Room.Status == def.Status);
            Legends.Add(new LegendItemViewModel($"{def.Label} {count}", def.Brush));
        }
    }

    private RoomCardViewModel BuildCard(BoardRoom board)
    {
        var room = board.Room;
        var state = MapState(room.Status);
        var statusText = MapStatusText(room.Status);
        var guestText = room.Status switch
        {
            RoomStatus.Occupied => board.ActiveStay?.GuestName ?? string.Empty,
            RoomStatus.Reserved => board.PendingReservation is null
                ? string.Empty
                : $"预定 · {board.PendingReservation.GuestName}",
            RoomStatus.Cleaning => "等待保洁",
            RoomStatus.Repair => room.Note ?? "维修中",
            RoomStatus.Disabled => "已停用",
            _ => string.Empty
        };

        var actions = BuildActions(board);
        return new RoomCardViewModel(
            room.RoomNo,
            room.TypeName,
            $"¥{room.Price:0.##}/晚",
            guestText,
            statusText,
            state,
            actions,
            card => Dialog.Show(new RoomActionDialogViewModel(
                Dialog,
                Toast,
                $"{card.RoomNo} · {card.TypeName}",
                $"{card.StatusText}｜{card.PriceText}",
                card.Actions)));
    }

    private List<RoomActionItem> BuildActions(BoardRoom board)
    {
        var room = board.Room;
        var actions = new List<RoomActionItem>();

        switch (room.Status)
        {
            case RoomStatus.Free:
                actions.Add(new RoomActionItem("办理入住", "登记客人信息并生成入住单", false,
                    () =>
                    {
                        Navigation.Navigate(PageKeys.CheckIn, new CheckInPrefill(RoomId: room.Id));
                        return Task.CompletedTask;
                    }));
                actions.Add(new RoomActionItem("新增预定", "为客户保留该房间", false,
                    () => ShowNewReservationAsync(room)));
                break;

            case RoomStatus.Occupied when board.ActiveStay is { } stay:
                actions.Add(new RoomActionItem("查看在住", "打开在住列表并定位房间", false,
                    () =>
                    {
                        Navigation.Navigate(PageKeys.Stay, new StayFilter(room.RoomNo));
                        return Task.CompletedTask;
                    }));
                actions.Add(new RoomActionItem("追加消费", "录入矿泉水、泡面等杂费", false,
                    () =>
                    {
                        Dialog.Show(new AddExtraDialogViewModel(Dialog, Toast, Api, stay, OnStayChangedAsync));
                        return Task.CompletedTask;
                    }));
                actions.Add(new RoomActionItem("退房结算", "核算房费、追加消费与押金抵扣", false,
                    () =>
                    {
                        Dialog.Show(new CheckoutDialogViewModel(Dialog, Toast, Api, stay, OnStayChangedAsync));
                        return Task.CompletedTask;
                    }));
                break;

            case RoomStatus.Reserved when board.PendingReservation is { } reservation:
                actions.Add(new RoomActionItem("转入住", "一键带入预定客人信息", false,
                    () =>
                    {
                        Navigation.Navigate(PageKeys.CheckIn,
                            new CheckInPrefill(room.Id, reservation.Id));
                        return Task.CompletedTask;
                    }));
                actions.Add(new RoomActionItem("查看预定", "查看预订单详情", false,
                    () =>
                    {
                        Dialog.Show(new ReservationDetailDialogViewModel(Dialog, reservation));
                        return Task.CompletedTask;
                    }));
                actions.Add(new RoomActionItem("取消预定", "释放房间，恢复为空闲", true,
                    async () =>
                    {
                        if (!await Dialog.ConfirmAsync("取消预定",
                                $"确定取消预定 {reservation.ResNo}（{reservation.GuestName}）吗？",
                                "取消预定", "返回"))
                        {
                            return;
                        }

                        await RunAsync(async () =>
                        {
                            await Api.CancelReservationAsync(reservation.Id);
                            await LoadBoardAsync();
                        }, "预定已取消，房间已恢复空闲");
                    }));
                break;

            case RoomStatus.Cleaning:
                actions.Add(new RoomActionItem("保洁完成", "将房间恢复为空闲", false,
                    async () => await RunAsync(async () =>
                    {
                        await Api.MarkCleaningCompleteAsync(room.Id);
                        await LoadBoardAsync();
                    }, $"房间 {room.RoomNo} 已完成保洁")));
                break;

            case RoomStatus.Repair:
            case RoomStatus.Disabled:
                actions.Add(new RoomActionItem("恢复为空闲", "维修 / 停用结束后恢复售卖", false,
                    async () => await RunAsync(async () =>
                    {
                        await Api.RestoreRoomAsync(room.Id);
                        await LoadBoardAsync();
                    }, $"房间 {room.RoomNo} 已恢复空闲")));
                break;
        }

        return actions;
    }

    private async Task ShowNewReservationAsync(Room room)
    {
        try
        {
            var freeRooms = await Api.GetFreeRoomsAsync();
            Dialog.Show(new NewReservationDialogViewModel(
                Dialog, Toast, Api, freeRooms, room.Id, async () =>
                {
                    await LoadBoardAsync();
                    NotifyDataChanged();
                }));
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
        }
    }

    private async Task OnStayChangedAsync()
    {
        await LoadBoardAsync();
        NotifyDataChanged();
    }

    private static AntRoomCardState MapState(RoomStatus status) => status switch
    {
        RoomStatus.Free => AntRoomCardState.Free,
        RoomStatus.Occupied => AntRoomCardState.Occupied,
        RoomStatus.Reserved => AntRoomCardState.Reserved,
        RoomStatus.Cleaning => AntRoomCardState.Cleaning,
        RoomStatus.Repair => AntRoomCardState.Repair,
        _ => AntRoomCardState.Disabled
    };

    private static string MapStatusText(RoomStatus status) => status switch
    {
        RoomStatus.Free => "空闲",
        RoomStatus.Occupied => "已入住",
        RoomStatus.Reserved => "预定",
        RoomStatus.Cleaning => "打扫中",
        RoomStatus.Repair => "维修",
        _ => "停用"
    };
}

/// <summary>楼层筛选项。</summary>
public sealed record FloorFilterOption(int? Value, string Text);

/// <summary>房型筛选项。</summary>
public sealed record RoomTypeFilterOption(int? Value, string Text);

/// <summary>房态图例项。</summary>
public sealed record LegendItemViewModel(string Text, IBrush Brush);

/// <summary>按楼层分组的房间卡片。</summary>
public sealed class FloorGroupViewModel
{
    public FloorGroupViewModel(int floor, IReadOnlyList<RoomCardViewModel> rooms)
    {
        Floor = floor;
        Rooms = rooms;
    }

    public int Floor { get; }
    public IReadOnlyList<RoomCardViewModel> Rooms { get; }
    public string Title => $"{Floor} 楼";
    public string CountText => $"{Rooms.Count} 间";
}

/// <summary>房态看板上的房间卡片。</summary>
public sealed class RoomCardViewModel
{
    public RoomCardViewModel(
        string roomNo,
        string typeName,
        string priceText,
        string guestText,
        string statusText,
        AntRoomCardState state,
        IReadOnlyList<RoomActionItem> actions,
        Action<RoomCardViewModel> openActions)
    {
        RoomNo = roomNo;
        TypeName = typeName;
        PriceText = priceText;
        GuestText = guestText;
        StatusText = statusText;
        State = state;
        Actions = actions;
        OpenActionsCommand = new RelayCommand(() => openActions(this));
    }

    public string RoomNo { get; }
    public string TypeName { get; }
    public string PriceText { get; }
    public string GuestText { get; }
    public string StatusText { get; }
    public AntRoomCardState State { get; }
    public IReadOnlyList<RoomActionItem> Actions { get; }
    public IRelayCommand OpenActionsCommand { get; }
}
