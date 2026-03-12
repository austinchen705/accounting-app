namespace AccountingApp.Core.Services;

public static class StatisticsTrendWindow
{
    public static IReadOnlyList<DateTime> GetSixMonthWindow(DateTime anchorMonth)
    {
        var monthStart = new DateTime(anchorMonth.Year, anchorMonth.Month, 1);
        return Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(offset - 5))
            .ToArray();
    }
}
