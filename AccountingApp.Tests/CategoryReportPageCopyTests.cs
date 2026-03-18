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

        Assert.Contains("CategoryReportPage", xaml);
        Assert.Contains("CategoryReportTabTitle", xaml);
        Assert.Contains("markup:Translate", xaml);
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

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("CategoryReportPageTitle", xaml);
        Assert.Contains("Week", xaml);
        Assert.Contains("Month", xaml);
        Assert.Contains("Year", xaml);
        Assert.Contains("All", xaml);
        Assert.Contains("CategoryReportEmptyStateText", xaml);
        Assert.Contains("CategoryItems", xaml);
    }

    [Fact]
    public void CategoryReportViewModel_uses_shared_category_palette_by_category_name()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryReportViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("CategoryColorPalette.BuildDistinctHexColors", code);
        Assert.Contains("colorByCategory[category.CategoryName]", code);
    }
}
