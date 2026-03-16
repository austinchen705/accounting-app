using AccountingApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingApp.Views;

public partial class CategoryListPage : ContentPage
{
    private readonly CategoryListViewModel _vm;

    public CategoryListPage() : this(ResolveViewModel())
    {
    }

    public CategoryListPage(CategoryListViewModel vm)
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
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await CategoryScrollView.ScrollToAsync(CategoryFormCard, ScrollToPosition.Start, true);
        });
    }

    private static CategoryListViewModel ResolveViewModel()
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        return services?.GetRequiredService<CategoryListViewModel>()
            ?? throw new InvalidOperationException("CategoryListViewModel service is unavailable.");
    }
}
