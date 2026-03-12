using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public class CategoryService
{
    private readonly DatabaseService _db;

    public CategoryService(DatabaseService db) => _db = db;

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

    public async Task<List<Category>> GetAllAsync() =>
        await _db.Db.Table<Category>().ToListAsync();

    public async Task<List<Category>> GetByTypeAsync(string type)
    {
        var normalized = NormalizeType(type);
        return await _db.Db.Table<Category>().Where(c => c.Type == normalized).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id) =>
        await _db.Db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();

    /// <returns>true if added; false if same (name,type) already exists.</returns>
    public async Task<bool> AddAsync(Category category)
    {
        category.Type = NormalizeType(category.Type);
        var existing = await _db.Db.Table<Category>()
            .Where(c => c.Name == category.Name && c.Type == category.Type).FirstOrDefaultAsync();
        if (existing is not null) return false;
        await _db.Db.InsertAsync(category);
        return true;
    }

    public async Task UpdateAsync(Category category)
    {
        category.Type = NormalizeType(category.Type);
        await _db.Db.UpdateAsync(category);
        await _db.Db.ExecuteAsync(
            "UPDATE Transactions SET Type = ? WHERE CategoryId = ?",
            category.Type,
            category.Id);
    }

    /// <returns>true if deleted; false if category has transactions.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var hasTransactions = await _db.Db.Table<Transaction>()
            .Where(t => t.CategoryId == id).CountAsync() > 0;
        if (hasTransactions) return false;
        await _db.Db.DeleteAsync<Category>(id);
        return true;
    }
}
