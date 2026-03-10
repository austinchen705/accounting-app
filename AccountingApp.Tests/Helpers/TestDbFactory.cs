using AccountingApp.Core.Services;
using SQLite;

namespace AccountingApp.Tests.Helpers;

public class TestDb : IAsyncDisposable
{
    private readonly string _path;
    public DatabaseService Service { get; }

    private TestDb(string path)
    {
        _path = path;
        Service = new DatabaseService(path);
    }

    public static async Task<TestDb> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var db = new TestDb(path);
        await db.Service.InitializeAsync();
        return db;
    }

    public async ValueTask DisposeAsync()
    {
        // Close the SQLite connection before deleting
        await Service.Db.CloseAsync();
        if (File.Exists(_path)) File.Delete(_path);
    }
}
