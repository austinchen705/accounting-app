namespace AccountingApp.Tests;

public class AssetTrendPageLayoutTests
{
    [Fact]
    public void AssetTrendPage_is_registered_in_shell_and_contains_trend_chart_inputs()
    {
        var shellXaml = ReadFile("../../../../AccountingApp/AppShell.xaml");
        var pageXaml = ReadFile("../../../../AccountingApp/Views/AssetTrendPage.xaml");

        Assert.Contains("AssetTrendPage", shellXaml);
        Assert.Contains("<lvc:CartesianChart", pageXaml);
        Assert.Contains("markup:Translate", pageXaml);
        Assert.Contains("AssetTrendStockLabel", pageXaml);
        Assert.Contains("FirstTrade", pageXaml);
        Assert.Contains("LatestTotalCaptionText", pageXaml);
        Assert.Contains("LatestTotalAmountText", pageXaml);
        Assert.Contains("CalendarDatePicker", pageXaml);
        Assert.Contains("InputContainerStyle", pageXaml);
    }

    [Fact]
    public void AssetTrendPage_wires_csv_import_command()
    {
        var pageXaml = ReadFile("../../../../AccountingApp/Views/AssetTrendPage.xaml");

        Assert.Contains("ImportCsvCommand", pageXaml);
        Assert.Contains("ImportErrorDetailsText", pageXaml);
        Assert.Contains("AssetTrendImportCsvButton", pageXaml);
        Assert.DoesNotContain("從 Google Drive 匯入", pageXaml);
    }

    [Fact]
    public void AssetTrendPage_wires_snapshot_edit_flow()
    {
        var pageXaml = ReadFile("../../../../AccountingApp/Views/AssetTrendPage.xaml");
        var codeBehind = ReadFile("../../../../AccountingApp/Views/AssetTrendPage.xaml.cs");

        Assert.Contains("EditSnapshotCommand", pageXaml);
        Assert.Contains("CancelEditCommand", pageXaml);
        Assert.Contains("IsEditing", pageXaml);
        Assert.Contains("FormTitleText", pageXaml);
        Assert.Contains("EditingSnapshotDisplayText", pageXaml);
        Assert.Contains("AssetTrendPropertyLabel", pageXaml);
        Assert.DoesNotContain("Fund3", pageXaml);
        Assert.Contains("ScrollToAsync", codeBehind);
    }

    private static string ReadFile(string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        return File.ReadAllText(path);
    }
}
