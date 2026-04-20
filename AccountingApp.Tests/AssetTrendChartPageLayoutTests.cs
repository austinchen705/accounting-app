namespace AccountingApp.Tests;

public class AssetTrendChartPageLayoutTests
{
    [Fact]
    public void AssetTrendChartPage_exists_and_contains_cartesian_chart_layout()
    {
        var pageCodeBehind = ReadFile("../../../../AccountingApp/Views/AssetTrendChartPage.xaml.cs");
        var shellXaml = ReadFile("../../../../AccountingApp/AppShell.xaml");
        var shellCode = ReadFile("../../../../AccountingApp/AppShell.xaml.cs");
        var pageXaml = ReadFile("../../../../AccountingApp/Views/AssetTrendChartPage.xaml");

        Assert.Contains("AssetTrendChartTab", shellXaml);
        Assert.Contains("views:AssetTrendChartPage", shellXaml);
        Assert.Contains("AssetTrendChartPage", shellCode);
        Assert.Contains("AssetTrendChartPageTitle", shellCode);
        Assert.Contains("<lvc:CartesianChart", pageXaml);
        Assert.Contains("AssetTrendChartPageTitle", pageXaml);
        Assert.Contains("DetailTrendSeries", pageXaml);
        Assert.Contains("DetailTrendXAxes", pageXaml);
        Assert.Contains("DetailTrendYAxes", pageXaml);
        Assert.Contains("LoadAsync", pageCodeBehind);
        Assert.DoesNotContain("AssetTrendHistorySectionTitle", pageXaml);
    }

    private static string ReadFile(string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        return File.ReadAllText(path);
    }
}
