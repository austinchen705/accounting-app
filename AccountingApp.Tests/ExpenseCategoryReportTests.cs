using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class ExpenseCategoryReportTests
{
    [Fact]
    public async Task BuildAsync_filters_selected_week_and_excludes_income_transactions()
    {
        var summary = await ExpenseCategoryReport.BuildAsync(
            [
                new ExpenseCategoryReportSourceTransaction(1, "TWD", 100, new DateTime(2026, 3, 9), "expense"),
                new ExpenseCategoryReportSourceTransaction(1, "TWD", 40, new DateTime(2026, 3, 10), "income"),
                new ExpenseCategoryReportSourceTransaction(2, "TWD", 50, new DateTime(2026, 3, 15), "expense"),
                new ExpenseCategoryReportSourceTransaction(3, "TWD", 999, new DateTime(2026, 3, 16), "expense")
            ],
            [
                new ExpenseCategoryReportCategory(1, "餐飲"),
                new ExpenseCategoryReportCategory(2, "交通"),
                new ExpenseCategoryReportCategory(3, "娛樂")
            ],
            ExpenseCategoryReportRange.Week,
            new DateTime(2026, 3, 12),
            "TWD",
            static (_, _) => Task.FromResult(1.0));

        Assert.Equal(150, summary.TotalExpense);
        Assert.Equal(2, summary.Categories.Count);
        Assert.Equal("餐飲", summary.Categories[0].CategoryName);
        Assert.Equal(1, summary.Categories[0].TransactionCount);
        Assert.Equal(100, summary.Categories[0].Amount);
        Assert.Equal("交通", summary.Categories[1].CategoryName);
        Assert.Equal(50, summary.Categories[1].Amount);
    }

    [Fact]
    public async Task BuildAsync_converts_amounts_to_base_currency_and_orders_ties_by_name()
    {
        var summary = await ExpenseCategoryReport.BuildAsync(
            [
                new ExpenseCategoryReportSourceTransaction(2, "USD", 5, new DateTime(2026, 7, 10), "expense"),
                new ExpenseCategoryReportSourceTransaction(1, "TWD", 150, new DateTime(2026, 7, 5), "expense"),
                new ExpenseCategoryReportSourceTransaction(3, "TWD", 20, new DateTime(2026, 7, 8), "expense")
            ],
            [
                new ExpenseCategoryReportCategory(1, "交通"),
                new ExpenseCategoryReportCategory(2, "餐飲"),
                new ExpenseCategoryReportCategory(3, "娛樂")
            ],
            ExpenseCategoryReportRange.Month,
            new DateTime(2026, 7, 1),
            "TWD",
            static (from, to) => Task.FromResult((from, to) switch
            {
                ("USD", "TWD") => 30.0,
                _ => 1.0
            }));

        Assert.Equal(320, summary.TotalExpense);
        Assert.Equal(3, summary.Categories.Count);
        Assert.Equal("交通", summary.Categories[0].CategoryName);
        Assert.Equal("餐飲", summary.Categories[1].CategoryName);
        Assert.Equal(150, summary.Categories[0].Amount);
        Assert.Equal(150, summary.Categories[1].Amount);
        Assert.Equal(20, summary.Categories[2].Amount);
    }

    [Fact]
    public async Task BuildAsync_returns_all_time_results_without_date_bounds()
    {
        var summary = await ExpenseCategoryReport.BuildAsync(
            [
                new ExpenseCategoryReportSourceTransaction(1, "TWD", 80, new DateTime(2025, 1, 1), "expense"),
                new ExpenseCategoryReportSourceTransaction(1, "TWD", 20, new DateTime(2026, 12, 31), "expense")
            ],
            [
                new ExpenseCategoryReportCategory(1, "餐飲")
            ],
            ExpenseCategoryReportRange.All,
            new DateTime(2026, 3, 12),
            "TWD",
            static (_, _) => Task.FromResult(1.0));

        Assert.Single(summary.Categories);
        Assert.Equal(100, summary.TotalExpense);
        Assert.Equal(2, summary.Categories[0].TransactionCount);
    }

    [Fact]
    public void GetDateWindow_returns_month_and_year_boundaries()
    {
        var monthWindow = ExpenseCategoryReport.GetDateWindow(
            ExpenseCategoryReportRange.Month,
            new DateTime(2026, 2, 18));
        var yearWindow = ExpenseCategoryReport.GetDateWindow(
            ExpenseCategoryReportRange.Year,
            new DateTime(2026, 2, 18));

        Assert.Equal(new DateTime(2026, 2, 1), monthWindow.Start);
        Assert.Equal(new DateTime(2026, 3, 1), monthWindow.EndExclusive);
        Assert.Equal(new DateTime(2026, 1, 1), yearWindow.Start);
        Assert.Equal(new DateTime(2027, 1, 1), yearWindow.EndExclusive);
    }
}
