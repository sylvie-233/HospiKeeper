using HospiKeeper.App.ViewModels;

namespace HospiKeeper.App.Controls;

/// <summary>房态色卡，三列网格复用。</summary>
public partial class RoomCardView : ContentView
{
    public RoomCardView() => InitializeComponent();

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        IsVisible = BindingContext is RoomCardViewModel;
    }
}
