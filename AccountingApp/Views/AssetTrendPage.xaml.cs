using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class AssetTrendPage : ContentPage
{
    private readonly AssetTrendViewModel _vm;

    public AssetTrendPage(AssetTrendViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        _vm.EditRequested += OnEditRequested;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnEditRequested(object? sender, EventArgs e)
    {
        await PageScrollView.ScrollToAsync(SnapshotFormFrame, ScrollToPosition.Start, true);
    }
}
