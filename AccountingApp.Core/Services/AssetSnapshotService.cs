using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public class AssetSnapshotService
{
    private readonly DatabaseService _db;

    public AssetSnapshotService(DatabaseService db)
    {
        _db = db;
    }

    public async Task AddAsync(AssetSnapshot snapshot)
    {
        Validate(snapshot);
        await _db.Db.InsertAsync(snapshot);
    }

    public async Task<List<AssetSnapshot>> GetAllAsync() =>
        await _db.Db.Table<AssetSnapshot>().OrderBy(x => x.Date).ToListAsync();

    public async Task UpdateAsync(AssetSnapshot snapshot)
    {
        Validate(snapshot);
        await _db.Db.UpdateAsync(snapshot);
    }

    public async Task DeleteAsync(int id) =>
        await _db.Db.DeleteAsync<AssetSnapshot>(id);

    private static void Validate(AssetSnapshot snapshot)
    {
        if (snapshot.Date == default)
        {
            throw new ArgumentException("Date is required.");
        }

        if (snapshot.Stock < 0 || snapshot.Cash < 0 || snapshot.FirstTrade < 0 || snapshot.Fund3 < 0)
        {
            throw new ArgumentException("Asset values must be non-negative.");
        }
    }
}
