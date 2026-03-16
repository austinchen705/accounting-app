using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class HomeDateWindowTests
{
    [Fact]
    public void GetDateWindow_returns_expected_day_week_month_year_and_all_windows()
    {
        var anchor = new DateTime(2026, 3, 12);

        var day = HomeDateWindow.GetDateWindow(HomeDateRange.Day, anchor);
        var week = HomeDateWindow.GetDateWindow(HomeDateRange.Week, anchor);
        var month = HomeDateWindow.GetDateWindow(HomeDateRange.Month, anchor);
        var year = HomeDateWindow.GetDateWindow(HomeDateRange.Year, anchor);
        var all = HomeDateWindow.GetDateWindow(HomeDateRange.All, anchor);

        Assert.Equal(new DateTime(2026, 3, 12), day.Start);
        Assert.Equal(new DateTime(2026, 3, 13), day.EndExclusive);
        Assert.Equal(new DateTime(2026, 3, 9), week.Start);
        Assert.Equal(new DateTime(2026, 3, 16), week.EndExclusive);
        Assert.Equal(new DateTime(2026, 3, 1), month.Start);
        Assert.Equal(new DateTime(2026, 4, 1), month.EndExclusive);
        Assert.Equal(new DateTime(2026, 1, 1), year.Start);
        Assert.Equal(new DateTime(2027, 1, 1), year.EndExclusive);
        Assert.Null(all.Start);
        Assert.Null(all.EndExclusive);
    }

    [Fact]
    public void MoveAnchor_shifts_day_week_month_and_year_but_not_all()
    {
        var anchor = new DateTime(2026, 3, 12);

        Assert.Equal(new DateTime(2026, 3, 13), HomeDateWindow.MoveAnchor(HomeDateRange.Day, anchor, 1));
        Assert.Equal(new DateTime(2026, 3, 19), HomeDateWindow.MoveAnchor(HomeDateRange.Week, anchor, 1));
        Assert.Equal(new DateTime(2026, 4, 1), HomeDateWindow.MoveAnchor(HomeDateRange.Month, anchor, 1));
        Assert.Equal(new DateTime(2027, 1, 1), HomeDateWindow.MoveAnchor(HomeDateRange.Year, anchor, 1));
        Assert.Equal(anchor.Date, HomeDateWindow.MoveAnchor(HomeDateRange.All, anchor, 1));
    }

    [Fact]
    public void GetPeriodLabel_formats_each_range_consistently()
    {
        var anchor = new DateTime(2026, 3, 12);

        Assert.Equal("2026/03/12", HomeDateWindow.GetPeriodLabel(HomeDateRange.Day, anchor));
        Assert.Equal("2026/03/09 - 03/15", HomeDateWindow.GetPeriodLabel(HomeDateRange.Week, anchor));
        Assert.Equal("2026/03", HomeDateWindow.GetPeriodLabel(HomeDateRange.Month, anchor));
        Assert.Equal("2026", HomeDateWindow.GetPeriodLabel(HomeDateRange.Year, anchor));
        Assert.Equal("全部期間", HomeDateWindow.GetPeriodLabel(HomeDateRange.All, anchor));
    }
}
