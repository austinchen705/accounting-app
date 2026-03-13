using CoreAssetSnapshotService = AccountingApp.Core.Services.AssetSnapshotService;
using AccountingApp.Core.Models;
using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public class AssetSnapshotService
{
    private readonly DatabaseService _databaseService;
    private readonly AccountingApp.Core.Services.DatabaseService _coreDatabaseService;
    private readonly CoreAssetSnapshotService _coreService;

    public AssetSnapshotService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _coreDatabaseService = new AccountingApp.Core.Services.DatabaseService(databaseService.DatabasePath);
        _coreService = new CoreAssetSnapshotService(_coreDatabaseService);
    }

    public async Task<List<AssetSnapshot>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        return await _coreService.GetAllAsync();
    }

    public async Task AddAsync(AssetSnapshot snapshot)
    {
        await EnsureInitializedAsync();
        await _coreService.AddAsync(snapshot);
    }

    public async Task UpdateAsync(AssetSnapshot snapshot)
    {
        await EnsureInitializedAsync();
        await _coreService.UpdateAsync(snapshot);
    }

    public async Task DeleteAsync(int id)
    {
        await EnsureInitializedAsync();
        await _coreService.DeleteAsync(id);
    }

    public async Task<AssetSnapshotImportResult> ImportCsvAsync(string csvContent)
    {
        await EnsureInitializedAsync();
        var result = AssetSnapshotCsvImporter.Parse(csvContent);
        foreach (var snapshot in result.Snapshots)
        {
            await _coreService.AddAsync(snapshot);
        }

        return result;
    }

    public async Task<AssetSnapshotImportResult> ReplaceImportCsvAsync(string csvContent)
    {
        await EnsureInitializedAsync();
        await _coreDatabaseService.Db.DeleteAllAsync<AssetSnapshot>();
        var result = AssetSnapshotCsvImporter.Parse(csvContent);
        foreach (var snapshot in result.Snapshots)
        {
            await _coreService.AddAsync(snapshot);
        }

        return result;
    }

    public AssetTrendSeriesResult BuildTrendSeries(IEnumerable<AssetSnapshot> snapshots) =>
        AssetTrendSeries.Build(snapshots);

    private async Task EnsureInitializedAsync()
    {
        await _databaseService.InitializeAsync();
        await _coreDatabaseService.InitializeAsync();
    }
}
