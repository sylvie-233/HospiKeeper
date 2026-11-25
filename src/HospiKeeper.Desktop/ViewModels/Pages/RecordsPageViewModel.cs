using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Desktop.ViewModels.Dialogs;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;
using Sylvie233.UI.Controls;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

public sealed class CheckInRowViewModel
{
    public CheckInRowViewModel(CheckInRecord record, RecordsPageViewModel page)
    {
        Record = record;
        var end = record.ActualOutTime ?? record.PlanOutTime;
        var nights = BillingCalculator.CountNights(record.CheckInTime, end);
        FeeText = $"¥{nights * record.Price:0.00}";
        DetailCommand = new RelayCommand(() => page.ShowCheckInDetail(this));
    }

    public CheckInRecord Record { get; }
    public string No => Record.CheckInNo;
    public string RoomNo => Record.RoomNo;
    public string GuestName => Record.GuestName;
    public string CheckInText => Record.CheckInTime.ToString("MM-dd HH:mm");
    public string OutText => Record.ActualOutTime?.ToString("MM-dd HH:mm") ?? "在住";
    public string FeeText { get; }
    public string OperatorName => Record.OperatorName;
    public string StatusText => Record.Status == CheckInStatus.Living ? "在住" : "已退房";
    public AntTagVariant StatusVariant =>
        Record.Status == CheckInStatus.Living ? AntTagVariant.Processing : AntTagVariant.Success;
    public IRelayCommand DetailCommand { get; }
}

public sealed class BillRowViewModel
{
    public BillRowViewModel(CheckoutBill bill, RecordsPageViewModel page)
    {
        Bill = bill;
        DetailCommand = new RelayCommand(() => page.ShowBillDetail(this));
    }

    public CheckoutBill Bill { get; }
    public string No => Bill.BillNo;
    public string RoomNo => Bill.RoomNo;
    public string GuestName => Bill.GuestName;
    public string RoomFeeText => $"¥{Bill.RoomFee:0.00}";
    public string ExtraText => $"¥{Bill.ExtraTotal + Bill.Damage:0.00}";
    public string DepositText => $"¥{Bill.DepositReceived:0.00}";
    public string DiffText => Bill.Diff >= 0 ? $"¥{Bill.Diff:0.00}" : $"-¥{-Bill.Diff:0.00}";
    public string CheckoutText => Bill.CheckoutTime.ToString("MM-dd HH:mm");
    public string OperatorName => Bill.OperatorName;
    public bool NeedsPayment => Bill.Diff > 0;
    public bool NeedsRefund => Bill.Diff < 0;
    public IRelayCommand DetailCommand { get; }
}

public sealed partial class RecordsPageViewModel : PageViewModelBase
{
    public RecordsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        QueryCommand = new AsyncRelayCommand(LoadAsync);
        ResetCommand = new RelayCommand(ResetFilters);
        ShowCheckInsCommand = new RelayCommand(() => SwitchTab(true));
        ShowBillsCommand = new RelayCommand(() => SwitchTab(false));
    }

    public override string Key => PageKeys.Records;
    public override string Title => "入住 / 账单记录";
    public override Geometry Icon => AntIcons.Records;
    public override string Group => "记录与统计";
    public override int PageOrder => 50;

    public ObservableCollection<CheckInRowViewModel> CheckInRows { get; } = new();
    public ObservableCollection<BillRowViewModel> BillRows { get; } = new();

    [ObservableProperty]
    private bool _isCheckInTab = true;

    [ObservableProperty]
    private string _roomNo = string.Empty;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    [ObservableProperty]
    private string _countText = string.Empty;

    public IAsyncRelayCommand QueryCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IRelayCommand ShowCheckInsCommand { get; }
    public IRelayCommand ShowBillsCommand { get; }

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();


    private void SwitchTab(bool checkIn)
    {
        IsCheckInTab = checkIn;
        _ = LoadAsync();
    }

    private void ResetFilters()
    {
        RoomNo = string.Empty;
        Keyword = string.Empty;
        DateFrom = null;
        DateTo = null;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var query = new RecordQuery(
                string.IsNullOrWhiteSpace(RoomNo) ? null : RoomNo.Trim(),
                string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim(),
                DateFrom,
                DateTo);

            if (IsCheckInTab)
            {
                var records = await Api.QueryCheckInsAsync(query);
                CheckInRows.Clear();
                foreach (var record in records)
                {
                    CheckInRows.Add(new CheckInRowViewModel(record, this));
                }

                CountText = $"入住记录 · 共 {CheckInRows.Count} 条";
            }
            else
            {
                var bills = await Api.QueryBillsAsync(query);
                BillRows.Clear();
                foreach (var bill in bills)
                {
                    BillRows.Add(new BillRowViewModel(bill, this));
                }

                CountText = $"退房账单 · 共 {BillRows.Count} 条";
            }
        });
    }

    internal void ShowCheckInDetail(CheckInRowViewModel row)
        => Dialog.Show(new CheckInDetailDialogViewModel(Dialog, row.Record));

    internal void ShowBillDetail(BillRowViewModel row)
        => Dialog.Show(new BillDetailDialogViewModel(Dialog, row.Bill));
}
