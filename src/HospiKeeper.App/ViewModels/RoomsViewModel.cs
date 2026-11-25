using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.App.ViewModels;

public sealed class RoomCardViewModel
{
    public RoomCardViewModel(BoardRoom board, Func<RoomCardViewModel, Task> open)
    {
        Room = board.Room;
        Stay = board.ActiveStay;
        Reservation = board.PendingReservation;
        OpenCommand = new AsyncRelayCommand(() => open(this));
        BackgroundColor = RoomsViewModel.StatusColor(Room.Status);
        StatusText = RoomsViewModel.StatusText(Room.Status);
        GuestText = Room.Status switch
        {
            RoomStatus.Occupied => Stay?.GuestName ?? "在住客人",
            RoomStatus.Reserved => Reservation is null
                ? "待到店"
                : $"预定 · {Reservation.GuestName}",
            RoomStatus.Cleaning => "等待保洁",
            RoomStatus.Repair => Room.Note ?? "维修中",
            RoomStatus.Disabled => "已停用",
            _ => "可入住"
        };
    }

    public RoomDto Room { get; }
    public CheckInRecordDto? Stay { get; }
    public ReservationDto? Reservation { get; }
    public string RoomNo => Room.RoomNo;
    public string TypeName => Room.TypeName;
    public string PriceText => $"¥{Room.Price:0}/晚";
    public string StatusText { get; }
    public string GuestText { get; }
    public Color BackgroundColor { get; }
    public IAsyncRelayCommand OpenCommand { get; }

    public string FloorText => $"{Room.Floor} 楼 · {Room.Beds} 床";
    public string StayGuestText => Stay is null
        ? string.Empty
        : $"{Stay.GuestName} · {Stay.GuestPhone}";
    public string StayTimeText => Stay is null
        ? string.Empty
        : $"{Stay.CheckInTime:MM-dd HH:mm} → {Stay.PlanOutTime:MM-dd HH:mm}";
    public string StayMoneyText => Stay is null
        ? string.Empty
        : $"押金 ¥{Stay.Deposit:0.00} · 追加 ¥{Stay.ExtraTotal:0.00}";
    public string ReservationText => Reservation is null
        ? string.Empty
        : $"{Reservation.GuestName} · {Reservation.GuestPhone} · {Reservation.ArriveTime:MM-dd HH:mm}";
    public bool HasStay => Stay is not null;
    public bool HasReservation => Reservation is not null;
}

public sealed class RoomRowViewModel
{
    public RoomRowViewModel(RoomCardViewModel? room1, RoomCardViewModel? room2, RoomCardViewModel? room3)
    {
        Room1 = room1;
        Room2 = room2;
        Room3 = room3;
    }

    public RoomCardViewModel? Room1 { get; }
    public RoomCardViewModel? Room2 { get; }
    public RoomCardViewModel? Room3 { get; }
}

public sealed class RoomLegendItem
{
    public RoomLegendItem(string text, Color color)
    {
        Text = text;
        Color = color;
    }

    public string Text { get; }
    public Color Color { get; }
}

/// <summary>只读房态总览：双维度筛选、三列房卡与底部详情。</summary>
public sealed partial class RoomsViewModel : BaseViewModel
{
    private IReadOnlyList<BoardRoom> _board = [];
    private int? _selectedFloor;
    private RoomStatus? _selectedStatus;

    public RoomsViewModel(ApiProvider apiProvider, AppSession session, ToastService toast)
        : base(apiProvider, session, toast)
    {
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        CloseSheetCommand = new RelayCommand(CloseSheet);
        BuildStatusChips();
    }

    [ObservableProperty] private bool _isSheetOpen;
    [ObservableProperty] private RoomCardViewModel? _selectedRoom;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string _summaryText = "共 0 间房";

    public ObservableCollection<RoomCardViewModel> Rooms { get; } = [];
    public ObservableCollection<RoomRowViewModel> RoomRows { get; } = [];
    public ObservableCollection<FilterChipViewModel> FloorChips { get; } = [];
    public ObservableCollection<FilterChipViewModel> StatusChips { get; } = [];
    public ObservableCollection<RoomLegendItem> Legend { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand CloseSheetCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private void BuildStatusChips()
    {
        StatusChips.Clear();
        StatusChips.Add(new FilterChipViewModel("全部", () => SelectStatus(null)) { IsSelected = true });
        foreach (var status in new[]
                 {
                     RoomStatus.Free, RoomStatus.Occupied, RoomStatus.Reserved,
                     RoomStatus.Cleaning, RoomStatus.Repair, RoomStatus.Disabled
                 })
        {
            var value = status;
            StatusChips.Add(new FilterChipViewModel(StatusText(value), () => SelectStatus(value)));
        }
    }

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            _board = await Api.GetBoardAsync();
            BuildFloorChips();
            ApplyFilter();
            MarkSynced();
        });

    private void BuildFloorChips()
    {
        var previous = _selectedFloor;
        FloorChips.Clear();
        FloorChips.Add(new FilterChipViewModel("全部楼层", () => SelectFloor(null))
        {
            IsSelected = previous is null
        });
        foreach (var floor in _board.Select(b => b.Room.Floor).Distinct().Order())
        {
            var value = floor;
            FloorChips.Add(new FilterChipViewModel($"{floor} 楼", () => SelectFloor(value))
            {
                IsSelected = previous == floor
            });
        }
    }

    private Task SelectFloor(int? floor)
    {
        _selectedFloor = floor;
        foreach (var chip in FloorChips)
        {
            chip.IsSelected = floor is null
                ? chip.Text == "全部楼层"
                : chip.Text == $"{floor} 楼";
        }

        ApplyFilter();
        return Task.CompletedTask;
    }

    private Task SelectStatus(RoomStatus? status)
    {
        _selectedStatus = status;
        foreach (var chip in StatusChips)
        {
            chip.IsSelected = status is null ? chip.Text == "全部" : chip.Text == StatusText(status.Value);
        }

        ApplyFilter();
        return Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        var filtered = _board
            .Where(b => (_selectedFloor is null || b.Room.Floor == _selectedFloor)
                        && (_selectedStatus is null || b.Room.Status == _selectedStatus))
            .OrderBy(b => b.Room.Floor)
            .ThenBy(b => b.Room.RoomNo, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Rooms.Clear();
        RoomRows.Clear();
        foreach (var board in filtered)
        {
            Rooms.Add(new RoomCardViewModel(board, OpenRoomAsync));
        }

        for (var i = 0; i < Rooms.Count; i += 3)
        {
            RoomRows.Add(new RoomRowViewModel(
                Rooms[i],
                i + 1 < Rooms.Count ? Rooms[i + 1] : null,
                i + 2 < Rooms.Count ? Rooms[i + 2] : null));
        }

        Legend.Clear();
        foreach (var status in new[]
                 {
                     RoomStatus.Free, RoomStatus.Occupied, RoomStatus.Reserved,
                     RoomStatus.Cleaning, RoomStatus.Repair, RoomStatus.Disabled
                 })
        {
            var count = _board.Count(b => b.Room.Status == status);
            Legend.Add(new RoomLegendItem($"{StatusText(status)} {count}", StatusColor(status)));
        }

        SummaryText = $"共 {filtered.Count} 间房";
        IsEmpty = Rooms.Count == 0;
    }

    private Task OpenRoomAsync(RoomCardViewModel room)
    {
        SelectedRoom = room;
        IsSheetOpen = true;
        return Task.CompletedTask;
    }

    private void CloseSheet()
    {
        IsSheetOpen = false;
        SelectedRoom = null;
    }

    public static string StatusText(RoomStatus status) => status switch
    {
        RoomStatus.Free => "空闲",
        RoomStatus.Occupied => "已入住",
        RoomStatus.Reserved => "预定",
        RoomStatus.Cleaning => "打扫中",
        RoomStatus.Repair => "维修",
        _ => "停用"
    };

    public static Color StatusColor(RoomStatus status) => status switch
    {
        RoomStatus.Free => Color.FromArgb("#16A34A"),
        RoomStatus.Occupied => Color.FromArgb("#2563EB"),
        RoomStatus.Reserved => Color.FromArgb("#7C3AED"),
        RoomStatus.Cleaning => Color.FromArgb("#0891B2"),
        RoomStatus.Repair => Color.FromArgb("#D4A106"),
        _ => Color.FromArgb("#6B7280")
    };
}
