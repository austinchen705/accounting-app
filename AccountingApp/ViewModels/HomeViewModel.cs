using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class HomeViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private readonly CurrencyService _currencyService;
    private readonly DataRefreshService _refreshService;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _balance;
    private string _currentMonth;
    private bool _hasRecentTransactions;

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

    public string CurrentMonth
    {
        get => _currentMonth;
        set
        {
            _currentMonth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentMonthLabel));
            OnPropertyChanged(nameof(SummaryTitle));
        }
    }

    public string CurrentMonthLabel
    {
        get
        {
            var month = DateTime.ParseExact($"{_currentMonth}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return month.ToString("yyyy/MM");
        }
    }

    public string SummaryTitle => $"{CurrentMonthLabel} 總覽";

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

    public ICommand AddTransactionCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }

    public HomeViewModel(
        TransactionService transactionService,
        CurrencyService currencyService,
        DataRefreshService refreshService)
    {
        _transactionService = transactionService;
        _currencyService = currencyService;
        _refreshService = refreshService;
        _currentMonth = DateTime.Today.ToString("yyyy-MM");
        AddTransactionCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
        PreviousMonthCommand = new Command(async () => await ChangeMonthAsync(-1));
        NextMonthCommand = new Command(async () => await ChangeMonthAsync(1));
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var (income, expense) = await _transactionService.GetMonthSummaryAsync(_currentMonth);
        TotalIncome = income;
        TotalExpense = expense;
        Balance = income - expense;

        var recent = await _transactionService.GetByMonthAsync(_currentMonth);
        var baseCurrency = Preferences.Get("base_currency", "TWD");
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

    private async Task ChangeMonthAsync(int deltaMonths)
    {
        var month = DateTime.ParseExact($"{_currentMonth}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)
            .AddMonths(deltaMonths);
        CurrentMonth = month.ToString("yyyy-MM");
        await LoadAsync();
    }

    private async void OnDataChanged()
    {
        await LoadAsync();
    }
}
