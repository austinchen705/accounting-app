using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;
using AmountInputSanitizer = AccountingApp.Core.Services.TransactionAmountInputSanitizer;

namespace AccountingApp.ViewModels;

[QueryProperty(nameof(TransactionId), "id")]
public class TransactionFormViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
    private readonly BudgetService _budgetService;
    private readonly ILocalizationService _localizationService;

    private int _transactionId;
    private string _amountText = string.Empty;
    private string _currency = "TWD";
    private Category? _selectedCategory;
    private DateTime _date = DateTime.Today;
    private string _note = string.Empty;
    private string _type = "expense";
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isEdit;

    public int TransactionId
    {
        get => _transactionId;
        set { _transactionId = value; OnPropertyChanged(); _ = LoadExistingAsync(value); }
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            var sanitized = AmountInputSanitizer.Sanitize(value);
            if (_amountText == sanitized)
            {
                return;
            }

            _amountText = sanitized;
            OnPropertyChanged();
        }
    }

    public string Currency
    {
        get => _currency;
        set { _currency = value; OnPropertyChanged(); }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    public DateTime Date
    {
        get => _date;
        set { _date = value.Date; OnPropertyChanged(); }
    }

    public string Note
    {
        get => _note;
        set { _note = value; OnPropertyChanged(); }
    }

    public string Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); _ = LoadCategoriesForTypeAsync(); }
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

    public string PageTitle => _localizationService.GetString(
        _isEdit ? "TransactionFormEditTitle" : "TransactionFormPageTitle");

    public string SaveButtonText => _localizationService.GetString(
        _isEdit ? "TransactionFormUpdateButton" : "TransactionFormSaveButton");

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<string> Currencies { get; } =
    [
        "TWD", "USD", "JPY", "EUR", "GBP", "CNY", "HKD", "AUD", "CAD", "SGD"
    ];

    public ICommand SaveCommand { get; }

    public TransactionFormViewModel(
        TransactionService transactionService,
        CategoryService categoryService,
        BudgetService budgetService,
        ILocalizationService localizationService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _budgetService = budgetService;
        _localizationService = localizationService;
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesForTypeAsync();
    }

    private async Task LoadCategoriesForTypeAsync(int? preferredCategoryId = null)
    {
        var cats = await _categoryService.GetByTypeAsync(Type);
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);
        var selectedCategoryId = preferredCategoryId ?? SelectedCategory?.Id;
        if (selectedCategoryId.HasValue)
        {
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == selectedCategoryId.Value);
        }

        if (SelectedCategory == null || !Categories.Contains(SelectedCategory))
            SelectedCategory = Categories.FirstOrDefault();
    }

    private async Task LoadExistingAsync(int id)
    {
        if (id <= 0) return;
        _isEdit = true;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        var txn = (await _transactionService.GetAllAsync()).FirstOrDefault(t => t.Id == id);
        if (txn is null) return;
        AmountText = txn.Amount.ToString(CultureInfo.InvariantCulture);
        Currency = txn.Currency;
        Date = txn.Date;
        Note = txn.Note;
        if (!string.Equals(_type, txn.Type, StringComparison.Ordinal))
        {
            _type = txn.Type;
            OnPropertyChanged(nameof(Type));
        }

        await LoadCategoriesForTypeAsync(txn.CategoryId);
    }

    private async Task SaveAsync()
    {
        if (!AmountInputSanitizer.TryParsePositiveDecimal(AmountText, out var amount))
        {
            ErrorMessage = _localizationService.GetString("TransactionFormInvalidAmountError");
            HasError = true;
            return;
        }

        HasError = false;
        var txn = new Transaction
        {
            Amount = amount,
            Currency = Currency,
            CategoryId = SelectedCategory?.Id ?? 0,
            Date = Date,
            Note = Note,
            Type = Type
        };

        if (_isEdit)
        {
            txn.Id = _transactionId;
            await _transactionService.UpdateAsync(txn);
        }
        else
        {
            await _transactionService.AddAsync(txn);
        }

        if (txn.Type == "expense" && txn.CategoryId > 0)
            await _budgetService.CheckAndNotifyAsync(txn.CategoryId, txn.Date.ToString("yyyy-MM"));

        await Shell.Current.GoToAsync("..");
    }
}
