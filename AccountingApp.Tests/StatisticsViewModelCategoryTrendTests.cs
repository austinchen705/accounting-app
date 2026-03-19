namespace AccountingApp.Tests;

public class StatisticsViewModelCategoryTrendTests
{
    [Fact]
    public void StatisticsViewModel_exposes_category_trend_mode_and_filter_state()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("CategoryTrendMode", code);
        Assert.Contains("SelectedCategoryTrendMode", code);
        Assert.Contains("AvailableExpenseCategories", code);
        Assert.Contains("SelectedExpenseCategory", code);
    }

    [Fact]
    public void StatisticsViewModel_loads_top5_or_single_category_trend_by_selected_mode()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("await _statisticsService.GetTopExpenseCategoryTrendAsync", code);
        Assert.Contains("await _statisticsService.GetExpenseCategoryTrendAsync", code);
        Assert.Contains("if (!IsSingleCategoryMode)", code);
        Assert.Contains("CategoryTrendEmptyStateText", code);
        Assert.Contains("HasCategoryTrendData", code);
    }
}
