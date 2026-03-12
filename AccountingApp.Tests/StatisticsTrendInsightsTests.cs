using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class StatisticsTrendInsightsTests
{
    [Fact]
    public void Build_uses_last_two_months_for_mom_and_window_values_for_summaries()
    {
        var summary = StatisticsTrendInsights.Build(
            [
                ("08月", 100m, 40m),
                ("09月", 120m, 60m),
                ("10月", 90m, 110m),
                ("11月", 150m, 70m),
                ("12月", 170m, 80m),
                ("01月", 204m, 100m)
            ]);

        Assert.Equal("+20.0 %", summary.IncomeMoMText);
        Assert.Equal("+25.0 %", summary.ExpenseMoMText);
        Assert.Equal("最高支出月：10月 (110)", summary.MaxExpenseText);
        Assert.Equal("最低淨額月：10月 (-20)", summary.MinNetText);
    }

    [Fact]
    public void Build_returns_placeholders_when_no_stats_exist()
    {
        var summary = StatisticsTrendInsights.Build([]);

        Assert.Equal("--", summary.IncomeMoMText);
        Assert.Equal("--", summary.ExpenseMoMText);
        Assert.Equal("--", summary.MaxExpenseText);
        Assert.Equal("--", summary.MinNetText);
    }
}
