using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed partial class RoomEditDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly Room? _room;
    private readonly Func<Task> _onSaved;

    public RoomEditDialogViewModel(
        DialogService host,
        ToastService toast,
        IHospiKeeperApi api,
        Room? room,
        IReadOnlyList<RoomType> roomTypes,
        Func<Task> onSaved) : base(host)
    {
        _toast = toast;
        _api = api;
        _room = room;
        _onSaved = onSaved;

        foreach (var type in roomTypes)
        {
            RoomTypes.Add(type);
        }

        _selectedRoomType = room is null
            ? RoomTypes.FirstOrDefault()
            : RoomTypes.FirstOrDefault(t => t.Id == room.TypeId) ?? RoomTypes.FirstOrDefault();

        if (room is not null)
        {
            RoomNo = room.RoomNo;
            FloorText = room.Floor.ToString();
            BedsText = room.Beds.ToString();
            PriceText = room.Price.ToString("0.00");
            MinPriceText = room.MinPrice.ToString("0.00");
            DepositText = room.Deposit.ToString("0.00");
            HasWindow = room.HasWindow;
            HasBath = room.HasBath;
            Note = room.Note ?? string.Empty;
        }

        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
    }

    public override string Title => _room is null ? "新增房间" : "编辑房间";
    public override double Width => 620;

    public ObservableCollection<RoomType> RoomTypes { get; } = new();

    [ObservableProperty]
    private RoomType? _selectedRoomType;

    [ObservableProperty]
    private string _roomNo = string.Empty;

    [ObservableProperty]
    private string _floorText = "1";

    [ObservableProperty]
    private string _bedsText = "1";

    [ObservableProperty]
    private string _priceText = string.Empty;

    [ObservableProperty]
    private string _minPriceText = string.Empty;

    [ObservableProperty]
    private string _depositText = "0.00";

    [ObservableProperty]
    private bool _hasWindow = true;

    [ObservableProperty]
    private bool _hasBath = true;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _roomNoError;

    [ObservableProperty]
    private string? _floorError;

    [ObservableProperty]
    private string? _bedsError;

    [ObservableProperty]
    private string? _typeError;

    [ObservableProperty]
    private string? _priceError;

    [ObservableProperty]
    private string? _minPriceError;

    [ObservableProperty]
    private string? _depositError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        RoomNoError = string.IsNullOrWhiteSpace(RoomNo) ? "请输入房号" : null;
        var floorValid = int.TryParse(FloorText, out var floor) && floor is >= 1 and <= 99;
        FloorError = floorValid ? null : "楼层需在 1 - 99 之间";
        var bedsValid = int.TryParse(BedsText, out var beds) && beds is >= 1 and <= 10;
        BedsError = bedsValid ? null : "床位数需在 1 - 10 之间";
        TypeError = SelectedRoomType is null ? "请选择房型" : null;
        var priceValid = decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price)
                         && price > 0;
        PriceError = priceValid ? null : "请输入有效的标价";
        var minValid = decimal.TryParse(MinPriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var minPrice)
                       && minPrice >= 0;
        MinPriceError = minValid ? null : "请输入有效的底价";
        var depositValid = decimal.TryParse(DepositText, NumberStyles.Number, CultureInfo.CurrentCulture, out var deposit)
                           && deposit >= 0;
        DepositError = depositValid ? null : "请输入有效的押金";
        SubmitError = null;

        if (priceValid && minValid && price < minPrice)
        {
            PriceError = "标价不能低于底价";
        }

        if (RoomNoError is not null || FloorError is not null || BedsError is not null
            || TypeError is not null || PriceError is not null || MinPriceError is not null
            || DepositError is not null)
        {
            return;
        }

        try
        {
            var input = new RoomInput(
                RoomNo.Trim(),
                floor,
                SelectedRoomType!.Id,
                beds,
                price,
                minPrice,
                deposit,
                HasWindow,
                HasBath,
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim());

            if (_room is null)
            {
                await _api.CreateRoomAsync(input);
                _toast.Success($"房间 {input.RoomNo} 已创建");
            }
            else
            {
                await _api.UpdateRoomAsync(_room.Id, input);
                _toast.Success($"房间 {input.RoomNo} 已更新");
            }

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
