using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;
using HomeDateRange = AccountingApp.Core.Services.HomeDateRange;
using HomeDateWindow = AccountingApp.Core.Services.HomeDateWindow;

namespace AccountingApp.ViewModels;

public class HomeViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private readonly CurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly DataRefreshService _refreshService;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _balance;
    private bool _hasRecentTransactions;
    private HomeDateRange _selectedRange = HomeDateRange.Month;
    private DateTime _anchorDate = DateTime.Today;
    private string _periodLabel = string.Empty;
    private bool _canNavigatePeriods = true;
    private Color _dayButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _dayButtonTextColor = Color.FromArgb("#007AFF");
    private Color _weekButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _weekButtonTextColor = Color.FromArgb("#007AFF");
    private Color _monthButtonBackgroundColor = Color.FromArgb("#007AFF");
    private Color _monthButtonTextColor = Colors.White;
    private Color _yearButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _yearButtonTextColor = Color.FromArgb("#007AFF");
    private Color _allButtonBackgroundColor = Color.FromArgb("#F2F4F7");
    private Color _allButtonTextColor = Color.FromArgb("#007AFF");
    private string _summaryCurrencyText = "單位：TWD";

    public decimal TotalIncome
    {
        get => _totalIncome;
        set { _totalIncome = value; OnPropertyChanged(); }
    }

    public decimal TotalExpense
    {
        get => _totalExpense;
        set { _totalExpense = value; OnPropertyChanged(); }
    }

    public decimal Balance
    {
        get => _balance;
        set { _balance = value; OnPropertyChanged(); }
    }

    public string PeriodLabel
    {
        get => _periodLabel;
        set { _periodLabel = value; OnPropertyChanged(); }
    }

    public string SummaryTitle => string.Format(
        _localizationService.GetString("SummaryTitleFormat"),
        PeriodLabel);

    public string SummaryCurrencyText
    {
        get => _summaryCurrencyText;
        set { _summaryCurrencyText = value; OnPropertyChanged(); }
    }

    public class RecentTransactionItemViewModel
    {
        public required Transaction Transaction { get; init; }
        public string BaseCurrency { get; init; } = "TWD";
        public decimal? ConvertedAmount { get; init; }
        public double? ExchangeRate { get; init; }

        public string Note => Transaction.Note;
        public DateTime Date => Transaction.Date;
        public string AmountDisplayText => $"{Transaction.Amount:N0} {Transaction.Currency}";
        public bool HasExchangeInfo => !string.Equals(Transaction.Currency, BaseCurrency, StringComparison.OrdinalIgnoreCase)
                                       && ConvertedAmount is not null
                                       && ExchangeRate is not null;
        public string ExchangeInfoText => ExchangeRate is null || ConvertedAmount is null
            ? string.Empty
            : $"{Transaction.Amount:N0} {Transaction.Currency} x {ExchangeRate.Value:0.####} = {ConvertedAmount.Value:N0} {BaseCurrency}";
    }

    public ObservableCollection<RecentTransactionItemViewModel> RecentTransactions { get; } = new();

    public bool HasRecentTransactions
    {
        get => _hasRecentTransactions;
        set { _hasRecentTransactions = value; OnPropertyChanged(); }
    }

    public bool CanNavigatePeriods
    {
        get => _canNavigatePeriods;
        set { _canNavigatePeriods = value; OnPropertyChanged(); }
    }

    public Color DayButtonBackgroundColor
    {
        get => _dayButtonBackgroundColor;
        set { _dayButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color DayButtonTextColor
    {
        get => _dayButtonTextColor;
        set { _dayButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color WeekButtonBackgroundColor
    {
        get => _weekButtonBackgroundColor;
        set { _weekButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color WeekButtonTextColor
    {
        get => _weekButtonTextColor;
        set { _weekButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color MonthButtonBackgroundColor
    {
        get => _monthButtonBackgroundColor;
        set { _monthButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color MonthButtonTextColor
    {
        get => _monthButtonTextColor;
        set { _monthButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color YearButtonBackgroundColor
    {
        get => _yearButtonBackgroundColor;
        set { _yearButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color YearButtonTextColor
    {
        get => _yearButtonTextColor;
        set { _yearButtonTextColor = value; OnPropertyChanged(); }
    }

    public Color AllButtonBackgroundColor
    {
        get => _allButtonBackgroundColor;
        set { _allButtonBackgroundColor = value; OnPropertyChanged(); }
    }

    public Color AllButtonTextColor
    {
        get => _allButtonTextColor;
        set { _allButtonTextColor = value; OnPropertyChanged(); }
    }

    public ICommand AddTransactionCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand SetRangeCommand { get; }

    public HomeViewModel(
        TransactionService transactionService,
        CurrencyService currencyService,
        ILocalizationService localizationService,
        DataRefreshService refreshService)
    {
        _transactionService = transactionService;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _refreshService = refreshService;
        AddTransactionCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
        PreviousMonthCommand = new Command(async () => await ChangePeriodAsync(-1));
        NextMonthCommand = new Command(async () => await ChangePeriodAsync(1));
        SetRangeCommand = new Command<string>(SetRange);
        UpdateRangeState();
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var (income, expense) = await _transactionService.GetSummaryAsync(_selectedRange, _anchorDate);
        TotalIncome = income;
        TotalExpense = expense;
        Balance = income - expense;

        var baseCurrency = Preferences.Get("base_currency", "TWD");
        SummaryCurrencyText = string.Format(_localizationService.GetString("UnitPrefix"), baseCurrency);
        var window = HomeDateWindow.GetDateWindow(_selectedRange, _anchorDate);
        var recent = await _transactionService.GetByDateRangeAsync(window.Start, window.EndExclusive);
        RecentTransactions.Clear();
        foreach (var t in recent.Take(10))
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            RecentTransactions.Add(new RecentTransactionItemViewModel
            {
                Transaction = t,
                BaseCurrency = baseCurrency,
                ExchangeRate = string.Equals(t.Currency, baseCurrency, StringComparison.OrdinalIgnoreCase) ? null : rate,
                ConvertedAmount = string.Equals(t.Currency, baseCurrency, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : Math.Round(t.Amount * (decimal)rate, 2)
            });
        }
        HasRecentTransactions = RecentTransactions.Count > 0;
    }

    private async Task ChangePeriodAsync(int delta)
    {
        if (!CanNavigatePeriods)
        {
            return;
        }

        _anchorDate = HomeDateWindow.MoveAnchor(_selectedRange, _anchorDate, delta);
        UpdateRangeState();
        await LoadAsync();
    }

    private void SetRange(string? rangeText)
    {
        if (!Enum.TryParse<HomeDateRange>(rangeText, ignoreCase: true, out var range))
        {
            return;
        }

        if (_selectedRange == range)
        {
            return;
        }

        _selectedRange = range;
        _anchorDate = NormalizeAnchorDate(_selectedRange, _anchorDate);
        UpdateRangeState();
        _ = LoadAsync();
    }

    private void UpdateRangeState()
    {
        CanNavigatePeriods = _selectedRange != HomeDateRange.All;
        PeriodLabel = HomeDateWindow.GetPeriodLabel(_selectedRange, _anchorDate);
        ApplyFilterButtonState();
        OnPropertyChanged(nameof(SummaryTitle));
    }

    private void ApplyFilterButtonState()
    {
        DayButtonBackgroundColor = GetButtonBackground(HomeDateRange.Day);
        DayButtonTextColor = GetButtonText(HomeDateRange.Day);
        WeekButtonBackgroundColor = GetButtonBackground(HomeDateRange.Week);
        WeekButtonTextColor = GetButtonText(HomeDateRange.Week);
        MonthButtonBackgroundColor = GetButtonBackground(HomeDateRange.Month);
        MonthButtonTextColor = GetButtonText(HomeDateRange.Month);
        YearButtonBackgroundColor = GetButtonBackground(HomeDateRange.Year);
        YearButtonTextColor = GetButtonText(HomeDateRange.Year);
        AllButtonBackgroundColor = GetButtonBackground(HomeDateRange.All);
        AllButtonTextColor = GetButtonText(HomeDateRange.All);
    }

    private Color GetButtonBackground(HomeDateRange range) =>
        _selectedRange == range ? Color.FromArgb("#007AFF") : Color.FromArgb("#F2F4F7");

    private Color GetButtonText(HomeDateRange range) =>
        _selectedRange == range ? Colors.White : Color.FromArgb("#007AFF");

    private static DateTime NormalizeAnchorDate(HomeDateRange range, DateTime anchorDate)
    {
        var date = anchorDate.Date;
        return range switch
        {
            HomeDateRange.Month => new DateTime(date.Year, date.Month, 1),
            HomeDateRange.Year => new DateTime(date.Year, 1, 1),
            _ => date
        };
    }

    private async void OnDataChanged()
    {
        await LoadAsync();
    }
}
