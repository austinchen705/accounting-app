using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public class BudgetService
{
    private readonly DatabaseService _db;
    private readonly TransactionService _transactionService;
    private readonly CurrencyService _currencyService;
    private readonly IPreferenceStore _preferences;

    public BudgetService(
        DatabaseService db,
        TransactionService transactionService,
        CurrencyService currencyService,
        IPreferenceStore preferences)
    {
        _db = db;
        _transactionService = transactionService;
        _currencyService = currencyService;
        _preferences = preferences;
    }

    public async Task<List<Budget>> GetByMonthAsync(string month) =>
        await _db.Db.Table<Budget>().Where(b => b.Month == month).ToListAsync();

    public async Task SetBudgetAsync(int categoryId, decimal amount, string month)
    {
        var existing = await _db.Db.Table<Budget>()
            .Where(b => b.CategoryId == categoryId && b.Month == month)
            .FirstOrDefaultAsync();

        if (existing is null)
            await _db.Db.InsertAsync(new Budget { CategoryId = categoryId, Amount = amount, Month = month });
        else
        {
            existing.Amount = amount;
            await _db.Db.UpdateAsync(existing);
        }
    }

    public async Task<decimal> GetUsageRatioAsync(int categoryId, string month)
    {
        var budget = await _db.Db.Table<Budget>()
            .Where(b => b.CategoryId == categoryId && b.Month == month)
            .FirstOrDefaultAsync();
        if (budget is null || budget.Amount <= 0) return 0;

        var txns = await _transactionService.GetByMonthAsync(month);
        var baseCurrency = _preferences.Get("base_currency", "TWD");
        decimal spent = 0;
        foreach (var t in txns.Where(t => t.CategoryId == categoryId && t.Type == "expense"))
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            spent += t.Amount * (decimal)rate;
        }

        return spent / budget.Amount;
    }

    public async Task<bool> IsOver80PercentAsync(int categoryId, string month) =>
        await GetUsageRatioAsync(categoryId, month) >= 0.8m;

    public async Task<bool> ShouldNotifyAsync(int categoryId, string month)
    {
        var ratio = await GetUsageRatioAsync(categoryId, month);
        if (ratio < 0.8m) return false;

        var notifKey = $"notif_{categoryId}_{month}";
        return !_preferences.Get(notifKey, false);
    }

    public void MarkNotified(int categoryId, string month)
    {
        var notifKey = $"notif_{categoryId}_{month}";
        _preferences.Set(notifKey, true);
    }
}
