using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Models;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Domain.Enums;
using Sylvie233.UI.Controls;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>预定状态筛选项。</summary>
public sealed record ReservationStatusOption(ReservationStatus? Value, string Text);

/// <summary>预定表格行。</summary>
public sealed class ReservationRowViewModel
{
    public ReservationRowViewModel(
        Reservation reservation,
        ReservationsPageViewModel page)
    {
        Reservation = reservation;
        ConvertCommand = new RelayCommand(() => page.Convert(this));
        CancelCommand = new AsyncRelayCommand(() => page.CancelAsync(this));
        DetailCommand = new RelayCommand(() => page.ShowDetail(this));
    }

    public Reservation Reservation { get; }
    public string ResNo => Reservation.ResNo;
    public string GuestName => Reservation.GuestName;
    public string GuestPhone => Reservation.GuestPhone;
    public string RoomText => string.IsNullOrWhiteSpace(Reservation.RoomNo)
        ? $"{Reservation.RoomTypeName} · 到店分配"
        : $"{Reservation.RoomNo} · {Reservation.RoomTypeName}";
    public string DateText => $"{Reservation.CheckInDate:MM-dd} → {Reservation.CheckOutDate:MM-dd}";
    public string ArriveText => Reservation.ArriveTime.ToString("MM-dd HH:mm");
    public string NoteText => string.IsNullOrWhiteSpace(Reservation.Note) ? "—" : Reservation.Note;
    public string OperatorName => Reservation.OperatorName;
    public string CheckInText => string.IsNullOrWhiteSpace(Reservation.CheckInNo) ? "—" : Reservation.CheckInNo;
    public bool IsPending => Reservation.Status == ReservationStatus.Pending;

    public string StatusText => Reservation.Status switch
    {
        ReservationStatus.Pending => "待到店",
        ReservationStatus.Converted => "已转入住",
        _ => "已取消"
    };

    public AntTagVariant StatusVariant => Reservation.Status switch
    {
        ReservationStatus.Pending => AntTagVariant.Warning,
        ReservationStatus.Converted => AntTagVariant.Success,
        _ => AntTagVariant.Default
    };

    public IRelayCommand ConvertCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }
    public IRelayCommand DetailCommand { get; }
}

/// <summary>预定管理页面。</summary>
public sealed partial class ReservationsPageViewModel : PageViewModelBase
{
    public ReservationsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        StatusOptions = new ObservableCollection<ReservationStatusOption>
        {
            new(null, "全部状态"),
            new(ReservationStatus.Pending, "待到店"),
            new(ReservationStatus.Converted, "已转入住"),
            new(ReservationStatus.Cancelled, "已取消")
        };
        _selectedStatus = StatusOptions[0];
        AddCommand = new AsyncRelayCommand(AddAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public override string Key => PageKeys.Reservations;
    public override string Title => "预定管理";
    public override Geometry Icon => AntIcons.Calendar;
    public override int PageOrder => 30;

    public ObservableCollection<ReservationRowViewModel> Rows { get; } = new();
    public ObservableCollection<ReservationStatusOption> StatusOptions { get; }

    [ObservableProperty]
    private ReservationStatusOption _selectedStatus;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private string _countText = "共 0 条";

    public IAsyncRelayCommand AddCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    partial void OnSelectedStatusChanged(ReservationStatusOption value) => _ = LoadAsync();

    partial void OnKeywordChanged(string value) => _ = LoadAsync();

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();


    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var list = await Api.GetReservationsAsync(SelectedStatus.Value);
            var keyword = Keyword.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(r =>
                    r.GuestName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || r.GuestPhone.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || (r.RoomNo?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                    || r.ResNo.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            Rows.Clear();
            foreach (var reservation in list)
            {
                Rows.Add(new ReservationRowViewModel(reservation, this));
            }

            CountText = $"共 {Rows.Count} 条";
        });
    }

    internal void Convert(ReservationRowViewModel row)
    {
        if (!row.IsPending)
        {
            Toast.Warning("该预定已处理。");
            return;
        }

        Navigation.Navigate(PageKeys.CheckIn, new CheckInPrefill(row.Reservation.RoomId, row.Reservation.Id));
    }

    internal async Task CancelAsync(ReservationRowViewModel row)
    {
        if (!row.IsPending)
        {
            return;
        }

        var confirmed = await Dialog.ConfirmAsync(
            "取消预定",
            $"确定取消预定 {row.ResNo}（{row.GuestName}）吗？房间将恢复空闲。",
            "取消预定",
            "返回");
        if (!confirmed)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await Api.CancelReservationAsync(row.Reservation.Id);
            await LoadAsync();
        }, $"预定 {row.ResNo} 已取消");
    }

    internal void ShowDetail(ReservationRowViewModel row)
        => Dialog.Show(new ReservationDetailDialogViewModel(Dialog, row.Reservation));

    private async Task AddAsync()
    {
        try
        {
            var freeRooms = await Api.GetFreeRoomsAsync();
            Dialog.Show(new NewReservationDialogViewModel(
                Dialog, Toast, Api, freeRooms, null, async () =>
                {
                    await LoadAsync();
                    NotifyDataChanged();
                }));
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
        }
    }
}
