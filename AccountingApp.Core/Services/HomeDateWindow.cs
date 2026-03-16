namespace AccountingApp.Core.Services;

public enum HomeDateRange
{
    Day,
    Week,
    Month,
    Year,
    All
}

public record HomeDateRangeWindow(DateTime? Start, DateTime? EndExclusive);

public static class HomeDateWindow
{
    public static HomeDateRangeWindow GetDateWindow(
        HomeDateRange range,
        DateTime anchorDate,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var date = anchorDate.Date;
        return range switch
        {
            HomeDateRange.Day => new(date, date.AddDays(1)),
            HomeDateRange.Week => BuildWeekWindow(date, weekStartsOn),
            HomeDateRange.Month => new(
                new DateTime(date.Year, date.Month, 1),
                new DateTime(date.Year, date.Month, 1).AddMonths(1)),
            HomeDateRange.Year => new(
                new DateTime(date.Year, 1, 1),
                new DateTime(date.Year + 1, 1, 1)),
            HomeDateRange.All => new(null, null),
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    public static DateTime MoveAnchor(HomeDateRange range, DateTime anchorDate, int delta)
    {
        var date = anchorDate.Date;
        return range switch
        {
            HomeDateRange.Day => date.AddDays(delta),
            HomeDateRange.Week => date.AddDays(7 * delta),
            HomeDateRange.Month => new DateTime(date.Year, date.Month, 1).AddMonths(delta),
            HomeDateRange.Year => new DateTime(date.Year, 1, 1).AddYears(delta),
            HomeDateRange.All => date,
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    public static string GetPeriodLabel(
        HomeDateRange range,
        DateTime anchorDate,
        DayOfWeek weekStartsOn = DayOfWeek.Monday)
    {
        var date = anchorDate.Date;
        return range switch
        {
            HomeDateRange.Day => date.ToString("yyyy/MM/dd"),
            HomeDateRange.Week => BuildWeekLabel(date, weekStartsOn),
            HomeDateRange.Month => date.ToString("yyyy/MM"),
            HomeDateRange.Year => date.ToString("yyyy"),
            HomeDateRange.All => "全部期間",
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    private static HomeDateRangeWindow BuildWeekWindow(DateTime anchorDate, DayOfWeek weekStartsOn)
    {
        var diff = (7 + (anchorDate.DayOfWeek - weekStartsOn)) % 7;
        var start = anchorDate.AddDays(-diff);
        return new HomeDateRangeWindow(start, start.AddDays(7));
    }

    private static string BuildWeekLabel(DateTime anchorDate, DayOfWeek weekStartsOn)
    {
        var window = BuildWeekWindow(anchorDate, weekStartsOn);
        var endDate = window.EndExclusive!.Value.AddDays(-1);
        return $"{window.Start:yyyy/MM/dd} - {endDate:MM/dd}";
    }
}
