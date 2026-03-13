using System.Globalization;
using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public record AssetSnapshotImportResult(
    IReadOnlyList<AssetSnapshot> Snapshots,
    int ImportedCount,
    int SkippedCount,
    IReadOnlyList<string> ErrorDetails);

public static class AssetSnapshotCsvImporter
{
    public static AssetSnapshotImportResult Parse(string csvContent)
    {
        var snapshots = new List<AssetSnapshot>();
        var errorDetails = new List<string>();
        var skippedCount = 0;
        var lines = csvContent
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .ToArray();

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var columns = line.Split(',');
            var rowNumber = index + 2;
            if (columns.Length < 5 ||
                !DateTime.TryParse(columns[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                !decimal.TryParse(columns[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var stock) ||
                !decimal.TryParse(columns[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var cash) ||
                !decimal.TryParse(columns[3], NumberStyles.Number, CultureInfo.InvariantCulture, out var firstTrade) ||
                !decimal.TryParse(columns[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var property))
            {
                skippedCount++;
                errorDetails.Add($"Skipped row {rowNumber}: invalid date or numeric value.");
                continue;
            }

            snapshots.Add(new AssetSnapshot
            {
                Date = date,
                Stock = stock,
                Cash = cash,
                FirstTrade = firstTrade,
                Property = property
            });
        }

        return new AssetSnapshotImportResult(snapshots, snapshots.Count, skippedCount, errorDetails);
    }
}
