using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class HomeViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _balance;
    private string _currentMonth;

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

    public ICommand AddTransactionCommand { get; }

    public HomeViewModel(TransactionService transactionService)
    {
        _transactionService = transactionService;
        _currentMonth = DateTime.Today.ToString("yyyy-MM");
        AddTransactionCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
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
    }
}
