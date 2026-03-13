using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class AssetSnapshotDatabaseSchemaTests
{
    [Fact]
    public async Task Initialize_creates_asset_snapshot_table()
    {
        await using var db = await TestDb.CreateAsync();

        var tableInfo = await db.Service.Db.GetTableInfoAsync("AssetSnapshot");

        Assert.NotEmpty(tableInfo);
    }
}
