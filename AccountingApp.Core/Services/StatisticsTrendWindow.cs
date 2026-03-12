namespace AccountingApp.Core.Services;

public static class StatisticsTrendWindow
{
    public static IReadOnlyList<DateTime> GetTwelveMonthWindow(DateTime anchorMonth)
    {
        var monthStart = new DateTime(anchorMonth.Year, anchorMonth.Month, 1);
        return Enumerable.Range(0, 12)
            .Select(offset => monthStart.AddMonths(offset - 11))
            .ToArray();
    }
}
