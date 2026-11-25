using CommunityToolkit.Mvvm.ComponentModel;
using HospiKeeper.Desktop.Services;

namespace HospiKeeper.Desktop.ViewModels.Dialogs;

/// <summary>弹窗 ViewModel 基类，提供关闭回调。</summary>
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
