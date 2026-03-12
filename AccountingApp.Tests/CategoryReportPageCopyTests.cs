namespace AccountingApp.Tests;

public class CategoryReportPageCopyTests
{
    [Fact]
    public void AppShell_includes_category_report_tab()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/AppShell.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("Title=\"分類報告\"", xaml);
        Assert.Contains("CategoryReportPage", xaml);
    }

    [Fact]
    public void CategoryReportPage_includes_range_filters_and_empty_state_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryReportPage.xaml"));

        Assert.True(File.Exists(path), $"Expected file to exist: {path}");

        var xaml = File.ReadAllText(path);

        Assert.Contains("分類報告", xaml);
        Assert.Contains("Week", xaml);
        Assert.Contains("Month", xaml);
        Assert.Contains("Year", xaml);
        Assert.Contains("All", xaml);
        Assert.Contains("此期間沒有支出紀錄", xaml);
        Assert.Contains("CategoryItems", xaml);
    }
}
