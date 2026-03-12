using System.Globalization;
using System.Text.Json;
using AccountingApp.Models;

namespace AccountingApp.Services;

public class JsonImportService
{
    private readonly DatabaseService _databaseService;
    private readonly CategoryService _categoryService;
    private readonly TransactionService _transactionService;

    public JsonImportService(DatabaseService databaseService, CategoryService categoryService, TransactionService transactionService)
    {
        _databaseService = databaseService;
        _categoryService = categoryService;
        _transactionService = transactionService;
    }

    public async Task<int> ImportTransactionsAsync(string jsonPath, bool replaceExisting = false)
    {
        await _databaseService.InitializeAsync();
        await _databaseService.ApplyImportPatchAsync();

        var json = await File.ReadAllTextAsync(jsonPath);
        return await ImportTransactionsFromJsonAsync(json, replaceExisting);
    }

    public async Task<int> ImportTransactionsFromJsonAsync(string json, bool replaceExisting = false)
    {
        await _databaseService.InitializeAsync();
        await _databaseService.ApplyImportPatchAsync();

        if (replaceExisting)
        {
            await _transactionService.DeleteAllAsync();
        }

        var payload = JsonSerializer.Deserialize<SeedPayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (payload?.Transactions is null || payload.Transactions.Count == 0)
            return 0;

        var existingCategories = await _categoryService.GetAllAsync();
        var inserted = 0;

        foreach (var item in payload.Transactions)
        {
            if (item.Amount <= 0) continue;

            var type = NormalizeType(item.Type);
            var categoryName = string.IsNullOrWhiteSpace(item.Category) ? "其他支出" : item.Category.Trim();

            var category = existingCategories.FirstOrDefault(c => c.Name == categoryName && c.Type == type);
            if (category is null)
            {
                await _categoryService.AddAsync(new Category { Name = categoryName, Type = type, Icon = "cat_other.png" });
                existingCategories = await _categoryService.GetAllAsync();
                category = existingCategories.FirstOrDefault(c => c.Name == categoryName && c.Type == type);

                // If same-name category exists with another type and AddAsync didn't create a new one,
                // fallback to a safe default category by type.
                if (category is null)
                {
                    var fallbackName = type == "income" ? "其他收入" : "其他支出";
                    category = existingCategories.FirstOrDefault(c => c.Name == fallbackName && c.Type == type);
                }

                // Final fallback: pick any category of the same type.
                category ??= existingCategories.FirstOrDefault(c => c.Type == type);
                if (category is null) continue;
            }

            var date = ParseDate(item.Date);
            var currency = string.IsNullOrWhiteSpace(item.Currency) ? "TWD" : item.Currency.Trim().ToUpperInvariant();

            var txn = new Transaction
            {
                Amount = item.Amount,
                Currency = currency,
                CategoryId = category.Id,
                Date = date,
                Note = item.Note ?? string.Empty,
                Type = type
            };

            await _transactionService.AddAsync(txn);
            inserted++;
        }

        return inserted;
    }

    private static DateTime ParseDate(string? date)
    {
        if (!string.IsNullOrWhiteSpace(date) &&
            DateTime.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateTime.Today;
    }

    private static string NormalizeType(string? type)
    {
        var value = (type ?? "expense").Trim().ToLowerInvariant();
        return value switch
        {
            "income" or "收入" => "income",
            _ => "expense"
        };
    }

    private class SeedPayload
    {
        public List<SeedTransaction> Transactions { get; set; } = [];
    }

    private class SeedTransaction
    {
        public string? Date { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Currency { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
