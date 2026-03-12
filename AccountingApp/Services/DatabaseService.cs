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
        await EnsureIndexesAsync();

        await SeedDefaultCategoriesAsync();
    }

    /// <summary>
    /// Legacy data patch to normalize old type values. Trigger this when import is explicitly requested.
    /// </summary>
    public async Task ApplyImportPatchAsync()
    {
        if (_db is null) await InitializeAsync();
        await NormalizeLegacyTypeValuesAsync();
    }

    private async Task NormalizeLegacyTypeValuesAsync()
    {
        // Backward compatibility: old data may store type as Chinese text.
        await _db!.ExecuteAsync("UPDATE Categories SET Type = 'expense' WHERE Type IN ('支出', 'Expense', 'EXPENSE');");
        await _db.ExecuteAsync("UPDATE Categories SET Type = 'income' WHERE Type IN ('收入', 'Income', 'INCOME');");
        await _db.ExecuteAsync("UPDATE Transactions SET Type = 'expense' WHERE Type IN ('支出', 'Expense', 'EXPENSE');");
        await _db.ExecuteAsync("UPDATE Transactions SET Type = 'income' WHERE Type IN ('收入', 'Income', 'INCOME');");
    }

    private async Task EnsureIndexesAsync()
    {
        // Transaction query hotspots: month/date range + category/currency filters.
        await _db!.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Transactions_Date ON Transactions(Date);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Transactions_CategoryId ON Transactions(CategoryId);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Transactions_Currency ON Transactions(Currency);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Transactions_Type ON Transactions(Type);");

        // Category and budget lookups.
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Categories_Type ON Categories(Type);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Categories_Name_Type ON Categories(Name, Type);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Budgets_Month ON Budgets(Month);");
        await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Budgets_CategoryId_Month ON Budgets(CategoryId, Month);");
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
