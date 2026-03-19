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
        Assert.Contains("CategoryReportRangeWeek", xaml);
        Assert.Contains("CategoryReportRangeMonth", xaml);
        Assert.Contains("CategoryReportRangeYear", xaml);
        Assert.Contains("CategoryReportRangeAll", xaml);
        Assert.Contains("CategoryReportEmptyStateText", xaml);
        Assert.Contains("CategoryItems", xaml);
        Assert.Contains("OpenCategoryDetailCommand", xaml);
        Assert.Contains("SelectionMode=\"Single\"", xaml);
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
        Assert.Contains("ILocalizedFormattingService", code);
        Assert.Contains("FormatCategoryReportPeriod", code);
        Assert.Contains("OpenCategoryDetailCommand", code);
        Assert.Contains("_selectedRange", code);
        Assert.Contains("_anchorDate", code);
        Assert.Contains("Shell.Current.GoToAsync", code);
        Assert.Contains("CategoryReportTransactionDetailPage", code);
        Assert.Contains("categoryId={item.CategoryId}", code);
        Assert.Contains("categoryName={Uri.EscapeDataString(item.CategoryName)}", code);
        Assert.Contains("range={_selectedRange}", code);
        Assert.Contains("anchorDate={Uri.EscapeDataString(_anchorDate.ToString(\"O\"))}", code);
    }

    [Fact]
    public void CategoryReportPage_return_flow_keeps_existing_range_state_by_reloading_only()
    {
        var pagePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryReportPage.xaml.cs"));
        var viewModelPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryReportViewModel.cs"));

        var pageCode = File.ReadAllText(pagePath);
        var viewModelCode = File.ReadAllText(viewModelPath);

        Assert.Contains("await _vm.LoadAsync();", pageCode);
        Assert.DoesNotContain("SetRange(", pageCode);
        Assert.DoesNotContain("_anchorDate = DateTime.Today;", pageCode);
        Assert.DoesNotContain("LoadAsync()\n    {\n        _selectedRange =", viewModelCode);
        Assert.DoesNotContain("LoadAsync()\n    {\n        _anchorDate =", viewModelCode);
    }
}
