using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class AssetTrendChartPage : ContentPage
{
    private readonly AssetTrendViewModel _vm;

    public AssetTrendChartPage(AssetTrendViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
