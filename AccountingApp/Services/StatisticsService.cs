using AccountingApp.Models;
using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public record CategoryStat(string CategoryName, decimal Amount);
public record MonthStat(string Month, decimal Income, decimal Expense);
public record ExpenseCategoryTrendStat(string CategoryName, decimal[] Values);

public class StatisticsService
{
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
    private readonly CurrencyService _currencyService;

    public StatisticsService(TransactionService transactionService, CategoryService categoryService, CurrencyService currencyService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _currencyService = currencyService;
    }

    public async Task<List<CategoryStat>> GetMonthCategoryStatsAsync(string month)
    {
        var txns = await _transactionService.GetByMonthAsync(month);
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var categories = await _categoryService.GetAllAsync();

        var grouped = new Dictionary<int, decimal>();
        foreach (var t in txns.Where(t => t.Type == "expense"))
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            grouped.TryAdd(t.CategoryId, 0);
            grouped[t.CategoryId] += t.Amount * (decimal)rate;
        }

        return grouped
            .Select(kv => new CategoryStat(
                categories.FirstOrDefault(c => c.Id == kv.Key)?.Name ?? "未知",
                kv.Value))
            .OrderByDescending(s => s.Amount)
            .ToList();
    }

    public async Task<List<MonthStat>> GetLast12MonthsStatsAsync(DateTime anchorMonth)
    {
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var result = new List<MonthStat>();

        foreach (var date in StatisticsTrendWindow.GetTwelveMonthWindow(anchorMonth))
        {
            var month = date.ToString("yyyy-MM");
            var (income, expense) = await _transactionService.GetMonthSummaryAsync(month);
            result.Add(new MonthStat(date.ToString("MM月"), income, expense));
        }

        return result;
    }

    public async Task<List<ExpenseCategoryTrendStat>> GetTopExpenseCategoryTrendAsync(DateTime anchorMonth, int topCount = 5)
    {
        var months = StatisticsTrendWindow.GetTwelveMonthWindow(anchorMonth)
            .Select(date => date.ToString("yyyy-MM"))
            .ToArray();
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var categories = await _categoryService.GetAllAsync();
        var values = new List<ExpenseCategoryMonthValue>();

        foreach (var month in months)
        {
            var txns = await _transactionService.GetByMonthAsync(month);
            var grouped = new Dictionary<int, decimal>();

            foreach (var txn in txns.Where(t => t.Type == "expense"))
            {
                var rate = await _currencyService.GetRateAsync(txn.Currency, baseCurrency);
                grouped.TryAdd(txn.CategoryId, 0);
                grouped[txn.CategoryId] += txn.Amount * (decimal)rate;
            }

            foreach (var (categoryId, amount) in grouped)
            {
                var name = categories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? "未知";
                values.Add(new ExpenseCategoryMonthValue(categoryId, name, month, amount));
            }
        }

        return StatisticsCategoryTrend.BuildTopExpenseCategorySeries(months, values, topCount)
            .Select(series => new ExpenseCategoryTrendStat(series.CategoryName, series.Values))
            .ToList();
    }
}
