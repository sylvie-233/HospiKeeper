using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Sylvie233.UI.Controls;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed class ReservationDetailDialogViewModel : DialogViewModelBase
{
    private readonly Reservation _reservation;

    public ReservationDetailDialogViewModel(DialogService host, Reservation reservation) : base(host)
    {
        _reservation = reservation;
        CloseCommand = new RelayCommand(Host.Close);
    }

    public override string Title => "预定详情";
    public override double Width => 520;

    public string ResNo => _reservation.ResNo;
    public string GuestName => _reservation.GuestName;
    public string GuestPhone => _reservation.GuestPhone;
    public string RoomText => $"{_reservation.RoomNo} · {_reservation.RoomTypeName}";
    public string ArriveText => _reservation.ArriveTime.ToString("yyyy-MM-dd HH:mm");
    public string CreatedText => _reservation.CreatedAt.ToString("yyyy-MM-dd HH:mm");
    public string OperatorName => _reservation.OperatorName;
    public string NoteText => string.IsNullOrWhiteSpace(_reservation.Note) ? "—" : _reservation.Note;
    public string CheckInText => string.IsNullOrWhiteSpace(_reservation.CheckInNo) ? "—" : _reservation.CheckInNo;

    public string StatusText => _reservation.Status switch
    {
        ReservationStatus.Pending => "待到店",
        ReservationStatus.Converted => "已转入住",
        _ => "已取消"
    };

    public AntTagVariant StatusVariant => _reservation.Status switch
    {
        ReservationStatus.Pending => AntTagVariant.Warning,
        ReservationStatus.Converted => AntTagVariant.Success,
        _ => AntTagVariant.Default
    };

    public IRelayCommand CloseCommand { get; }
}
