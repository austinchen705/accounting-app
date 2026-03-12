using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class HomeViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
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
        set { _currentMonth = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Transaction> RecentTransactions { get; } = new();

    public bool HasRecentTransactions
    {
        get => _hasRecentTransactions;
        set { _hasRecentTransactions = value; OnPropertyChanged(); }
    }

    public ICommand AddTransactionCommand { get; }

    public HomeViewModel(TransactionService transactionService, DataRefreshService refreshService)
    {
        _transactionService = transactionService;
        _refreshService = refreshService;
        _currentMonth = DateTime.Today.ToString("yyyy-MM");
        AddTransactionCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var (income, expense) = await _transactionService.GetMonthSummaryAsync(_currentMonth);
        TotalIncome = income;
        TotalExpense = expense;
        Balance = income - expense;

        var recent = await _transactionService.GetRecentAsync(10);
        RecentTransactions.Clear();
        foreach (var t in recent) RecentTransactions.Add(t);
        HasRecentTransactions = RecentTransactions.Count > 0;
    }

    private async void OnDataChanged()
    {
        await LoadAsync();
    }
}
