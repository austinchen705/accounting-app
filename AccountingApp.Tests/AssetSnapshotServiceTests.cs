using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class AssetSnapshotServiceTests
{
    [Fact]
    public async Task AddAsync_rejects_negative_values()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new AssetSnapshotService(db.Service);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.AddAsync(new AssetSnapshot { Date = DateTime.Today, Stock = -1, Cash = 1, FirstTrade = 1, Fund3 = 1 }));

        Assert.Contains("non-negative", ex.Message);
    }

    [Fact]
    public async Task AddAsync_persists_valid_snapshot()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new AssetSnapshotService(db.Service);

        await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 2), Stock = 1, Cash = 2, FirstTrade = 3, Fund3 = 4 });

        var all = await svc.GetAllAsync();

        Assert.Single(all);
        Assert.Equal(10m, all[0].Stock + all[0].Cash + all[0].FirstTrade + all[0].Fund3);
    }

    [Fact]
    public async Task GetAllAsync_returns_snapshots_ordered_by_date_ascending()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new AssetSnapshotService(db.Service);

        await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 3), Stock = 1, Cash = 1, FirstTrade = 1, Fund3 = 1 });
        await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 1), Stock = 2, Cash = 2, FirstTrade = 2, Fund3 = 2 });

        var all = await svc.GetAllAsync();

        Assert.Equal(new DateTime(2026, 1, 1), all[0].Date);
        Assert.Equal(new DateTime(2026, 1, 3), all[1].Date);
    }

    [Fact]
    public async Task UpdateAsync_changes_stored_values()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new AssetSnapshotService(db.Service);

        await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 1), Stock = 1, Cash = 2, FirstTrade = 3, Fund3 = 4 });
        var snapshot = (await svc.GetAllAsync()).Single();
        snapshot.Stock = 50;
        snapshot.Cash = 60;

        await svc.UpdateAsync(snapshot);

        var updated = (await svc.GetAllAsync()).Single();
        Assert.Equal(50m, updated.Stock);
        Assert.Equal(60m, updated.Cash);
    }

    [Fact]
    public async Task DeleteAsync_removes_snapshot()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new AssetSnapshotService(db.Service);

        await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 1), Stock = 1, Cash = 2, FirstTrade = 3, Fund3 = 4 });
        var snapshot = (await svc.GetAllAsync()).Single();

        await svc.DeleteAsync(snapshot.Id);

        Assert.Empty(await svc.GetAllAsync());
    }
}
