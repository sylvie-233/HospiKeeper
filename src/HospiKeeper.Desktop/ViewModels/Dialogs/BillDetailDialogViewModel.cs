using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed class BillDetailDialogViewModel : DialogViewModelBase
{
    private readonly CheckoutBill _bill;

    public BillDetailDialogViewModel(DialogService host, CheckoutBill bill) : base(host)
    {
        _bill = bill;
        Extras = bill.Extras
            .Select(e => new ExtraLineViewModel(e.Item, $"¥{e.Amount:0.00}"))
            .ToList();
        CloseCommand = new RelayCommand(Host.Close);
    }

    public override string Title => "退房账单详情";
    public override double Width => 620;

    public string No => _bill.BillNo;
    public string CheckInNo => string.IsNullOrWhiteSpace(_bill.CheckInNo) ? "—" : _bill.CheckInNo;
    public string RoomNo => _bill.RoomNo;
    public string RoomType => _bill.RoomTypeName;
    public string GuestName => _bill.GuestName;
    public string CheckInText => _bill.CheckInTime.ToString("yyyy-MM-dd HH:mm");
    public string CheckoutText => _bill.CheckoutTime.ToString("yyyy-MM-dd HH:mm");
    public string CreatedText => _bill.CreatedAt.ToString("yyyy-MM-dd HH:mm");
    public string RoomFeeText => $"¥{_bill.RoomFee:0.00}";
    public string ExtraTotalText => $"¥{_bill.ExtraTotal:0.00}";
    public string DamageText => $"¥{_bill.Damage:0.00}";
    public string DepositText => $"¥{_bill.DepositReceived:0.00}";
    public string TotalText => $"¥{_bill.Total:0.00}";
    public string DiffText => _bill.Diff >= 0 ? $"¥{_bill.Diff:0.00}" : $"-¥{-_bill.Diff:0.00}";
    public string OperatorName => _bill.OperatorName;
    public string NoteText => string.IsNullOrWhiteSpace(_bill.Note) ? "无" : _bill.Note;
    public string UnitsText => _bill.Mode == BillingMode.Hourly
        ? $"加钟 {_bill.Nights} 小时"
        : $"{_bill.Nights} 晚";

    public IReadOnlyList<ExtraLineViewModel> Extras { get; }
    public bool HasExtras => Extras.Count > 0;
    public bool NeedsPayment => _bill.Diff > 0;
    public bool NeedsRefund => _bill.Diff < 0;
    public bool IsEven => _bill.Diff == 0;

    public string ResultText => _bill.Diff > 0
        ? $"应补收 ¥{_bill.Diff:0.00}"
        : _bill.Diff < 0
            ? $"应退 ¥{-_bill.Diff:0.00}"
            : "押金正好抵扣";

    public IRelayCommand CloseCommand { get; }
}
