using AccountingApp.Models;

namespace AccountingApp.Services;

public class CategoryService
{
    private readonly DatabaseService _db;

    public CategoryService(DatabaseService db) => _db = db;

    private async Task EnsureInitializedAsync()
    {
        await _db.InitializeAsync();
    }

    private static string NormalizeType(string? type)
    {
        var value = (type ?? "expense").Trim();
        return value switch
        {
            "收入" => "income",
            "支出" => "expense",
            _ => value.ToLowerInvariant()
        };
    }

    public async Task<List<Category>> GetAllAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Db.Table<Category>().ToListAsync();
    }

    public async Task<List<Category>> GetByTypeAsync(string type)
    {
        await EnsureInitializedAsync();
        var normalized = NormalizeType(type);
        if (normalized == "expense")
        {
            return await _db.Db.Table<Category>()
                .Where(c => c.Type == "expense" || c.Type == "支出" || c.Type == "Expense" || c.Type == "EXPENSE")
                .ToListAsync();
        }

        if (normalized == "income")
        {
            return await _db.Db.Table<Category>()
                .Where(c => c.Type == "income" || c.Type == "收入" || c.Type == "Income" || c.Type == "INCOME")
                .ToListAsync();
        }

        return await _db.Db.Table<Category>().Where(c => c.Type == normalized).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _db.Db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> AddAsync(Category category)
    {
        await EnsureInitializedAsync();
        category.Type = NormalizeType(category.Type);

        var existing = await _db.Db.Table<Category>()
            .Where(c => c.Name == category.Name && c.Type == category.Type).FirstOrDefaultAsync();
        if (existing is not null) return false;

        await _db.Db.InsertAsync(category);
        return true;
    }

    public async Task UpdateAsync(Category category)
    {
        await EnsureInitializedAsync();
        category.Type = NormalizeType(category.Type);
        await _db.Db.UpdateAsync(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await EnsureInitializedAsync();

        var hasTransactions = await _db.Db.Table<Transaction>()
            .Where(t => t.CategoryId == id).CountAsync() > 0;
        if (hasTransactions) return false;

        await _db.Db.DeleteAsync<Category>(id);
        return true;
    }
}
