using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class CategoryListPage : ContentPage
{
    private readonly CategoryListViewModel _vm;

    public CategoryListPage(CategoryListViewModel vm)
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
