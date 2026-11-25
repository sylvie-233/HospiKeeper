using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;
using Sylvie233.UI.Controls;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>入住详情弹窗。</summary>
public sealed class CheckInDetailDialogViewModel : DialogViewModelBase
{
    private readonly CheckInRecord _record;

    public CheckInDetailDialogViewModel(DialogService host, CheckInRecord record) : base(host)
    {
        _record = record;
        var end = record.ActualOutTime ?? record.PlanOutTime;
        var nights = BillingCalculator.CountNights(record.CheckInTime, end);
        RoomFeeText = $"¥{nights * record.Price:0.00}";
        NightsText = $"{nights} 晚";
        ExtraTotalText = $"¥{record.ExtraTotal:0.00}";
        TotalText = $"¥{nights * record.Price + record.ExtraTotal:0.00}";
        Extras = record.ExtraCharges
            .Select(e => new ExtraLineViewModel(e.Item, $"¥{e.Amount:0.00}"))
            .ToList();
        Companions = record.Companions
            .Select(c => string.IsNullOrWhiteSpace(c.Phone)
                ? c.Name
                : $"{c.Name} · {c.Phone}")
            .ToList();
        CloseCommand = new RelayCommand(Host.Close);
    }

    public override string Title => "入住单详情";
    public override double Width => 620;

    public string No => _record.CheckInNo;
    public string RoomNo => _record.RoomNo;
    public string RoomType => _record.RoomTypeName;
    public string GuestName => _record.GuestName;
    public string GuestPhone => _record.GuestPhone;
    public string GuestIdCard => _record.GuestIdCard;
    public string CheckInText => _record.CheckInTime.ToString("yyyy-MM-dd HH:mm");
    public string PlanOutText => _record.PlanOutTime.ToString("yyyy-MM-dd HH:mm");
    public string ActualOutText => _record.ActualOutTime?.ToString("yyyy-MM-dd HH:mm") ?? "在住中";
    public string PriceText => $"¥{_record.Price:0.00}";
    public string DepositText => $"¥{_record.Deposit:0.00}";
    public string OperatorName => _record.OperatorName;
    public string NoteText => string.IsNullOrWhiteSpace(_record.Note) ? "无" : _record.Note;
    public string NightsText { get; }
    public string RoomFeeText { get; }
    public string ExtraTotalText { get; }
    public string TotalText { get; }
    public IReadOnlyList<ExtraLineViewModel> Extras { get; }
    public IReadOnlyList<string> Companions { get; }
    public bool HasExtras => Extras.Count > 0;
    public bool HasCompanions => Companions.Count > 0;

    public string StatusText => _record.Status == CheckInStatus.Living ? "在住" : "已退房";
    public AntTagVariant StatusVariant =>
        _record.Status == CheckInStatus.Living ? AntTagVariant.Processing : AntTagVariant.Success;

    public IRelayCommand CloseCommand { get; }
}
