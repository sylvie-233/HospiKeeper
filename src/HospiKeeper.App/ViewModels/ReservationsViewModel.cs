using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.App.Services;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.App.ViewModels;

public sealed class ReservationCardViewModel
{
    public ReservationCardViewModel(
        ReservationDto reservation,
        Func<ReservationCardViewModel, Task> open,
        Func<ReservationCardViewModel, Task> cancel)
    {
        Reservation = reservation;
        OpenCommand = new AsyncRelayCommand(() => open(this));
        CancelCommand = new AsyncRelayCommand(() => cancel(this));
        StatusColor = reservation.Status switch
        {
            ReservationStatus.Pending => Color.FromArgb("#7C3AED"),
            ReservationStatus.Converted => Color.FromArgb("#2563EB"),
            _ => Color.FromArgb("#6B7280")
        };
        ChannelColor = reservation.Channel == ReservationChannel.Web
            ? Color.FromArgb("#EA580C")
            : Color.FromArgb("#2563EB");
    }

    public ReservationDto Reservation { get; }
    public string ResNo => Reservation.ResNo;
    public string GuestName => Reservation.GuestName;
    public string GuestPhone => Reservation.GuestPhone;
    public string RoomTypeName => Reservation.RoomTypeName;
    public string RoomText => string.IsNullOrWhiteSpace(Reservation.RoomNo)
        ? "未分配"
        : Reservation.RoomNo!;
    public string DateText => $"{Reservation.CheckInDate:MM-dd} → {Reservation.CheckOutDate:MM-dd}";
    public string ArriveText => Reservation.ArriveTime.ToString("MM-dd HH:mm");
    public string ChannelText => Reservation.Channel == ReservationChannel.Web ? "游客在线" : "前台代订";
    public Color ChannelColor { get; }
    public Color StatusColor { get; }
    public bool IsPending => Reservation.Status == ReservationStatus.Pending;
    public string StatusText => Reservation.Status switch
    {
        ReservationStatus.Pending => "待到店",
        ReservationStatus.Converted => "已转入住",
        _ => "已取消"
    };
    public string NoteText => string.IsNullOrWhiteSpace(Reservation.Note) ? "—" : Reservation.Note!;
    public string CheckInText => string.IsNullOrWhiteSpace(Reservation.CheckInNo) ? "—" : Reservation.CheckInNo!;
    public IAsyncRelayCommand OpenCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
}

/// <summary>预定管理：状态筛选、详情与远程取消。</summary>
public sealed partial class ReservationsViewModel : BaseViewModel
{
    private ReservationStatus? _status;

    public ReservationsViewModel(ApiProvider apiProvider, AppSession session, ToastService toast)
        : base(apiProvider, session, toast)
    {
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        CloseSheetCommand = new RelayCommand(CloseSheet);
        BuildStatusChips();
    }

    [ObservableProperty] private bool _isSheetOpen;
    [ObservableProperty] private ReservationCardViewModel? _selected;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string _summaryText = "共 0 条预定";

    public ObservableCollection<ReservationCardViewModel> Reservations { get; } = [];
    public ObservableCollection<FilterChipViewModel> StatusChips { get; } = [];
    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand CloseSheetCommand { get; }

    public override Task OnAppearingAsync() => LoadAsync();

    private void BuildStatusChips()
    {
        AddChip("全部", null, true);
        AddChip("待到店", ReservationStatus.Pending);
        AddChip("已转入住", ReservationStatus.Converted);
        AddChip("已取消", ReservationStatus.Cancelled);
    }

    private void AddChip(string text, ReservationStatus? status, bool selected = false)
        => StatusChips.Add(new FilterChipViewModel(text, () => SelectStatus(status))
        {
            IsSelected = selected
        });

    private async Task SelectStatus(ReservationStatus? status)
    {
        _status = status;
        foreach (var chip in StatusChips)
        {
            chip.IsSelected = status is null ? chip.Text == "全部" : chip.Text == StatusText(status.Value);
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
        => await ExecuteAsync(async () =>
        {
            var list = await Api.GetReservationsAsync(_status);
            Reservations.Clear();
            foreach (var item in list)
            {
                Reservations.Add(new ReservationCardViewModel(item, OpenAsync, CancelAsync));
            }

            IsEmpty = Reservations.Count == 0;
            SummaryText = $"共 {Reservations.Count} 条预定";
            MarkSynced();
        });

    private Task OpenAsync(ReservationCardViewModel card)
    {
        Selected = card;
        IsSheetOpen = true;
        return Task.CompletedTask;
    }

    private void CloseSheet()
    {
        IsSheetOpen = false;
        Selected = null;
    }

    private async Task CancelAsync(ReservationCardViewModel card)
    {
        if (!card.IsPending)
        {
            return;
        }

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return;
        }

        var confirmed = await page.DisplayAlertAsync(
            "取消预定",
            $"确定取消预定 {card.ResNo}（{card.GuestName}）吗？\n取消后该房型日期段库存自动回补。",
            "确认取消",
            "返回");
        if (!confirmed)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            await Api.CancelReservationAsync(card.Reservation.Id);
            CloseSheet();
        }, $"预定 {card.ResNo} 已取消");
        await LoadAsync();
    }

    private static string StatusText(ReservationStatus status) => status switch
    {
        ReservationStatus.Pending => "待到店",
        ReservationStatus.Converted => "已转入住",
        _ => "已取消"
    };
}
