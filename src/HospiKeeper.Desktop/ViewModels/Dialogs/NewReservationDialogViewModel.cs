using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed record RoomOptionViewModel(Room Room)
{
    public string DisplayText => $"{Room.RoomNo} · {Room.TypeName} · ¥{Room.Price:0.##}";
}

public sealed partial class NewReservationDialogViewModel : DialogViewModelBase
{
    private static readonly Regex PhoneRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly Func<Task> _onSaved;

    public NewReservationDialogViewModel(
        DialogService host,
        ToastService toast,
        IHospiKeeperApi api,
        IReadOnlyList<Room> freeRooms,
        int? preselectedRoomId,
        Func<Task> onSaved) : base(host)
    {
        _toast = toast;
        _api = api;
        _onSaved = onSaved;
        SubmitCommand = new AsyncRelayCommand(SubmitAsync);

        foreach (var room in freeRooms.OrderBy(r => r.Floor).ThenBy(r => r.RoomNo))
        {
            RoomOptions.Add(new RoomOptionViewModel(room));
        }

        SelectedRoom = RoomOptions.FirstOrDefault(r => r.Room.Id == preselectedRoomId)
                       ?? RoomOptions.FirstOrDefault();
    }

    public override string Title => "新增预定";
    public override double Width => 560;

    public ObservableCollection<RoomOptionViewModel> RoomOptions { get; } = new();

    [ObservableProperty]
    private RoomOptionViewModel? _selectedRoom;

    [ObservableProperty]
    private string _guestName = string.Empty;

    [ObservableProperty]
    private string _guestPhone = string.Empty;

    [ObservableProperty]
    private string _arriveTimeText = DateTime.Now.AddHours(2).ToString("yyyy-MM-dd HH:00");

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _guestNameError;

    [ObservableProperty]
    private string? _guestPhoneError;

    [ObservableProperty]
    private string? _roomError;

    [ObservableProperty]
    private string? _arriveTimeError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        GuestNameError = string.IsNullOrWhiteSpace(GuestName) ? "请输入客人姓名" : null;
        GuestPhoneError = PhoneRegex.IsMatch(GuestPhone.Trim()) ? null : "请输入 11 位有效手机号";
        RoomError = SelectedRoom is null ? "请选择预定房间" : null;
        ArriveTimeError = DateTime.TryParse(ArriveTimeText, out _) ? null : "请使用 yyyy-MM-dd HH:mm 格式";
        SubmitError = null;

        if (GuestNameError is not null || GuestPhoneError is not null
            || RoomError is not null || ArriveTimeError is not null)
        {
            return;
        }

        try
        {
            var reservation = await _api.CreateReservationAsync(new ReservationRequest(
                GuestName.Trim(),
                GuestPhone.Trim(),
                SelectedRoom!.Room.Id,
                DateTime.Parse(ArriveTimeText),
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()));

            _toast.Success($"预定成功：{reservation.ResNo}");
            Host.Close();
            await _onSaved();
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
}
