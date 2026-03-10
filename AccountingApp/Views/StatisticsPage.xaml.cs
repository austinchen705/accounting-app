using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsViewModel _vm;

    public StatisticsPage(StatisticsViewModel vm)
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
