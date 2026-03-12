using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class CategoryReportPage : ContentPage
{
    private readonly CategoryReportViewModel _vm;

    public CategoryReportPage(CategoryReportViewModel vm)
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
