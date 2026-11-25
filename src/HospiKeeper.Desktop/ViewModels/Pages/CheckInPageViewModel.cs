using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Models;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;
using Sylvie233.UI.Icons;
using Avalonia.Media;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>入住登记的可选房间。</summary>
public sealed record CheckInRoomOption(Room Room, bool IsReserved)
{
    public string DisplayText =>
        $"{Room.RoomNo} · {Room.TypeName} · ¥{Room.Price:0.##}" + (IsReserved ? "（预定）" : string.Empty);
}

/// <summary>同住人输入行。</summary>
public sealed partial class CompanionRowViewModel : ObservableObject
{
    public CompanionRowViewModel(Action<CompanionRowViewModel> remove)
    {
        RemoveCommand = new RelayCommand(() => remove(this));
    }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _idCard = string.Empty;

    public IRelayCommand RemoveCommand { get; }
}

/// <summary>入住登记页面。</summary>
public sealed partial class CheckInPageViewModel : PageViewModelBase
{
    private static readonly Regex PhoneRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex IdCardRegex = new(@"^\d{17}[\dXx]$", RegexOptions.Compiled);

    private bool _applying;
    private int? _reservationId;

    public CheckInPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
        ResetCommand = new RelayCommand(Reset);
        AddCompanionCommand = new RelayCommand(() => Companions.Add(new CompanionRowViewModel(RemoveCompanion)));
        SetNowCommand = new RelayCommand(() =>
        {
            CheckInTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            if (!DateTime.TryParse(PlanOutTimeText, out var planOut) || planOut <= DateTime.Now)
            {
                PlanOutTimeText = DateTime.Today.AddDays(1).AddHours(12).ToString("yyyy-MM-dd HH:mm");
            }
        });
        SetNextDayNoonCommand = new RelayCommand(() =>
        {
            var baseDate = DateTime.TryParse(CheckInTimeText, out var ci) ? ci.Date : DateTime.Today;
            PlanOutTimeText = baseDate.AddDays(1).AddHours(12).ToString("yyyy-MM-dd HH:mm");
        });
        ResetTimes();
    }

    public override string Key => PageKeys.CheckIn;
    public override string Title => "入住登记";
    public override Geometry Icon => AntIcons.Plus;
    public override int PageOrder => 20;

    public ObservableCollection<CheckInRoomOption> RoomOptions { get; } = new();
    public ObservableCollection<CompanionRowViewModel> Companions { get; } = new();

    [ObservableProperty]
    private CheckInRoomOption? _selectedRoom;

    [ObservableProperty]
    private string _guestName = string.Empty;

    [ObservableProperty]
    private string _guestPhone = string.Empty;

    [ObservableProperty]
    private string _guestIdCard = string.Empty;

    [ObservableProperty]
    private string _checkInTimeText = string.Empty;

    [ObservableProperty]
    private string _planOutTimeText = string.Empty;

    [ObservableProperty]
    private string _priceText = string.Empty;

    [ObservableProperty]
    private string _depositText = string.Empty;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _reservationBanner;

    [ObservableProperty]
    private string _priceHint = "选择房间后自动带出标价 / 底价 / 押金";

    [ObservableProperty]
    private string _nightsText = "—";

    [ObservableProperty]
    private string _roomFeeText = "—";

    [ObservableProperty]
    private string _totalText = "—";

    [ObservableProperty]
    private string _priceWarning = string.Empty;

    [ObservableProperty]
    private string? _guestNameError;

    [ObservableProperty]
    private string? _guestPhoneError;

    [ObservableProperty]
    private string? _guestIdCardError;

    [ObservableProperty]
    private string? _roomError;

    [ObservableProperty]
    private string? _timeError;

    [ObservableProperty]
    private string? _priceError;

    [ObservableProperty]
    private string? _depositError;

    [ObservableProperty]
    private string? _companionError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IRelayCommand AddCompanionCommand { get; }
    public IRelayCommand SetNowCommand { get; }
    public IRelayCommand SetNextDayNoonCommand { get; }

    public bool HasReservation => !string.IsNullOrWhiteSpace(ReservationBanner);

    partial void OnSelectedRoomChanged(CheckInRoomOption? value)
    {
        if (_applying || value is null)
        {
            return;
        }

        PriceText = value.Room.Price.ToString("0.00");
        DepositText = value.Room.Deposit.ToString("0.00");
        PriceHint = $"标价 ¥{value.Room.Price:0.00} · 底价 ¥{value.Room.MinPrice:0.00} · 押金标准 ¥{value.Room.Deposit:0.00}";
        Recalculate();
    }

    partial void OnCheckInTimeTextChanged(string value) => Recalculate();

    partial void OnPlanOutTimeTextChanged(string value) => Recalculate();

    partial void OnPriceTextChanged(string value) => Recalculate();

    partial void OnDepositTextChanged(string value) => Recalculate();

    public override async Task OnNavigatedToAsync(object? argument)
    {
        if (argument is CheckInPrefill prefill)
        {
            await ApplyPrefillAsync(prefill);
            return;
        }

        _reservationId = null;
        ReservationBanner = null;
        OnPropertyChanged(nameof(HasReservation));
        await LoadRoomsAsync(null, null);
        if (string.IsNullOrWhiteSpace(CheckInTimeText) || string.IsNullOrWhiteSpace(PlanOutTimeText))
        {
            ResetTimes();
        }
        Recalculate();
    }

    private async Task ApplyPrefillAsync(CheckInPrefill prefill)
    {
        try
        {
            Reservation? reservation = null;
            if (prefill.ReservationId is int reservationId)
            {
                reservation = await Api.GetReservationAsync(reservationId);
                if (reservation is null)
                {
                    Toast.Warning("预订单不存在，可能已被处理。");
                }
            }

            _reservationId = reservation?.Id;
            await LoadRoomsAsync(prefill.RoomId ?? reservation?.RoomId, reservation);

            if (reservation is not null)
            {
                _applying = true;
                GuestName = reservation.GuestName;
                GuestPhone = reservation.GuestPhone;
                ReservationBanner = reservation.RoomId is null
                    ? $"来自游客预订 {reservation.ResNo} · {reservation.RoomTypeName} · {reservation.CheckInDate:MM-dd} → {reservation.CheckOutDate:MM-dd} · 请选择该房型空闲房"
                    : $"来自预定 {reservation.ResNo} · {reservation.GuestName} · {reservation.ArriveTime:MM-dd HH:mm} 到店";
                OnPropertyChanged(nameof(HasReservation));

                var arrival = reservation.ArriveTime <= DateTime.Now
                    ? DateTime.Now
                    : reservation.ArriveTime;
                CheckInTimeText = arrival.ToString("yyyy-MM-dd HH:mm");
                PlanOutTimeText = reservation.CheckOutDate.Date > reservation.CheckInDate.Date
                    ? reservation.CheckOutDate.Date.AddHours(12).ToString("yyyy-MM-dd HH:mm")
                    : arrival.Date.AddDays(1).AddHours(12).ToString("yyyy-MM-dd HH:mm");
                SelectedRoom = RoomOptions.FirstOrDefault(r => r.Room.Id == reservation.RoomId)
                               ?? SelectedRoom;
                _applying = false;
                Recalculate();
            }
        }
        catch (Exception ex)
        {
            Toast.Error($"加载预定信息失败：{ex.Message}");
        }
    }

    private async Task LoadRoomsAsync(int? preferredRoomId, Reservation? reservation)
    {
        try
        {
            var allRooms = await Api.GetRoomsAsync();
            var options = allRooms
                .Where(r => reservation is not null && reservation.RoomId is null
                    ? r.Status == RoomStatus.Free && r.TypeId == reservation.TypeId
                    : r.Status == RoomStatus.Free
                      || (reservation is not null && r.Id == reservation.RoomId))
                .OrderBy(r => r.Floor)
                .ThenBy(r => r.RoomNo, StringComparer.OrdinalIgnoreCase)
                .Select(r => new CheckInRoomOption(
                    r,
                    reservation is not null && r.Id == reservation.RoomId && r.Status == RoomStatus.Reserved))
                .ToList();

            var selectedId = preferredRoomId ?? SelectedRoom?.Room.Id;
            _applying = true;
            RoomOptions.Clear();
            foreach (var option in options)
            {
                RoomOptions.Add(option);
            }

            SelectedRoom = RoomOptions.FirstOrDefault(o => o.Room.Id == selectedId)
                           ?? RoomOptions.FirstOrDefault(o => !o.IsReserved)
                           ?? RoomOptions.FirstOrDefault();
            _applying = false;

            if (SelectedRoom is not null)
            {
                PriceText = SelectedRoom.Room.Price.ToString("0.00");
                DepositText = SelectedRoom.Room.Deposit.ToString("0.00");
                PriceHint = $"标价 ¥{SelectedRoom.Room.Price:0.00} · 底价 ¥{SelectedRoom.Room.MinPrice:0.00} · 押金标准 ¥{SelectedRoom.Room.Deposit:0.00}";
            }

            Recalculate();
        }
        catch (Exception ex)
        {
            Toast.Error($"加载空闲房间失败：{ex.Message}");
        }
    }

    private void Recalculate()
    {
        NightsText = "—";
        RoomFeeText = "—";
        TotalText = "—";
        PriceWarning = string.Empty;

        if (!DateTime.TryParse(CheckInTimeText, out var checkIn)
            || !DateTime.TryParse(PlanOutTimeText, out var planOut)
            || planOut <= checkIn)
        {
            return;
        }

        if (!decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price)
            || price <= 0)
        {
            return;
        }

        decimal.TryParse(DepositText, NumberStyles.Number, CultureInfo.CurrentCulture, out var deposit);

        var nights = BillingCalculator.CountNights(checkIn, planOut);
        var roomFee = nights * price;
        NightsText = $"{nights} 晚";
        RoomFeeText = $"¥{roomFee:0.00}";
        TotalText = $"¥{roomFee + deposit:0.00}";

        if (SelectedRoom is not null && price < SelectedRoom.Room.MinPrice)
        {
            PriceWarning = $"成交单价低于底价 ¥{SelectedRoom.Room.MinPrice:0.00}";
        }
    }

    private void ResetTimes()
    {
        CheckInTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        PlanOutTimeText = DateTime.Today.AddDays(1).AddHours(12).ToString("yyyy-MM-dd HH:mm");
    }

    private void RemoveCompanion(CompanionRowViewModel row) => Companions.Remove(row);

    private async Task SubmitAsync()
    {
        SubmitError = null;
        GuestNameError = string.IsNullOrWhiteSpace(GuestName) ? "请输入客人姓名" : null;
        GuestPhoneError = PhoneRegex.IsMatch(GuestPhone.Trim()) ? null : "请输入 11 位有效手机号";
        GuestIdCardError = IdCardRegex.IsMatch(GuestIdCard.Trim()) ? null : "请输入 18 位有效身份证号";
        RoomError = SelectedRoom is null ? "请选择入住房间" : null;

        var hasCheckIn = DateTime.TryParse(CheckInTimeText, out var checkIn);
        var hasPlanOut = DateTime.TryParse(PlanOutTimeText, out var planOut);
        var timeValid = hasCheckIn && hasPlanOut && planOut > checkIn;
        TimeError = timeValid ? null : "预计退房时间必须晚于入住时间（yyyy-MM-dd HH:mm）";

        var priceValid = decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price)
                         && price > 0;
        PriceError = !priceValid
            ? "请输入有效单价"
            : SelectedRoom is not null && price < SelectedRoom.Room.MinPrice
                ? $"成交单价不能低于底价 ¥{SelectedRoom.Room.MinPrice:0.00}"
                : null;

        var depositValid = decimal.TryParse(DepositText, NumberStyles.Number, CultureInfo.CurrentCulture, out var deposit)
                           && deposit >= 0;
        DepositError = depositValid ? null : "请输入有效押金";

        var companions = ValidateCompanions();

        if (GuestNameError is not null || GuestPhoneError is not null || GuestIdCardError is not null
            || RoomError is not null || TimeError is not null || PriceError is not null
            || DepositError is not null || CompanionError is not null)
        {
            return;
        }

        try
        {
            var record = await Api.CheckInAsync(new CheckInRequest(
                SelectedRoom!.Room.Id,
                GuestName.Trim(),
                GuestPhone.Trim(),
                GuestIdCard.Trim(),
                checkIn,
                planOut,
                price,
                deposit,
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                _reservationId,
                companions));

            Toast.Success($"入住成功：{record.CheckInNo}，房间 {record.RoomNo} 已转已入住");
            NotifyDataChanged();
            Reset();
            Navigation.Navigate(PageKeys.Dashboard);
        }
        catch (ApiException ex)
        {
            SubmitError = ex.Message;
        }
        catch (Exception ex)
        {
            SubmitError = ex.Message;
        }
    }

    private List<CompanionInput> ValidateCompanions()
    {
        CompanionError = null;
        var result = new List<CompanionInput>();

        foreach (var row in Companions)
        {
            if (string.IsNullOrWhiteSpace(row.Name)
                && string.IsNullOrWhiteSpace(row.Phone)
                && string.IsNullOrWhiteSpace(row.IdCard))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.Name))
            {
                CompanionError = "同住人姓名不能为空";
                return result;
            }

            if (!string.IsNullOrWhiteSpace(row.Phone) && !PhoneRegex.IsMatch(row.Phone.Trim()))
            {
                CompanionError = $"同住人「{row.Name}」手机号格式无效";
                return result;
            }

            if (!string.IsNullOrWhiteSpace(row.IdCard) && !IdCardRegex.IsMatch(row.IdCard.Trim()))
            {
                CompanionError = $"同住人「{row.Name}」身份证号格式无效";
                return result;
            }

            result.Add(new CompanionInput(
                row.Name.Trim(),
                string.IsNullOrWhiteSpace(row.Phone) ? null : row.Phone.Trim(),
                string.IsNullOrWhiteSpace(row.IdCard) ? null : row.IdCard.Trim().ToUpperInvariant()));
        }

        return result;
    }

    private void Reset()
    {
        _applying = true;
        GuestName = string.Empty;
        GuestPhone = string.Empty;
        GuestIdCard = string.Empty;
        Note = string.Empty;
        Companions.Clear();
        _reservationId = null;
        ReservationBanner = null;
        OnPropertyChanged(nameof(HasReservation));
        GuestNameError = null;
        GuestPhoneError = null;
        GuestIdCardError = null;
        RoomError = null;
        TimeError = null;
        PriceError = null;
        DepositError = null;
        CompanionError = null;
        SubmitError = null;
        ResetTimes();
        _applying = false;

        _ = LoadRoomsAsync(null, null);
    }
}
