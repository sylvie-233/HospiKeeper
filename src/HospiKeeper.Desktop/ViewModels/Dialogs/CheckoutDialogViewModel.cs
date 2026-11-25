using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>计费方式下拉选项。</summary>
public sealed record BillingModeOption(BillingMode Value, string Text);

/// <summary>账单消费明细展示行。</summary>
public sealed record ExtraLineViewModel(string Item, string AmountText);

/// <summary>退房结算弹窗。</summary>
public sealed partial class CheckoutDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly CheckInRecord _record;
    private readonly Func<Task> _onSaved;

    public CheckoutDialogViewModel(
        DialogService host,
        ToastService toast,
        IHospiKeeperApi api,
        CheckInRecord record,
        Func<Task> onSaved) : base(host)
    {
        _toast = toast;
        _api = api;
        _record = record;
        _onSaved = onSaved;

        Modes = new ObservableCollection<BillingModeOption>
        {
            new(BillingMode.Daily, "按天计费"),
            new(BillingMode.Hourly, "加钟（按小时）")
        };
        _selectedMode = Modes[0];

        Extras = record.ExtraCharges
            .Select(e => new ExtraLineViewModel(e.Item, $"¥{e.Amount:0.00}"))
            .ToList();
        ExtraTotalText = $"¥{record.ExtraTotal:0.00}";

        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
        SetNowCommand = new RelayCommand(() => ActualOutText = DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        var defaultOut = DateTime.Now > record.CheckInTime ? DateTime.Now : record.CheckInTime.AddHours(1);
        ActualOutText = defaultOut.ToString("yyyy-MM-dd HH:mm");
        Recalculate();
    }

    public override string Title => "退房结算";
    public override double Width => 640;

    public string RoomNo => _record.RoomNo;
    public string RoomTypeName => _record.RoomTypeName;
    public string GuestName => _record.GuestName;
    public string GuestPhone => _record.GuestPhone;
    public string CheckInText => _record.CheckInTime.ToString("yyyy-MM-dd HH:mm");
    public string PlanOutText => _record.PlanOutTime.ToString("yyyy-MM-dd HH:mm");
    public string PriceText => $"¥{_record.Price:0.00}";
    public string DepositText => $"¥{_record.Deposit:0.00}";
    public string CheckInNo => _record.CheckInNo;

    public IReadOnlyList<ExtraLineViewModel> Extras { get; }
    public bool HasExtras => Extras.Count > 0;
    public string ExtraTotalText { get; }

    public ObservableCollection<BillingModeOption> Modes { get; }

    [ObservableProperty]
    private BillingModeOption _selectedMode;

    [ObservableProperty]
    private string _actualOutText = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    [ObservableProperty]
    private string _damageText = "0.00";

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string _roomFeeText = "—";

    [ObservableProperty]
    private string _totalText = "—";

    [ObservableProperty]
    private string _depositPreviewText = "—";

    [ObservableProperty]
    private string _resultText = "—";

    [ObservableProperty]
    private string _feeNote = string.Empty;

    [ObservableProperty]
    private bool _needsPayment;

    [ObservableProperty]
    private bool _needsRefund;

    [ObservableProperty]
    private bool _isEven;

    [ObservableProperty]
    private string? _actualOutError;

    [ObservableProperty]
    private string? _damageError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }
    public IRelayCommand SetNowCommand { get; }

    partial void OnActualOutTextChanged(string value) => Recalculate();

    partial void OnDamageTextChanged(string value) => Recalculate();

    partial void OnSelectedModeChanged(BillingModeOption value) => Recalculate();

    private void Recalculate()
    {
        ActualOutError = null;
        DamageError = null;

        if (!DateTime.TryParse(ActualOutText, out var actualOut))
        {
            ActualOutError = "请使用 yyyy-MM-dd HH:mm 格式";
            ClearPreview();
            return;
        }

        if (actualOut <= _record.CheckInTime)
        {
            ActualOutError = $"实际退房时间必须晚于入住时间 {_record.CheckInTime:yyyy-MM-dd HH:mm}";
            ClearPreview();
            return;
        }

        if (!decimal.TryParse(DamageText, NumberStyles.Number, CultureInfo.CurrentCulture, out var damage)
            || damage < 0)
        {
            DamageError = "损坏扣费不能为负数";
            ClearPreview();
            return;
        }

        try
        {
            var billing = BillingCalculator.Calculate(
                _record.CheckInTime,
                actualOut,
                _record.Price,
                _record.Deposit,
                _record.ExtraTotal,
                damage,
                SelectedMode.Value);

            RoomFeeText = $"¥{billing.RoomFee:0.00}";
            DepositPreviewText = $"¥{billing.Deposit:0.00}";
            TotalText = $"¥{billing.Total:0.00}";
            NeedsPayment = billing.NeedsPayment;
            NeedsRefund = billing.NeedsRefund;
            IsEven = !NeedsPayment && !NeedsRefund;
            ResultText = billing.Difference > 0
                ? $"应补收 ¥{billing.Difference:0.00}"
                : billing.Difference < 0
                    ? $"应退 ¥{-billing.Difference:0.00}"
                    : "押金正好抵扣";
            FeeNote = billing.Mode == BillingMode.Daily
                ? $"按天计费：{billing.Units} 晚 × ¥{billing.UnitPrice:0.00}/晚（跨日不足一晚按一晚计）"
                : $"加钟计费：{billing.Units} 小时，按 标价 ÷ 24 折算";
        }
        catch (Exception ex)
        {
            ActualOutError = ex.Message;
            ClearPreview();
        }
    }

    private void ClearPreview()
    {
        RoomFeeText = "—";
        TotalText = "—";
        DepositPreviewText = "—";
        ResultText = "—";
        FeeNote = string.Empty;
        NeedsPayment = false;
        NeedsRefund = false;
        IsEven = false;
    }

    private async Task SubmitAsync()
    {
        Recalculate();
        SubmitError = null;

        if (ActualOutError is not null || DamageError is not null)
        {
            return;
        }

        try
        {
            var bill = await _api.CheckoutAsync(new CheckoutRequest(
                _record.Id,
                DateTime.Parse(ActualOutText),
                decimal.Parse(DamageText, NumberStyles.Number, CultureInfo.CurrentCulture),
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                SelectedMode.Value));

            _toast.Success($"退房完成：{bill.BillNo}，房间 {_record.RoomNo} 已转为待打扫");
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
