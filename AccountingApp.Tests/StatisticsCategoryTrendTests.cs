using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class StatisticsCategoryTrendTests
{
    [Fact]
    public void BuildTopExpenseCategorySeries_ranks_by_window_total_and_fills_missing_months_with_zero()
    {
        var months = new[] { "2026-01", "2026-02", "2026-03" };
        var series = StatisticsCategoryTrend.BuildTopExpenseCategorySeries(
            months,
            [
                new ExpenseCategoryMonthValue(1, "餐飲", "2026-01", 100),
                new ExpenseCategoryMonthValue(1, "餐飲", "2026-03", 50),
                new ExpenseCategoryMonthValue(2, "交通", "2026-01", 40),
                new ExpenseCategoryMonthValue(2, "交通", "2026-02", 40),
                new ExpenseCategoryMonthValue(2, "交通", "2026-03", 40),
                new ExpenseCategoryMonthValue(3, "娛樂", "2026-02", 60)
            ],
            topCount: 2);

        Assert.Equal(2, series.Count);
        Assert.Equal("餐飲", series[0].CategoryName);
        Assert.Equal(new decimal[] { 100, 0, 50 }, series[0].Values);
        Assert.Equal("交通", series[1].CategoryName);
        Assert.Equal(new decimal[] { 40, 40, 40 }, series[1].Values);
    }

    [Fact]
    public void BuildTopExpenseCategorySeries_returns_only_categories_with_data_when_fewer_than_top_count()
    {
        var months = new[] { "2026-01", "2026-02" };
        var series = StatisticsCategoryTrend.BuildTopExpenseCategorySeries(
            months,
            [
                new ExpenseCategoryMonthValue(1, "餐飲", "2026-01", 100),
                new ExpenseCategoryMonthValue(2, "交通", "2026-02", 30)
            ],
            topCount: 5);

        Assert.Equal(2, series.Count);
        Assert.Equal("餐飲", series[0].CategoryName);
        Assert.Equal("交通", series[1].CategoryName);
    }

    [Fact]
    public void BuildSingleExpenseCategorySeries_fills_missing_months_with_zero()
    {
        var months = new[] { "2026-01", "2026-02", "2026-03" };
        var values = new[]
        {
            new ExpenseCategoryMonthValue(7, "Rent", "2026-01", 1200),
            new ExpenseCategoryMonthValue(7, "Rent", "2026-03", 1300)
        };

        var series = StatisticsCategoryTrend.BuildSingleExpenseCategorySeries(months, values, 7);

        Assert.Equal("Rent", series.CategoryName);
        Assert.Equal(new decimal[] { 1200, 0, 1300 }, series.Values);
    }
}
