namespace AccountingApp.Tests;

public class GoogleDriveServiceTests
{
    [Fact]
    public void GoogleDriveService_declares_csv_listing_support_for_asset_import()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Services/GoogleDriveService.cs"));
        var source = File.ReadAllText(path);

        Assert.Contains("GoogleDriveFileItem", source);
        Assert.Contains("FilterCsvFiles", source);
        Assert.Contains("ListBackupFolderFilesAsync", source);
    }
}
