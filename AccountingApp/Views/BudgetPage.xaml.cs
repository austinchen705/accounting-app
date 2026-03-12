using AccountingApp.ViewModels;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace AccountingApp.Views;

public partial class BudgetPage : ContentPage
{
    private readonly BudgetViewModel _vm;

    public BudgetPage(BudgetViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);
        On<iOS>().SetUseSafeArea(false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
