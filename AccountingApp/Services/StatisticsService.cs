using AccountingApp.Models;
using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public record CategoryStat(string CategoryName, decimal Amount);
public record MonthStat(string Month, decimal Income, decimal Expense);

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

    public async Task<List<MonthStat>> GetLast6MonthsStatsAsync(DateTime anchorMonth)
    {
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var result = new List<MonthStat>();

        foreach (var date in StatisticsTrendWindow.GetSixMonthWindow(anchorMonth))
        {
            var month = date.ToString("yyyy-MM");
            var (income, expense) = await _transactionService.GetMonthSummaryAsync(month);
            result.Add(new MonthStat(date.ToString("MM月"), income, expense));
        }

        return result;
    }
}
