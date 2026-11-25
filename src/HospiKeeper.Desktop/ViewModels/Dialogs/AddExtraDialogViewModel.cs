using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>追加消费弹窗。</summary>
public sealed partial class AddExtraDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly CheckInRecord _record;
    private readonly Func<Task> _onSaved;

    public AddExtraDialogViewModel(
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
        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
    }

    public override string Title => "追加消费";
    public override double Width => 460;

    public string RoomGuestText => $"{_record.RoomNo} · {_record.GuestName}";
    public string CheckInNoText => _record.CheckInNo;

    [ObservableProperty]
    private string _item = string.Empty;

    [ObservableProperty]
    private string _amountText = string.Empty;

    [ObservableProperty]
    private string? _itemError;

    [ObservableProperty]
    private string? _amountError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        ItemError = string.IsNullOrWhiteSpace(Item) ? "请输入消费项目" : null;
        var validAmount = decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount)
                          && amount > 0;
        AmountError = validAmount ? null : "请输入大于 0 的金额";
        SubmitError = null;

        if (ItemError is not null || AmountError is not null)
        {
            return;
        }

        try
        {
            await _api.AddExtraChargeAsync(_record.Id, Item.Trim(), amount);
            _toast.Success($"已入账：{Item.Trim()} ¥{amount:0.00}");
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
