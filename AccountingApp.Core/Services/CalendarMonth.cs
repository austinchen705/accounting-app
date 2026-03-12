namespace AccountingApp.Core.Services;

public record CalendarDayCell(DateTime Date, bool IsCurrentMonth);

public static class CalendarMonth
{
    public static IReadOnlyList<CalendarDayCell> BuildGrid(DateTime visibleMonth)
    {
        var monthStart = new DateTime(visibleMonth.Year, visibleMonth.Month, 1);
        var gridStart = monthStart.AddDays(-(int)monthStart.DayOfWeek);

        return Enumerable.Range(0, 42)
            .Select(offset =>
            {
                var date = gridStart.AddDays(offset);
                return new CalendarDayCell(date, date.Month == visibleMonth.Month && date.Year == visibleMonth.Year);
            })
            .ToArray();
    }
}
