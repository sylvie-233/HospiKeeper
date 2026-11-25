using HospiKeeper.App.ViewModels;

namespace HospiKeeper.App.Views;

public partial class RoomsPage : ContentPage
{
    private readonly RoomsViewModel _viewModel;

    public RoomsPage(RoomsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }
}
