namespace AccountingApp.Core.Services;

public record ExpenseCategoryMonthValue(int CategoryId, string CategoryName, string Month, decimal Amount);
public record ExpenseCategoryTrendSeries(string CategoryName, decimal[] Values);

public static class StatisticsCategoryTrend
{
    public static IReadOnlyList<ExpenseCategoryTrendSeries> BuildTopExpenseCategorySeries(
        IReadOnlyList<string> months,
        IReadOnlyList<ExpenseCategoryMonthValue> values,
        int topCount)
    {
        var topCategoryIds = values
            .GroupBy(v => new { v.CategoryId, v.CategoryName })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.CategoryName,
                Total = group.Sum(item => item.Amount)
            })
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.CategoryName, StringComparer.Ordinal)
            .Take(topCount)
            .ToList();

        return topCategoryIds
            .Select(category =>
            {
                var monthAmounts = values
                    .Where(v => v.CategoryId == category.CategoryId)
                    .ToDictionary(v => v.Month, v => v.Amount);

                var seriesValues = months
                    .Select(month => monthAmounts.TryGetValue(month, out var amount) ? amount : 0m)
                    .ToArray();

                return new ExpenseCategoryTrendSeries(category.CategoryName, seriesValues);
            })
            .ToArray();
    }

    public static ExpenseCategoryTrendSeries BuildSingleExpenseCategorySeries(
        IReadOnlyList<string> months,
        IReadOnlyList<ExpenseCategoryMonthValue> values,
        int categoryId)
    {
        var categoryValues = values
            .Where(v => v.CategoryId == categoryId)
            .ToList();

        var categoryName = categoryValues
            .Select(v => v.CategoryName)
            .FirstOrDefault()
            ?? string.Empty;
        var monthAmounts = categoryValues.ToDictionary(v => v.Month, v => v.Amount);
        var seriesValues = months
            .Select(month => monthAmounts.TryGetValue(month, out var amount) ? amount : 0m)
            .ToArray();

        return new ExpenseCategoryTrendSeries(categoryName, seriesValues);
    }
}
