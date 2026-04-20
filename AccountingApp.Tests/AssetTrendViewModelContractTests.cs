namespace AccountingApp.Tests;

public class AssetTrendViewModelContractTests
{
    [Fact]
    public void AssetTrendViewModel_declares_expected_loading_and_command_surface()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/AssetTrendViewModel.cs"));
        var vmCode = File.ReadAllText(path);

        Assert.Contains("LoadAsync", vmCode);
        Assert.Contains("AddSnapshotCommand", vmCode);
        Assert.Contains("ImportCsvCommand", vmCode);
        Assert.Contains("ImportedCount", vmCode);
        Assert.Contains("SkippedCount", vmCode);
        Assert.Contains("EditSnapshotCommand", vmCode);
        Assert.Contains("CancelEditCommand", vmCode);
        Assert.Contains("IsEditing", vmCode);
        Assert.Contains("FormTitleText", vmCode);
        Assert.Contains("EditingSnapshotDisplayText", vmCode);
        Assert.Contains("AssetTrendPropertySeriesName", vmCode);
        Assert.Contains("EditRequested", vmCode);
        Assert.Contains("ImportErrorDetails", vmCode);
        Assert.Contains("ReplaceImportCsvAsync", vmCode);
        Assert.Contains("AssetTrendImportConfirmTitle", vmCode);
        Assert.Contains("AssetTrendImportSummaryFormat", vmCode);
        Assert.Contains("BuildCondensedDateLabels", vmCode);
        Assert.Contains("LatestTotalCaptionText", vmCode);
        Assert.Contains("LatestTotalAmountText", vmCode);
        Assert.Contains("ApplyLatestTotalSummary", vmCode);
        Assert.Contains("SummaryTrendSeries", vmCode);
        Assert.Contains("SummaryTrendXAxes", vmCode);
        Assert.Contains("SummaryTrendYAxes", vmCode);
        Assert.Contains("DetailTrendSeries", vmCode);
        Assert.Contains("DetailTrendXAxes", vmCode);
        Assert.Contains("DetailTrendYAxes", vmCode);
        Assert.Contains("BuildTrendSeries(trend, \"#111827\")", vmCode);
        Assert.Contains("BuildTrendSeries(trend, \"#F9FAFB\")", vmCode);
        Assert.Contains("AssetTrendChartAxisHelper", vmCode);
        Assert.DoesNotContain("ImportFromGoogleDriveCommand", vmCode);
        Assert.DoesNotContain("Drive debug", vmCode);
        Assert.DoesNotContain("ListBackupFolderFilesAsync()", vmCode);
    }
}
