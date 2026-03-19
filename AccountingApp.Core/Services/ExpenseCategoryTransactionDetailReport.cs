namespace AccountingApp.Core.Services;

public record ExpenseCategoryReportDetailSourceTransaction(
    int TransactionId,
    int CategoryId,
    string Note,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Type);

public record ExpenseCategoryTransactionDetailStat(
    int TransactionId,
    int CategoryId,
    string CategoryName,
    string Note,
    DateTime Date,
    decimal Amount,
    string Currency,
    decimal ConvertedAmount);

public record ExpenseCategoryTransactionDateGroup(
    string DateLabel,
    IReadOnlyList<ExpenseCategoryTransactionDetailStat> Items);

public static class ExpenseCategoryTransactionDetailReport
{
    public static async Task<IReadOnlyList<ExpenseCategoryTransactionDetailStat>> BuildAsync(
        IEnumerable<ExpenseCategoryReportDetailSourceTransaction> transactions,
        int categoryId,
        string categoryName,
        ExpenseCategoryReportRange range,
        DateTime anchorDate,
        string baseCurrency,
        Func<string, string, Task<double>> rateResolver,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var window = ExpenseCategoryReport.GetDateWindow(range, anchorDate, weekStartsOn);
        var results = new List<ExpenseCategoryTransactionDetailStat>();

        foreach (var transaction in transactions)
        {
            if (!string.Equals(transaction.Type, "expense", StringComparison.OrdinalIgnoreCase) ||
                transaction.CategoryId != categoryId)
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
            results.Add(new ExpenseCategoryTransactionDetailStat(
                transaction.TransactionId,
                transaction.CategoryId,
                categoryName,
                transaction.Note,
                transaction.Date,
                transaction.Amount,
                transaction.Currency,
                transaction.Amount * (decimal)rate));
        }

        return results
            .OrderByDescending(detail => detail.Date)
            .ThenByDescending(detail => detail.TransactionId)
            .ToList();
    }

    public static IReadOnlyList<ExpenseCategoryTransactionDateGroup> BuildGroups(
        IEnumerable<ExpenseCategoryTransactionDetailStat> details)
    {
        return details
            .OrderByDescending(detail => detail.Date)
            .ThenByDescending(detail => detail.TransactionId)
            .GroupBy(detail => detail.Date.Date)
            .Select(group => new ExpenseCategoryTransactionDateGroup(
                group.Key.ToString("yyyy/MM/dd"),
                group.ToList()))
            .ToList();
    }
}
