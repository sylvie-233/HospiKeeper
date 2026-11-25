using HospiKeeper.App.ViewModels;

namespace HospiKeeper.App.Views;

public partial class MinePage : ContentPage
{
    private readonly MineViewModel _viewModel;

    public MinePage(MineViewModel viewModel)
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
