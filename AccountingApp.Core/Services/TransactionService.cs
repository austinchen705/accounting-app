using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public class TransactionService
{
    private readonly DatabaseService _db;
    private readonly CurrencyService _currencyService;
    private readonly IPreferenceStore _preferences;

    public TransactionService(DatabaseService db, CurrencyService currencyService, IPreferenceStore preferences)
    {
        _db = db;
        _currencyService = currencyService;
        _preferences = preferences;
    }

    public async Task<List<Transaction>> GetAllAsync() =>
        await _db.Db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();

    public async Task<List<Transaction>> GetByMonthAsync(string month)
    {
        var all = await _db.Db.Table<Transaction>().ToListAsync();
        return all.Where(t => t.Date.ToString("yyyy-MM") == month)
                  .OrderByDescending(t => t.Date).ToList();
    }

    public async Task<List<Transaction>> GetFilteredAsync(string? month, int? categoryId, string? currency)
    {
        var all = await _db.Db.Table<Transaction>().ToListAsync();
        return all.Where(t =>
            (month == null || t.Date.ToString("yyyy-MM") == month) &&
            (categoryId == null || t.CategoryId == categoryId) &&
            (currency == null || t.Currency == currency))
            .OrderByDescending(t => t.Date).ToList();
    }

    public async Task AddAsync(Transaction transaction) =>
        await _db.Db.InsertAsync(transaction);

    public async Task UpdateAsync(Transaction transaction) =>
        await _db.Db.UpdateAsync(transaction);

    public async Task DeleteAsync(int id) =>
        await _db.Db.DeleteAsync<Transaction>(id);

    public async Task<(decimal Income, decimal Expense)> GetMonthSummaryAsync(string month)
    {
        var txns = await GetByMonthAsync(month);
        var baseCurrency = _preferences.Get("base_currency", "TWD");
        decimal income = 0, expense = 0;
        foreach (var t in txns)
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            var converted = t.Amount * (decimal)rate;
            if (t.Type == "income") income += converted;
            else expense += converted;
        }
        return (income, expense);
    }

    public async Task<List<Transaction>> GetRecentAsync(int count = 10)
    {
        var all = await _db.Db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
        return all.Take(count).ToList();
    }
}
