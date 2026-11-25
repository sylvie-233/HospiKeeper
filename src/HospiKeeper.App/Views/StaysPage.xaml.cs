using HospiKeeper.App.ViewModels;

namespace HospiKeeper.App.Views;

public partial class StaysPage : ContentPage
{
    private readonly StaysViewModel _viewModel;

    public StaysPage(StaysViewModel viewModel)
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
