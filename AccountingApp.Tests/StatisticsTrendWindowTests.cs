using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class StatisticsTrendWindowTests
{
    [Fact]
    public void GetTwelveMonthWindow_returns_chronological_window_ending_at_anchor_month()
    {
        var months = StatisticsTrendWindow.GetTwelveMonthWindow(new DateTime(2026, 6, 1))
            .Select(date => date.ToString("yyyy-MM"))
            .ToArray();

        Assert.Equal(
            ["2025-07", "2025-08", "2025-09", "2025-10", "2025-11", "2025-12", "2026-01", "2026-02", "2026-03", "2026-04", "2026-05", "2026-06"],
            months);
    }

    [Fact]
    public void GetTwelveMonthWindow_crosses_year_boundary_when_anchor_is_january()
    {
        var months = StatisticsTrendWindow.GetTwelveMonthWindow(new DateTime(2026, 1, 1))
            .Select(date => date.ToString("yyyy-MM"))
            .ToArray();

        Assert.Equal(
            ["2025-02", "2025-03", "2025-04", "2025-05", "2025-06", "2025-07", "2025-08", "2025-09", "2025-10", "2025-11", "2025-12", "2026-01"],
            months);
    }
}
