using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class TransactionListViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private readonly CurrencyService _currencyService;
    private readonly ILocalizationService _localizationService;
    private readonly DataRefreshService _refreshService;
    private bool _hasTransactions;
    private decimal _dailyIncome;
    private decimal _dailyExpense;
    private decimal _dailyBalance;
    private string _summaryCurrencyText = "單位：TWD";

    public class TransactionItemViewModel
    {
        public required Transaction Transaction { get; init; }
        public string BaseCurrency { get; init; } = "TWD";
        public decimal? ConvertedAmount { get; init; }
        public double? ExchangeRate { get; init; }

        public int Id => Transaction.Id;
        public decimal Amount => Transaction.Amount;
        public string Currency => Transaction.Currency;
        public int CategoryId => Transaction.CategoryId;
        public DateTime Date => Transaction.Date;
        public string Note => Transaction.Note;
        public string Type => Transaction.Type;
        public string AmountDisplayText => $"{Transaction.Amount:N0} {Transaction.Currency}";
        public bool HasExchangeInfo => !string.Equals(Transaction.Currency, BaseCurrency, StringComparison.OrdinalIgnoreCase)
                                       && ConvertedAmount is not null
                                       && ExchangeRate is not null;
        public string ExchangeInfoText => ExchangeRate is null || ConvertedAmount is null
            ? string.Empty
            : $"{Transaction.Amount:N0} {Transaction.Currency} x {ExchangeRate.Value:0.####} = {ConvertedAmount.Value:N0} {BaseCurrency}";
    }

    public bool HasTransactions
    {
        get => _hasTransactions;
        set { _hasTransactions = value; OnPropertyChanged(); }
    }

    public decimal DailyIncome
    {
        get => _dailyIncome;
        set { _dailyIncome = value; OnPropertyChanged(); }
    }

    public decimal DailyExpense
    {
        get => _dailyExpense;
        set { _dailyExpense = value; OnPropertyChanged(); }
    }

    public decimal DailyBalance
    {
        get => _dailyBalance;
        set { _dailyBalance = value; OnPropertyChanged(); }
    }

    public string SummaryCurrencyText
    {
        get => _summaryCurrencyText;
        set { _summaryCurrencyText = value; OnPropertyChanged(); }
    }

    public string DailyIncomeText => string.Format(_localizationService.GetString("TransactionListIncomeFormat"), DailyIncome);
    public string DailyExpenseText => string.Format(_localizationService.GetString("TransactionListExpenseFormat"), DailyExpense);
    public string DailyBalanceText => string.Format(_localizationService.GetString("TransactionListBalanceFormat"), DailyBalance);

    public ObservableCollection<TransactionItemViewModel> Transactions { get; } = new();

    private DateTime _filterDate = DateTime.Today;
    public DateTime FilterDate
    {
        get => _filterDate;
        set
        {
            _filterDate = value.Date;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilterDateLabel));
            _ = LoadAsync();
        }
    }

    public string FilterDateLabel => FilterDate.ToString("yyyy/MM/dd");

    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand ResetToTodayCommand { get; }

    public TransactionListViewModel(
        TransactionService transactionService,
        CurrencyService currencyService,
        ILocalizationService localizationService,
        DataRefreshService refreshService)
    {
        _transactionService = transactionService;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _refreshService = refreshService;
        DeleteCommand = new Command<TransactionItemViewModel>(async t =>
        {
            if (t is null) return;
            await DeleteAsync(t.Transaction);
        });
        EditCommand = new Command<TransactionItemViewModel>(async t =>
        {
            if (t is null) return;
            await Shell.Current.GoToAsync($"TransactionFormPage?id={t.Id}");
        });
        AddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
        ResetToTodayCommand = new Command(() => FilterDate = DateTime.Today);
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var list = await _transactionService.GetByDateAsync(FilterDate);
        var baseCurrency = Preferences.Get("base_currency", "TWD");

        Transactions.Clear();
        foreach (var t in list)
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            Transactions.Add(new TransactionItemViewModel
            {
                Transaction = t,
                BaseCurrency = baseCurrency,
                ExchangeRate = string.Equals(t.Currency, baseCurrency, StringComparison.OrdinalIgnoreCase) ? null : rate,
                ConvertedAmount = string.Equals(t.Currency, baseCurrency, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : Math.Round(t.Amount * (decimal)rate, 2)
            });
        }
        HasTransactions = Transactions.Count > 0;
        SummaryCurrencyText = string.Format(_localizationService.GetString("UnitPrefix"), baseCurrency);

        decimal income = 0;
        decimal expense = 0;
        foreach (var transaction in list)
        {
            var rate = await _currencyService.GetRateAsync(transaction.Currency, baseCurrency);
            var converted = transaction.Amount * (decimal)rate;
            if (transaction.Type == "income")
            {
                income += converted;
            }
            else
            {
                expense += converted;
            }
        }

        DailyIncome = income;
        DailyExpense = expense;
        DailyBalance = DailyIncome - DailyExpense;
        OnPropertyChanged(nameof(DailyIncomeText));
        OnPropertyChanged(nameof(DailyExpenseText));
        OnPropertyChanged(nameof(DailyBalanceText));
    }

    private async Task DeleteAsync(Transaction transaction)
    {
        bool confirm = await Application.Current!.Windows[0].Page!
            .DisplayAlert("確認刪除", "確定要刪除這筆記錄嗎？", "刪除", "取消");
        if (!confirm) return;

        await _transactionService.DeleteAsync(transaction.Id);
        var item = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
        if (item is not null)
        {
            Transactions.Remove(item);
        }
        HasTransactions = Transactions.Count > 0;
        _refreshService.NotifyChanged();
    }

    private async void OnDataChanged()
    {
        await LoadAsync();
    }
}
