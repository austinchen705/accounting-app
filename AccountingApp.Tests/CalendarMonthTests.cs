using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class CalendarMonthTests
{
    [Fact]
    public void BuildGrid_returns_42_cells_with_leading_days_from_previous_month()
    {
        var cells = CalendarMonth.BuildGrid(new DateTime(2026, 7, 15));

        Assert.Equal(42, cells.Count);
        Assert.Equal(new DateTime(2026, 6, 28), cells[0].Date);
        Assert.False(cells[0].IsCurrentMonth);
        Assert.Equal(new DateTime(2026, 7, 1), cells[3].Date);
        Assert.True(cells[3].IsCurrentMonth);
    }

    [Fact]
    public void BuildGrid_crosses_year_boundary_for_january()
    {
        var cells = CalendarMonth.BuildGrid(new DateTime(2026, 1, 10));

        Assert.Equal(new DateTime(2025, 12, 28), cells[0].Date);
        Assert.Equal(new DateTime(2026, 2, 7), cells[^1].Date);
    }
}
