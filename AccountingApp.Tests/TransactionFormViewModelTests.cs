namespace AccountingApp.Tests;

public class TransactionFormViewModelTests
{
    [Fact]
    public void TransactionFormViewModel_loads_categories_using_existing_category_id_when_editing()
    {
        var source = ReadTransactionFormViewModel();

        Assert.Contains("private async Task LoadCategoriesForTypeAsync(int? preferredCategoryId = null)", source);
        Assert.Contains("var selectedCategoryId = preferredCategoryId ?? SelectedCategory?.Id;", source);
        Assert.Contains("Categories.FirstOrDefault(c => c.Id == selectedCategoryId.Value)", source);
        Assert.Contains("await LoadCategoriesForTypeAsync(txn.CategoryId);", source);
    }

    [Fact]
    public void TransactionFormViewModel_does_not_assign_selected_category_before_categories_finish_loading()
    {
        var source = ReadTransactionFormViewModel();

        Assert.DoesNotContain("SelectedCategory = Categories.FirstOrDefault(c => c.Id == txn.CategoryId);", source);
        Assert.Contains("OnPropertyChanged(nameof(Type));", source);
    }

    private static string ReadTransactionFormViewModel()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/TransactionFormViewModel.cs"));

        return File.ReadAllText(path);
    }
}
