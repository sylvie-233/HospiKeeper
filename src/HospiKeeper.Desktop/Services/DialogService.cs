using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.ViewModels.Dialogs;

namespace HospiKeeper.Desktop.Services;

/// <summary>主窗口内的模态弹窗宿主。</summary>
public sealed partial class DialogService : ObservableObject
{
    [ObservableProperty]
    private DialogViewModelBase? _current;

    public DialogService()
    {
        CloseCommand = new RelayCommand(Close);
    }

    public RelayCommand CloseCommand { get; }

    public bool IsOpen => Current is not null;

    partial void OnCurrentChanged(DialogViewModelBase? value)
        => OnPropertyChanged(nameof(IsOpen));

    public void Show(DialogViewModelBase dialog) => Current = dialog;

    public void Close()
    {
        var dialog = Current;
        Current = null;
        dialog?.OnClosed();
    }

    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string okText = "确定",
        string cancelText = "取消",
        bool danger = true)
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Show(new ConfirmDialogViewModel(this, title, message, okText, cancelText, danger, completion));
        return completion.Task;
    }
}
