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

        Assert.Contains("截至所選月份的近 12 個月收支趨勢", xaml);
        Assert.Contains("前 5 大支出分類近 12 個月趨勢", xaml);
        Assert.Contains("CategoryTrendSeries", xaml);
    }

    [Fact]
    public void StatisticsViewModel_uses_shared_category_palette_for_chart_colors()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("CategoryColorPalette.GetHexColorForKey", code);
        Assert.DoesNotContain("private static readonly SKColor[] ChartColors", code);
    }
}
