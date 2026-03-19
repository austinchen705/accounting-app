namespace AccountingApp.Tests;

public class StatisticsPageTrendCopyTests
{
    [Fact]
    public void StatisticsPage_uses_12_month_trend_copy_and_includes_category_trend_section()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/StatisticsPage.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("StatisticsTrendSectionTitle", xaml);
        Assert.Contains("StatisticsCategoryTrendSectionTitle", xaml);
        Assert.Contains("CategoryTrendSeries", xaml);
        Assert.Contains("IsTop5Mode", xaml);
        Assert.Contains("IsSingleCategoryMode", xaml);
        Assert.Contains("AvailableExpenseCategories", xaml);
        Assert.Contains("SelectedExpenseCategory", xaml);
        Assert.Contains("InputContainerStyle", xaml);
        Assert.Contains("ItemDisplayBinding=\"{Binding Name}\"", xaml);
        Assert.Contains("StatisticsCategoryTrendTop5ModeLabel", xaml);
        Assert.Contains("StatisticsCategoryTrendSingleCategoryModeLabel", xaml);
        Assert.Contains("StatisticsCategoryTrendSelectCategoryPrompt", xaml);
    }

    [Fact]
    public void StatisticsViewModel_uses_shared_category_palette_for_chart_colors()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("CategoryColorPalette.BuildDistinctHexColors", code);
        Assert.Contains("colorByCategory[stat.CategoryName]", code);
        Assert.DoesNotContain("private static readonly SKColor[] ChartColors", code);
    }

    [Fact]
    public void StatisticsViewModel_uses_shared_localized_formatting_service_for_month_display()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("ILocalizedFormattingService", code);
        Assert.Contains("SelectedMonthLabel", code);
        Assert.Contains("FormatMonthYear", code);
        Assert.DoesNotContain("StringFormat='{0:yyyy年MM月}'", ReadStatisticsXaml());
    }

    [Fact]
    public void LocalizedFormattingService_exists_for_shared_period_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Services/LocalizedFormattingService.cs"));

        Assert.True(File.Exists(path));

        var code = File.ReadAllText(path);
        Assert.Contains("interface ILocalizedFormattingService", code);
        Assert.Contains("FormatMonthYear", code);
        Assert.Contains("FormatYear", code);
        Assert.Contains("FormatCategoryReportPeriod", code);
    }

    private static string ReadStatisticsXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/StatisticsPage.xaml"));

        return File.ReadAllText(path);
    }
}
