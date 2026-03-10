using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class TransactionListViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private string _selectedMonth;
    private int? _selectedCategoryId;
    private string? _selectedCurrency;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public string SelectedMonth
    {
        get => _selectedMonth;
        set { _selectedMonth = value; OnPropertyChanged(); _ = LoadAsync(); }
    }

    private DateTime _filterDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime FilterDate
    {
        get => _filterDate;
        set
        {
            _filterDate = value; OnPropertyChanged();
            SelectedMonth = value.ToString("yyyy-MM");
        }
    }

    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand AddCommand { get; }

    public TransactionListViewModel(TransactionService transactionService)
    {
        _transactionService = transactionService;
        _selectedMonth = DateTime.Today.ToString("yyyy-MM");
        DeleteCommand = new Command<Transaction>(async t => await DeleteAsync(t));
        EditCommand = new Command<Transaction>(async t =>
            await Shell.Current.GoToAsync($"TransactionFormPage?id={t.Id}"));
        AddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
    }

    public async Task LoadAsync()
    {
        var list = await _transactionService.GetFilteredAsync(SelectedMonth, _selectedCategoryId, _selectedCurrency);
        Transactions.Clear();
        foreach (var t in list) Transactions.Add(t);
    }

    private async Task DeleteAsync(Transaction transaction)
    {
        bool confirm = await Application.Current!.Windows[0].Page!
            .DisplayAlert("確認刪除", "確定要刪除這筆記錄嗎？", "刪除", "取消");
        if (!confirm) return;

        await _transactionService.DeleteAsync(transaction.Id);
        Transactions.Remove(transaction);
    }
}
