using AccountingApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingApp.Views;

public partial class CategoryFormPage : ContentPage
{
    private readonly CategoryFormViewModel _vm;

    public CategoryFormPage() : this(ResolveViewModel())
    {
    }

    public CategoryFormPage(CategoryFormViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    public void PrepareForCreate()
    {
        _vm.PrepareForCreate();
    }

    public Task PrepareForEditAsync(int id)
    {
        return _vm.PrepareForEditAsync(id);
    }

    private static CategoryFormViewModel ResolveViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetRequiredService<CategoryFormViewModel>()
            ?? throw new InvalidOperationException("CategoryFormViewModel service is unavailable.");
    }
}
