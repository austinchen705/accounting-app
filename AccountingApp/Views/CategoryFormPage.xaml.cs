using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class CategoryFormPage : ContentPage
{
    public CategoryFormPage(CategoryFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
