using AccountingApp.Models;
using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public record CategoryStat(string CategoryName, decimal Amount);
public record MonthStat(string Month, decimal Income, decimal Expense);
public record ExpenseCategoryTrendStat(string CategoryName, decimal[] Values);
public record ExpenseCategoryOptionStat(int CategoryId, string CategoryName);
public record ExpenseCategoryReportItemStat(string CategoryName, int TransactionCount, decimal Amount);
public record ExpenseCategoryReportStat(decimal TotalExpense, IReadOnlyList<ExpenseCategoryReportItemStat> Categories);

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
        var values = await BuildExpenseCategoryMonthValuesAsync(months);

        return StatisticsCategoryTrend.BuildTopExpenseCategorySeries(months, values, topCount)
            .Select(series => new ExpenseCategoryTrendStat(series.CategoryName, series.Values))
            .ToList();
    }

    public async Task<ExpenseCategoryTrendStat?> GetExpenseCategoryTrendAsync(DateTime anchorMonth, int categoryId)
    {
        var months = StatisticsTrendWindow.GetTwelveMonthWindow(anchorMonth)
            .Select(date => date.ToString("yyyy-MM"))
            .ToArray();
        var values = await BuildExpenseCategoryMonthValuesAsync(months);
        var series = StatisticsCategoryTrend.BuildSingleExpenseCategorySeries(months, values, categoryId);

        return string.IsNullOrWhiteSpace(series.CategoryName)
            ? null
            : new ExpenseCategoryTrendStat(series.CategoryName, series.Values);
    }

    public async Task<IReadOnlyList<ExpenseCategoryOptionStat>> GetExpenseCategoriesAsync()
    {
        return (await _categoryService.GetByTypeAsync("expense"))
            .OrderBy(category => category.Name, StringComparer.Ordinal)
            .Select(category => new ExpenseCategoryOptionStat(category.Id, category.Name))
            .ToList();
    }

    public async Task<ExpenseCategoryReportStat> GetExpenseCategoryReportAsync(
        ExpenseCategoryReportRange range,
        DateTime anchorDate)
    {
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var transactions = await _transactionService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();

        var summary = await ExpenseCategoryReport.BuildAsync(
            transactions.Select(transaction => new ExpenseCategoryReportSourceTransaction(
                transaction.CategoryId,
                transaction.Currency,
                transaction.Amount,
                transaction.Date,
                transaction.Type)),
            categories.Select(category => new ExpenseCategoryReportCategory(category.Id, category.Name)),
            range,
            anchorDate,
            baseCurrency,
            _currencyService.GetRateAsync);

        return new ExpenseCategoryReportStat(
            summary.TotalExpense,
            summary.Categories
                .Select(category => new ExpenseCategoryReportItemStat(
                    category.CategoryName,
                    category.TransactionCount,
                    category.Amount))
                .ToList());
    }

    private async Task<List<ExpenseCategoryMonthValue>> BuildExpenseCategoryMonthValuesAsync(IReadOnlyList<string> months)
    {
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var categories = await _categoryService.GetAllAsync();
        var categoryNameById = categories.ToDictionary(category => category.Id, category => category.Name);
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
                categoryNameById.TryGetValue(categoryId, out var categoryName);
                values.Add(new ExpenseCategoryMonthValue(categoryId, categoryName ?? "未知", month, amount));
            }
        }

        return values;
    }
}
