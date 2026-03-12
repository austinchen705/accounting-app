using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountingApp.Models;
using AccountingApp.Services;

namespace AccountingApp.ViewModels;

public class TransactionListViewModel : BindableObject
{
    private readonly TransactionService _transactionService;
    private readonly DataRefreshService _refreshService;
    private bool _hasTransactions;

    public bool HasTransactions
    {
        get => _hasTransactions;
        set { _hasTransactions = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Transaction> Transactions { get; } = new();

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

    public TransactionListViewModel(TransactionService transactionService, DataRefreshService refreshService)
    {
        _transactionService = transactionService;
        _refreshService = refreshService;
        DeleteCommand = new Command<Transaction>(async t => await DeleteAsync(t));
        EditCommand = new Command<Transaction>(async t =>
            await Shell.Current.GoToAsync($"TransactionFormPage?id={t.Id}"));
        AddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("TransactionFormPage"));
        ResetToTodayCommand = new Command(() => FilterDate = DateTime.Today);
        _refreshService.DataChanged += OnDataChanged;
    }

    public async Task LoadAsync()
    {
        var list = await _transactionService.GetByDateAsync(FilterDate);

        Transactions.Clear();
        foreach (var t in list) Transactions.Add(t);
        HasTransactions = Transactions.Count > 0;
    }

    private async Task DeleteAsync(Transaction transaction)
    {
        bool confirm = await Application.Current!.Windows[0].Page!
            .DisplayAlert("確認刪除", "確定要刪除這筆記錄嗎？", "刪除", "取消");
        if (!confirm) return;

        await _transactionService.DeleteAsync(transaction.Id);
        Transactions.Remove(transaction);
        HasTransactions = Transactions.Count > 0;
        _refreshService.NotifyChanged();
    }

    private async void OnDataChanged()
    {
        await LoadAsync();
    }
}
