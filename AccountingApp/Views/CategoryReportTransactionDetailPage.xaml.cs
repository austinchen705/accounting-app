using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class CategoryReportTransactionDetailPage : ContentPage
{
    private readonly CategoryReportTransactionDetailViewModel _vm;

    public CategoryReportTransactionDetailPage(CategoryReportTransactionDetailViewModel vm)
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
