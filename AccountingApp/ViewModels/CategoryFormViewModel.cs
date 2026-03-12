using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

[QueryProperty(nameof(CategoryId), "id")]
public class CategoryFormViewModel : BindableObject
{
    public class CategoryTypeOption
    {
        public string Value { get; init; } = "expense";
        public string Label { get; init; } = "支出";
    }

    private readonly CategoryService _categoryService;
    private int _categoryId;
    private string _name = string.Empty;
    private string _type = "expense";
    private CategoryTypeOption? _selectedTypeOption;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isEdit;

    public int CategoryId
    {
        get => _categoryId;
        set
        {
            _categoryId = value;
            OnPropertyChanged();
            _ = LoadExistingAsync(value);
        }
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

    public List<CategoryTypeOption> TypeOptions { get; } =
    [
        new() { Value = "expense", Label = "支出" },
        new() { Value = "income", Label = "收入" }
    ];

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

    public ICommand SaveCommand { get; }

    public CategoryFormViewModel(CategoryService categoryService)
    {
        _categoryService = categoryService;
        _selectedTypeOption = TypeOptions.First();
        SaveCommand = new Command(async () => await SaveAsync());
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
            c.Id != CategoryId);

        if (duplicated)
        {
            ErrorMessage = "此分類名稱已存在";
            HasError = true;
            return;
        }

        if (_isEdit)
        {
            var existing = await _categoryService.GetByIdAsync(CategoryId);
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
            var category = new Category { Name = normalizedName, Type = Type };
            await _categoryService.AddAsync(category);
        }

        HasError = false;
        await Shell.Current.GoToAsync("..");
    }

    private async Task LoadExistingAsync(int id)
    {
        if (id <= 0) return;

        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return;

        _isEdit = true;
        Name = category.Name;
        Type = category.Type;
        SelectedTypeOption = TypeOptions.FirstOrDefault(x => x.Value == category.Type) ?? TypeOptions.First();
    }
}
