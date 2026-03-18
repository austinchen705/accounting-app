using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;
public class CategoryFormViewModel : BindableObject
{
    private readonly ILocalizationService _localizationService;
    public class CategoryTypeOption
    {
        public string Value { get; init; } = "expense";
        public string Label { get; init; } = string.Empty;
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
    [];

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

    public string FormTitle => _isEdit
        ? _localizationService.GetString("CategoryFormEditTitle")
        : _localizationService.GetString("CategoryFormCreateTitle");

    public string SubmitButtonText => _isEdit
        ? _localizationService.GetString("CategoryFormUpdateButton")
        : _localizationService.GetString("CategoryFormCreateButton");

    public ICommand SaveCommand { get; }

    public CategoryFormViewModel(CategoryService categoryService, ILocalizationService localizationService)
    {
        _categoryService = categoryService;
        _localizationService = localizationService;
        TypeOptions =
        [
            new() { Value = "expense", Label = _localizationService.GetString("CategoryTypeExpenseLabel") },
            new() { Value = "income", Label = _localizationService.GetString("CategoryTypeIncomeLabel") }
        ];
        _selectedTypeOption = TypeOptions.First();
        SaveCommand = new Command(async () => await SaveAsync());
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = _localizationService.GetString("CategoryNameRequiredError");
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
            ErrorMessage = _localizationService.GetString("CategoryNameDuplicateError");
            HasError = true;
            return;
        }

        if (_isEdit)
        {
            var existing = await _categoryService.GetByIdAsync(CategoryId);
            if (existing is null)
            {
                ErrorMessage = _localizationService.GetString("CategoryNotFoundError");
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
        if (Shell.Current?.Navigation?.NavigationStack?.Count > 1)
        {
            await Shell.Current.Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    public void PrepareForCreate()
    {
        _isEdit = false;
        CategoryId = 0;
        Name = string.Empty;
        Type = "expense";
        SelectedTypeOption = TypeOptions.First();
        ErrorMessage = string.Empty;
        HasError = false;
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SubmitButtonText));
    }

    public async Task PrepareForEditAsync(int id)
    {
        if (id <= 0) return;

        CategoryId = id;
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null) return;

        _isEdit = true;
        Name = category.Name;
        Type = category.Type;
        SelectedTypeOption = TypeOptions.FirstOrDefault(x => x.Value == category.Type) ?? TypeOptions.First();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SubmitButtonText));
    }
}
