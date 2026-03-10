using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class CategoryListViewModel : BindableObject
{
    private readonly CategoryService _categoryService;
    public ObservableCollection<Category> Categories { get; } = new();

    public ICommand DeleteCommand { get; }

    public CategoryListViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService;
        DeleteCommand = new Command<Category>(async c => await DeleteCategoryAsync(c));
    }

    public async Task LoadAsync()
    {
        var list = await _categoryService.GetAllAsync();
        Categories.Clear();
        foreach (var c in list) Categories.Add(c);
    }

    private async Task DeleteCategoryAsync(Category category)
    {
        var success = await _categoryService.DeleteAsync(category.Id);
        if (success)
            Categories.Remove(category);
        else
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("無法刪除", "此分類下仍有記錄，無法刪除。", "確定");
    }
}
