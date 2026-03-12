using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;
using CalendarMonth = AccountingApp.Core.Services.CalendarMonth;

namespace AccountingApp.ViewModels;

[QueryProperty(nameof(TransactionId), "id")]
public class TransactionFormViewModel : BindableObject
{
    public class CalendarDayItem
    {
        public DateTime Date { get; init; }
        public string Label { get; init; } = string.Empty;
        public Color TextColor { get; init; } = Colors.Black;
        public Color BackgroundColor { get; init; } = Colors.Transparent;
        public Color BorderColor { get; init; } = Colors.Transparent;
    }

    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
    private readonly BudgetService _budgetService;

    private int _transactionId;
    private decimal _amount;
    private string _currency = "TWD";
    private Category? _selectedCategory;
    private DateTime _date = DateTime.Today;
    private string _note = string.Empty;
    private string _type = "expense";
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _isEdit;
    private bool _isCalendarVisible;
    private DateTime _calendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private string _dateDisplayText = DateTime.Today.ToString("yyyy/MM/dd");
    private string _calendarMonthText = DateTime.Today.ToString("yyyy年MM月");

    public int TransactionId
    {
        get => _transactionId;
        set { _transactionId = value; OnPropertyChanged(); _ = LoadExistingAsync(value); }
    }

    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
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
        set
        {
            _date = value;
            DateDisplayText = value.ToString("yyyy/MM/dd");
            OnPropertyChanged();
            SyncCalendarMonth(value);
            RefreshCalendarDays();
        }
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

    public bool IsCalendarVisible
    {
        get => _isCalendarVisible;
        set { _isCalendarVisible = value; OnPropertyChanged(); }
    }

    public string DateDisplayText
    {
        get => _dateDisplayText;
        private set { _dateDisplayText = value; OnPropertyChanged(); }
    }

    public string CalendarMonthText
    {
        get => _calendarMonthText;
        private set { _calendarMonthText = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<CalendarDayItem> CalendarDays { get; } = new();
    public IReadOnlyList<string> WeekdayHeaders { get; } = ["日", "一", "二", "三", "四", "五", "六"];
    public ObservableCollection<string> Currencies { get; } = new()
        { "TWD", "USD", "JPY", "EUR", "GBP", "CNY", "HKD", "AUD", "CAD", "SGD" };

    public ICommand SaveCommand { get; }
    public ICommand OpenCalendarCommand { get; }
    public ICommand CloseCalendarCommand { get; }
    public ICommand PreviousCalendarMonthCommand { get; }
    public ICommand NextCalendarMonthCommand { get; }
    public ICommand SelectCalendarDateCommand { get; }

    public TransactionFormViewModel(TransactionService transactionService, CategoryService categoryService, BudgetService budgetService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _budgetService = budgetService;
        SaveCommand = new Command(async () => await SaveAsync());
        OpenCalendarCommand = new Command(OpenCalendar);
        CloseCalendarCommand = new Command(() => IsCalendarVisible = false);
        PreviousCalendarMonthCommand = new Command(() => ChangeCalendarMonth(-1));
        NextCalendarMonthCommand = new Command(() => ChangeCalendarMonth(1));
        SelectCalendarDateCommand = new Command<CalendarDayItem>(SelectCalendarDate);
        RefreshCalendarDays();
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesForTypeAsync();
        RefreshCalendarDays();
    }

    private async Task LoadCategoriesForTypeAsync()
    {
        var cats = await _categoryService.GetByTypeAsync(Type);
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);
        if (SelectedCategory == null || !Categories.Contains(SelectedCategory))
            SelectedCategory = Categories.FirstOrDefault();
    }

    private async Task LoadExistingAsync(int id)
    {
        if (id <= 0) return;
        _isEdit = true;
        var txn = (await _transactionService.GetAllAsync()).FirstOrDefault(t => t.Id == id);
        if (txn is null) return;
        Amount = txn.Amount;
        Currency = txn.Currency;
        Date = txn.Date;
        Note = txn.Note;
        Type = txn.Type;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == txn.CategoryId);
    }

    private async Task SaveAsync()
    {
        if (Amount <= 0)
        {
            ErrorMessage = "請輸入有效金額（大於 0）";
            HasError = true;
            return;
        }

        HasError = false;
        var txn = new Transaction
        {
            Amount = Amount,
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

        // Check budget after saving expense
        if (txn.Type == "expense" && txn.CategoryId > 0)
            await _budgetService.CheckAndNotifyAsync(txn.CategoryId, txn.Date.ToString("yyyy-MM"));

        await Shell.Current.GoToAsync("..");
    }

    private void OpenCalendar()
    {
        SyncCalendarMonth(Date);
        RefreshCalendarDays();
        IsCalendarVisible = true;
    }

    private void ChangeCalendarMonth(int offset)
    {
        _calendarMonth = _calendarMonth.AddMonths(offset);
        UpdateCalendarMonthText();
        RefreshCalendarDays();
    }

    private void SelectCalendarDate(CalendarDayItem? day)
    {
        if (day is null)
        {
            return;
        }

        Date = day.Date;
        IsCalendarVisible = false;
    }

    private void SyncCalendarMonth(DateTime date)
    {
        _calendarMonth = new DateTime(date.Year, date.Month, 1);
        UpdateCalendarMonthText();
    }

    private void UpdateCalendarMonthText()
    {
        CalendarMonthText = _calendarMonth.ToString("yyyy年MM月");
    }

    private void RefreshCalendarDays()
    {
        var cells = CalendarMonth.BuildGrid(_calendarMonth);
        CalendarDays.Clear();

        foreach (var cell in cells)
        {
            var isSelected = cell.Date.Date == Date.Date;
            var isToday = cell.Date.Date == DateTime.Today;
            CalendarDays.Add(new CalendarDayItem
            {
                Date = cell.Date,
                Label = cell.Date.Day.ToString(),
                TextColor = isSelected
                    ? Colors.White
                    : cell.IsCurrentMonth ? Colors.Black : Colors.Gray,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#0F766E")
                    : Colors.Transparent,
                BorderColor = isToday && !isSelected
                    ? Color.FromArgb("#0F766E")
                    : Colors.Transparent
            });
        }
    }
}
