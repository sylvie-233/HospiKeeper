using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>房型新增 / 编辑弹窗。</summary>
public sealed partial class RoomTypeEditDialogViewModel : DialogViewModelBase
{
    private readonly IHospiKeeperApi _api;
    private readonly ToastService _toast;
    private readonly RoomType? _roomType;
    private readonly Func<Task> _onSaved;

    public RoomTypeEditDialogViewModel(
        DialogService host,
        ToastService toast,
        IHospiKeeperApi api,
        RoomType? roomType,
        Func<Task> onSaved) : base(host)
    {
        _toast = toast;
        _api = api;
        _roomType = roomType;
        _onSaved = onSaved;

        if (roomType is not null)
        {
            Name = roomType.Name;
            PriceText = roomType.BasePrice.ToString("0.00");
            Note = roomType.Note ?? string.Empty;
        }

        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
    }

    public override string Title => _roomType is null ? "新增房型" : "编辑房型";
    public override double Width => 460;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _priceText = "0.00";

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _nameError;

    [ObservableProperty]
    private string? _priceError;

    [ObservableProperty]
    private string? _submitError;

    public IAsyncRelayCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        NameError = string.IsNullOrWhiteSpace(Name) ? "请输入房型名称" : null;
        var priceValid = decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price)
                         && price >= 0;
        PriceError = priceValid ? null : "请输入有效的基础定价";
        SubmitError = null;

        if (NameError is not null || PriceError is not null)
        {
            return;
        }

        try
        {
            var input = new RoomTypeInput(Name.Trim(), price,
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim());

            if (_roomType is null)
            {
                await _api.CreateRoomTypeAsync(input);
                _toast.Success($"房型「{input.Name}」已创建");
            }
            else
            {
                await _api.UpdateRoomTypeAsync(_roomType.Id, input);
                _toast.Success($"房型「{input.Name}」已更新");
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
