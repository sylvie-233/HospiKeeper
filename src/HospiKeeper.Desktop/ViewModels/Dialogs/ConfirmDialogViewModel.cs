using CommunityToolkit.Mvvm.Input;
using HospiKeeper.Desktop.Services;
using HospiKeeper.UI.Controls;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public sealed class ConfirmDialogViewModel : DialogViewModelBase
{
    private readonly TaskCompletionSource<bool> _completion;
    private bool _completed;

    public ConfirmDialogViewModel(
        DialogService host,
        string title,
        string message,
        string okText,
        string cancelText,
        bool danger,
        TaskCompletionSource<bool> completion) : base(host)
    {
        Title = title;
        Message = message;
        OkText = okText;
        CancelText = cancelText;
        IsDanger = danger;
        _completion = completion;
        OkCommand = new RelayCommand(() => Complete(true));
        CancelCommand = new RelayCommand(() => Complete(false));
    }

    public override string Title { get; }

    public override double Width => 420;

    public string Message { get; }
    public string OkText { get; }
    public string CancelText { get; }
    public bool IsDanger { get; }

    public AntButtonVariant OkVariant => IsDanger ? AntButtonVariant.Danger : AntButtonVariant.Primary;

    public IRelayCommand OkCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public override void OnClosed()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _completion.TrySetResult(false);
    }

    private void Complete(bool result)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _completion.TrySetResult(result);
        Host.Close();
    }
}
