using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class CategoryListViewModel : BindableObject
{
    private readonly CategoryService _categoryService;
    public class CategoryItemViewModel
    {
        public required Category Category { get; init; }
        public string Name => Category.Name;
        public string TypeText => Category.Type == "income" ? "收入" : "支出";
    }

    public ObservableCollection<CategoryItemViewModel> Categories { get; } = new();

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public CategoryListViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService;
        AddCommand = new Command(async () => await Shell.Current.GoToAsync("CategoryFormPage"));
        EditCommand = new Command<CategoryItemViewModel>(async item => await EditCategoryAsync(item?.Category));
        DeleteCommand = new Command<CategoryItemViewModel>(async item => await DeleteCategoryAsync(item?.Category));
    }

    public async Task LoadAsync()
    {
        var list = await _categoryService.GetAllAsync();
        Categories.Clear();
        foreach (var c in list)
            Categories.Add(new CategoryItemViewModel { Category = c });
    }

    private async Task DeleteCategoryAsync(Category? category)
    {
        if (category is null) return;

        var success = await _categoryService.DeleteAsync(category.Id);
        if (success)
        {
            var item = Categories.FirstOrDefault(x => x.Category.Id == category.Id);
            if (item is not null) Categories.Remove(item);
        }
        else
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("無法刪除", "此分類下仍有記錄，無法刪除。", "確定");
    }

    private async Task EditCategoryAsync(Category? category)
    {
        if (category is null) return;
        await Shell.Current.GoToAsync($"CategoryFormPage?id={category.Id}");
    }
}
