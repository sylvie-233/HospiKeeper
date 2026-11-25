using CommunityToolkit.Mvvm.ComponentModel;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

public abstract class DialogViewModelBase : ObservableObject
{
    protected DialogViewModelBase(DialogService host) => Host = host;

    public DialogService Host { get; }

    public abstract string Title { get; }

    public virtual double Width => 520;

    /// <summary>宿主关闭弹窗时回调（如确认框需要完成 Task）。</summary>
    public virtual void OnClosed()
    {
    }
}
