using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class TransactionFormPage : ContentPage
{
    private readonly TransactionFormViewModel _vm;

    public TransactionFormPage(TransactionFormViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}
