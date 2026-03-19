using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class CategoryReportTransactionDetailViewModelTests
{
    [Fact]
    public void BuildGroups_orders_transactions_by_date_and_newest_group_first()
    {
        var groups = ExpenseCategoryTransactionDetailReport.BuildGroups(
            [
                new ExpenseCategoryTransactionDetailStat(2, 1, "交通", "捷運", new DateTime(2025, 6, 5), 50, "TWD", 50),
                new ExpenseCategoryTransactionDetailStat(3, 1, "交通", "停車", new DateTime(2025, 6, 25), 80, "TWD", 80),
                new ExpenseCategoryTransactionDetailStat(4, 1, "交通", "高鐵", new DateTime(2025, 6, 25, 18, 0, 0), 120, "TWD", 120)
            ]);

        Assert.Equal("2025/06/25", groups[0].DateLabel);
        Assert.Equal("2025/06/05", groups[1].DateLabel);
        Assert.Equal([4, 3], groups[0].Items.Select(item => item.TransactionId).ToArray());
        Assert.All(groups, group => Assert.NotEmpty(group.Items));
    }

    [Fact]
    public void CategoryReportTransactionDetailViewModel_exposes_grouped_period_and_total_properties()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs"));

        Assert.True(File.Exists(path), $"Expected file to exist: {path}");

        var code = File.ReadAllText(path);

        Assert.Contains("ObservableCollection<TransactionDateGroup>", code);
        Assert.Contains("PeriodLabel", code);
        Assert.Contains("TotalAmountText", code);
    }
}
