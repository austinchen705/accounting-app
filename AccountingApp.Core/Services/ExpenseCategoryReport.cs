namespace AccountingApp.Core.Services;

public enum ExpenseCategoryReportRange
{
    Week,
    Month,
    Year,
    All
}

public record ExpenseCategoryReportDateWindow(DateTime? Start, DateTime? EndExclusive);

public record ExpenseCategoryReportSourceTransaction(
    int CategoryId,
    string Currency,
    decimal Amount,
    DateTime Date,
    string Type);

public record ExpenseCategoryReportCategory(int CategoryId, string CategoryName);

public record ExpenseCategoryReportRow(
    int CategoryId,
    string CategoryName,
    int TransactionCount,
    decimal Amount);

public record ExpenseCategoryReportSummary(
    decimal TotalExpense,
    IReadOnlyList<ExpenseCategoryReportRow> Categories);

public static class ExpenseCategoryReport
{
    public static ExpenseCategoryReportDateWindow GetDateWindow(
        ExpenseCategoryReportRange range,
        DateTime anchorDate,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var date = anchorDate.Date;
        return range switch
        {
            ExpenseCategoryReportRange.Week => BuildWeekWindow(date, weekStartsOn),
            ExpenseCategoryReportRange.Month => new(
                new DateTime(date.Year, date.Month, 1),
                new DateTime(date.Year, date.Month, 1).AddMonths(1)),
            ExpenseCategoryReportRange.Year => new(
                new DateTime(date.Year, 1, 1),
                new DateTime(date.Year + 1, 1, 1)),
            ExpenseCategoryReportRange.All => new(null, null),
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    public static async Task<ExpenseCategoryReportSummary> BuildAsync(
        IEnumerable<ExpenseCategoryReportSourceTransaction> transactions,
        IEnumerable<ExpenseCategoryReportCategory> categories,
        ExpenseCategoryReportRange range,
        DateTime anchorDate,
        string baseCurrency,
        Func<string, string, Task<double>> rateResolver,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var window = GetDateWindow(range, anchorDate, weekStartsOn);
        var categoryNames = categories.ToDictionary(category => category.CategoryId, category => category.CategoryName);
        var grouped = new Dictionary<int, (decimal Amount, int Count)>();

        foreach (var transaction in transactions)
        {
            if (!string.Equals(transaction.Type, "expense", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var date = transaction.Date.Date;
            if (window.Start is not null && date < window.Start.Value)
            {
                continue;
            }

            if (window.EndExclusive is not null && date >= window.EndExclusive.Value)
            {
                continue;
            }

            var rate = await rateResolver(transaction.Currency, baseCurrency);
            var convertedAmount = transaction.Amount * (decimal)rate;
            grouped.TryAdd(transaction.CategoryId, (0, 0));
            var current = grouped[transaction.CategoryId];
            grouped[transaction.CategoryId] = (current.Amount + convertedAmount, current.Count + 1);
        }

        var rows = grouped
            .Select(entry => new ExpenseCategoryReportRow(
                entry.Key,
                categoryNames.TryGetValue(entry.Key, out var name) ? name : "未知",
                entry.Value.Count,
                entry.Value.Amount))
            .OrderByDescending(row => row.Amount)
            .ThenBy(row => row.CategoryName, StringComparer.Ordinal)
            .ToList();

        return new ExpenseCategoryReportSummary(rows.Sum(row => row.Amount), rows);
    }

    private static ExpenseCategoryReportDateWindow BuildWeekWindow(DateTime anchorDate, DayOfWeek weekStartsOn)
    {
        var diff = (7 + (anchorDate.DayOfWeek - weekStartsOn)) % 7;
        var start = anchorDate.AddDays(-diff);
        return new ExpenseCategoryReportDateWindow(start, start.AddDays(7));
    }
}
