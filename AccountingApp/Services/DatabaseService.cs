using SQLite;
using AccountingApp.Models;

namespace AccountingApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private const int SchemaVersion = 1;

    public string DatabasePath { get; }

    public DatabaseService()
    {
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "accounting.db");
    }

    public async Task InitializeAsync()
    {
        if (_db is not null) return;

        _db = new SQLiteAsyncConnection(DatabasePath);
        await _db.CreateTableAsync<Transaction>();
        await _db.CreateTableAsync<Category>();
        await _db.CreateTableAsync<Budget>();
        await _db.CreateTableAsync<ExchangeRateCache>();

        await SeedDefaultCategoriesAsync();
    }

    private async Task SeedDefaultCategoriesAsync()
    {
        var count = await _db!.Table<Category>().CountAsync();
        if (count > 0) return;

        var defaults = new List<Category>
        {
            new() { Name = "餐飲", Icon = "cat_food.png", Type = "expense" },
            new() { Name = "交通", Icon = "cat_transport.png", Type = "expense" },
            new() { Name = "娛樂", Icon = "cat_fun.png", Type = "expense" },
            new() { Name = "購物", Icon = "cat_shopping.png", Type = "expense" },
            new() { Name = "醫療", Icon = "cat_medical.png", Type = "expense" },
            new() { Name = "其他支出", Icon = "cat_other.png", Type = "expense" },
            new() { Name = "薪資", Icon = "cat_salary.png", Type = "income" },
            new() { Name = "其他收入", Icon = "cat_other.png", Type = "income" },
        };

        await _db.InsertAllAsync(defaults);
    }

    public SQLiteAsyncConnection Db => _db ?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
}
