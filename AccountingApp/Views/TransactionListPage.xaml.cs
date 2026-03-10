using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class TransactionListPage : ContentPage
{
    private readonly TransactionListViewModel _vm;

    public TransactionListPage(TransactionListViewModel vm)
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
