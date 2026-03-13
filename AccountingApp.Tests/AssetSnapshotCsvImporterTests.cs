using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AssetSnapshotCsvImporterTests
{
    [Fact]
    public void Import_skips_invalid_rows_and_reports_counts()
    {
        var csv = "Date,Stock,Cash,FirstTrade,Property\n2026-01-01,1,2,3,4\nBAD,1,2,3,4";

        var result = AssetSnapshotCsvImporter.Parse(csv);

        Assert.Single(result.Snapshots);
        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Single(result.ErrorDetails);
        Assert.Contains("row 3", result.ErrorDetails[0], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4m, result.Snapshots[0].Property);
    }
}
