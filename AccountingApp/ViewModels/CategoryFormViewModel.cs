using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class CategoryFormViewModel : BindableObject
{
    private readonly CategoryService _categoryService;
    private string _name = string.Empty;
    private string _type = "expense";
    private string _errorMessage = string.Empty;
    private bool _hasError;

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

        var category = new Category { Name = Name.Trim(), Type = Type };
        var success = await _categoryService.AddAsync(category);
        if (!success)
        {
            ErrorMessage = "此分類名稱已存在";
            HasError = true;
            return;
        }

        HasError = false;
        await Shell.Current.GoToAsync("..");
    }
}
