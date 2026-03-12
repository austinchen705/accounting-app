using System.Globalization;

namespace AccountingApp.Core.Services;

public record StatisticsTrendInsightSummary(
    string IncomeMoMText,
    string ExpenseMoMText,
    string MaxExpenseText,
    string MinNetText);

public static class StatisticsTrendInsights
{
    public static StatisticsTrendInsightSummary Build(
        IReadOnlyList<(string Month, decimal Income, decimal Expense)> stats)
    {
        if (stats.Count == 0)
        {
            return new StatisticsTrendInsightSummary("--", "--", "--", "--");
        }

        var incomeMoMText = "--";
        var expenseMoMText = "--";

        if (stats.Count >= 2)
        {
            var latest = stats[^1];
            var previous = stats[^2];
            incomeMoMText = FormatMoM(previous.Income, latest.Income);
            expenseMoMText = FormatMoM(previous.Expense, latest.Expense);
        }

        var maxExpense = stats.MaxBy(s => s.Expense);
        var minNet = stats.MinBy(s => s.Income - s.Expense);
        var net = minNet.Income - minNet.Expense;

        return new StatisticsTrendInsightSummary(
            incomeMoMText,
            expenseMoMText,
            $"最高支出月：{maxExpense.Month} ({maxExpense.Expense:N0})",
            $"最低淨額月：{minNet.Month} ({net:N0})");
    }

    private static string FormatMoM(decimal previous, decimal current)
    {
        if (previous == 0)
        {
            return "--";
        }

        var ratio = ((current - previous) / previous) * 100;
        var value = ratio.ToString("0.0", CultureInfo.InvariantCulture);
        return ratio >= 0 ? $"+{value} %" : $"{value} %";
    }
}
