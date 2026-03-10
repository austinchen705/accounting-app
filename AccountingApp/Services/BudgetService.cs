using AccountingApp.Models;

namespace AccountingApp.Services;

public class BudgetService
{
    private readonly DatabaseService _db;
    private readonly TransactionService _transactionService;
    private readonly CurrencyService _currencyService;

    public BudgetService(DatabaseService db, TransactionService transactionService, CurrencyService currencyService)
    {
        _db = db;
        _transactionService = transactionService;
        _currencyService = currencyService;
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

    /// <summary>Returns usage ratio (0.0–1.0+) for a category in a month.</summary>
    public async Task<decimal> GetUsageRatioAsync(int categoryId, string month)
    {
        var budget = await _db.Db.Table<Budget>()
            .Where(b => b.CategoryId == categoryId && b.Month == month)
            .FirstOrDefaultAsync();
        if (budget is null || budget.Amount <= 0) return 0;

        var txns = await _transactionService.GetByMonthAsync(month);
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        decimal spent = 0;
        foreach (var t in txns.Where(t => t.CategoryId == categoryId && t.Type == "expense"))
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            spent += t.Amount * (decimal)rate;
        }

        return spent / budget.Amount;
    }

    /// <summary>Returns (categoryId, usageRatio) for all budgeted categories this month.</summary>
    public async Task<List<(int CategoryId, decimal BudgetAmount, decimal SpentAmount, decimal Ratio)>> GetMonthUsageAsync(string month)
    {
        var budgets = await GetByMonthAsync(month);
        var result = new List<(int, decimal, decimal, decimal)>();
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var txns = await _transactionService.GetByMonthAsync(month);

        foreach (var b in budgets)
        {
            decimal spent = 0;
            foreach (var t in txns.Where(t => t.CategoryId == b.CategoryId && t.Type == "expense"))
            {
                var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
                spent += t.Amount * (decimal)rate;
            }
            result.Add((b.CategoryId, b.Amount, spent, b.Amount > 0 ? spent / b.Amount : 0));
        }
        return result;
    }

    public async Task CheckAndNotifyAsync(int categoryId, string month)
    {
        var ratio = await GetUsageRatioAsync(categoryId, month);
        if (ratio < 0.8m) return;

        var notifKey = $"notif_{categoryId}_{month}";
        var alreadyNotified = Preferences.Get(notifKey, false);
        if (alreadyNotified) return;

        Preferences.Set(notifKey, true);
        await SendLocalNotificationAsync(categoryId, ratio);
    }

    private async Task SendLocalNotificationAsync(int categoryId, decimal ratio)
    {
        var pct = (int)(ratio * 100);
        await NotificationService.SendAsync(
            "預算提醒",
            $"分類 #{categoryId} 本月支出已達預算 {pct}%，請注意！");
    }
}
