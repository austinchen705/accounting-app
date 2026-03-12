using AccountingApp.Models;
using SQLite;

namespace AccountingApp.Services;

public class TransactionService
{
    private readonly DatabaseService _db;
    private readonly CurrencyService _currency;

    public TransactionService(DatabaseService db, CurrencyService currency)
    {
        _db = db;
        _currency = currency;
    }

    private async Task EnsureInitializedAsync()
    {
        await _db.InitializeAsync();
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<List<Transaction>> GetByMonthAsync(string month)
    {
        await EnsureInitializedAsync();
        if (!DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var start))
            return [];

        var end = start.AddMonths(1);
        return await _db.Db.Table<Transaction>()
            .Where(t => t.Date >= start && t.Date < end)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetByDateAsync(DateTime date)
    {
        await EnsureInitializedAsync();
        var start = date.Date;
        var end = start.AddDays(1);
        return await _db.Db.Table<Transaction>()
            .Where(t => t.Date >= start && t.Date < end)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetFilteredAsync(string? month, int? categoryId, string? currency)
    {
        await EnsureInitializedAsync();
        var query = _db.Db.Table<Transaction>();

        if (!string.IsNullOrWhiteSpace(month) &&
            DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var start))
        {
            var end = start.AddMonths(1);
            query = query.Where(t => t.Date >= start && t.Date < end);
        }

        if (categoryId is not null)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(currency))
            query = query.Where(t => t.Currency == currency);

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task AddAsync(Transaction transaction)
    {
        await EnsureInitializedAsync();
        await _db.Db.InsertAsync(transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        await EnsureInitializedAsync();
        await _db.Db.UpdateAsync(transaction);
    }

    public async Task DeleteAsync(int id)
    {
        await EnsureInitializedAsync();
        await _db.Db.DeleteAsync<Transaction>(id);
    }

    public async Task DeleteAllAsync()
    {
        await EnsureInitializedAsync();
        await _db.Db.DeleteAllAsync<Transaction>();
    }

    /// <summary>Returns (totalIncome, totalExpense) for a month in base currency.</summary>
    public async Task<(decimal Income, decimal Expense)> GetMonthSummaryAsync(string month)
    {
        var txns = await GetByMonthAsync(month);
        var baseCurrency = Preferences.Get("base_currency", "TWD");

        decimal income = 0, expense = 0;
        foreach (var t in txns)
        {
            var rate = await _currency.GetRateAsync(t.Currency, baseCurrency);
            var converted = t.Amount * (decimal)rate;
            if (t.Type == "income") income += converted;
            else expense += converted;
        }
        return (income, expense);
    }

    public async Task<List<Transaction>> GetRecentAsync(int count = 10)
    {
        await EnsureInitializedAsync();
        var all = await _db.Db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
        return all.Take(count).ToList();
    }
}
