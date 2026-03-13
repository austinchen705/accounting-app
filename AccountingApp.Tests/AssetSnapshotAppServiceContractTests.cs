namespace AccountingApp.Tests;

public class AssetSnapshotAppServiceContractTests
{
    [Fact]
    public void AssetSnapshotService_declares_replace_import_flow()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Services/AssetSnapshotService.cs"));
        var source = File.ReadAllText(path);

        Assert.Contains("ReplaceImportCsvAsync", source);
        Assert.Contains("DeleteAllAsync<AssetSnapshot>", source);
    }
}
