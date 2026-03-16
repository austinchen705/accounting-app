using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class CategoryListViewModel : BindableObject
{
    private readonly CategoryService _categoryService;
    private bool _hasCategories;
    private int _editingCategoryId;
    private string _name = string.Empty;
    private string _type = "expense";
    private CategoryTypeOption? _selectedTypeOption;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isEditing;

    public event EventHandler? EditRequested;

    public class CategoryTypeOption
    {
        public string Value { get; init; } = "expense";
        public string Label { get; init; } = "支出";
    }

    public class CategoryItemViewModel
    {
        public required Category Category { get; init; }
        public string Name => Category.Name;
        public string TypeText => Category.Type == "income" ? "收入" : "支出";
    }

    public ObservableCollection<CategoryItemViewModel> Categories { get; } = new();
    public List<CategoryTypeOption> TypeOptions { get; } =
    [
        new() { Value = "expense", Label = "支出" },
        new() { Value = "income", Label = "收入" }
    ];

    public bool HasCategories
    {
        get => _hasCategories;
        set { _hasCategories = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    public CategoryTypeOption? SelectedTypeOption
    {
        get => _selectedTypeOption;
        set
        {
            _selectedTypeOption = value;
            Type = value?.Value ?? "expense";
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        set { _hasError = value; OnPropertyChanged(); }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormTitleText));
            OnPropertyChanged(nameof(SubmitButtonText));
        }
    }

    public string FormTitleText => IsEditing ? "編輯分類" : "新增分類";

    public string SubmitButtonText => IsEditing ? "更新分類" : "新增分類";

    public ICommand SaveCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCommand { get; }

    public CategoryListViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService;
        _selectedTypeOption = TypeOptions.First();
        SaveCommand = new Command(async () => await SaveAsync());
        EditCommand = new Command<object>(item => BeginEdit(ResolveCategory(item)));
        CancelEditCommand = new Command(CancelEdit);
        DeleteCommand = new Command<object>(async item => await DeleteCategoryAsync(ResolveCategory(item)));
    }

    public async Task LoadAsync()
    {
        var list = await _categoryService.GetAllAsync();
        Categories.Clear();
        foreach (var c in list)
            Categories.Add(new CategoryItemViewModel { Category = c });
        HasCategories = Categories.Count > 0;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "請輸入分類名稱";
            HasError = true;
            return;
        }

        var normalizedName = Name.Trim();
        var all = await _categoryService.GetAllAsync();
        var duplicated = all.Any(c =>
            c.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
            c.Type == Type &&
            c.Id != _editingCategoryId);

        if (duplicated)
        {
            ErrorMessage = "此分類名稱已存在";
            HasError = true;
            return;
        }

        if (IsEditing)
        {
            var existing = await _categoryService.GetByIdAsync(_editingCategoryId);
            if (existing is null)
            {
                ErrorMessage = "找不到要編輯的分類";
                HasError = true;
                return;
            }

            existing.Name = normalizedName;
            existing.Type = Type;
            await _categoryService.UpdateAsync(existing);
        }
        else
        {
            var added = await _categoryService.AddAsync(new Category { Name = normalizedName, Type = Type });
            if (!added)
            {
                ErrorMessage = "此分類名稱已存在";
                HasError = true;
                return;
            }
        }

        CancelEdit();
        await LoadAsync();
    }

    private async Task DeleteCategoryAsync(Category? category)
    {
        if (category is null) return;

        var success = await _categoryService.DeleteAsync(category.Id);
        if (success)
        {
            var item = Categories.FirstOrDefault(x => x.Category.Id == category.Id);
            if (item is not null) Categories.Remove(item);
            HasCategories = Categories.Count > 0;
        }
        else
            await Application.Current!.Windows[0].Page!
                .DisplayAlert("無法刪除", "此分類下仍有記錄，無法刪除。", "確定");
    }

    private static Category? ResolveCategory(object? item)
    {
        return item switch
        {
            CategoryItemViewModel viewModel => viewModel.Category,
            Category category => category,
            _ => null
        };
    }

    private void BeginEdit(Category? category)
    {
        if (category is null) return;

        _editingCategoryId = category.Id;
        Name = category.Name;
        Type = category.Type;
        SelectedTypeOption = TypeOptions.FirstOrDefault(x => x.Value == category.Type) ?? TypeOptions.First();
        ErrorMessage = string.Empty;
        HasError = false;
        IsEditing = true;
        EditRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelEdit()
    {
        _editingCategoryId = 0;
        Name = string.Empty;
        Type = "expense";
        SelectedTypeOption = TypeOptions.First();
        ErrorMessage = string.Empty;
        HasError = false;
        IsEditing = false;
    }
}
