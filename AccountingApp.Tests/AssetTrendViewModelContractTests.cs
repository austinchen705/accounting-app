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
        Assert.Contains("Property(房產)", vmCode);
        Assert.Contains("EditRequested", vmCode);
        Assert.Contains("ImportErrorDetails", vmCode);
        Assert.Contains("ReplaceImportCsvAsync", vmCode);
        Assert.Contains("確認匯入", vmCode);
        Assert.Contains("BuildCondensedDateLabels", vmCode);
        Assert.Contains("MinStep = 2_500_000", vmCode);
        Assert.Contains("ForceStepToMin = true", vmCode);
        Assert.Contains("Labeler = FormatYAxisValue", vmCode);
        Assert.DoesNotContain("ImportFromGoogleDriveCommand", vmCode);
        Assert.DoesNotContain("Drive debug", vmCode);
        Assert.DoesNotContain("ListBackupFolderFilesAsync()", vmCode);
    }
}
