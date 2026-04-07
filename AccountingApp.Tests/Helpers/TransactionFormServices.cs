using AccountingApp.Tests.Helpers;

namespace AccountingApp.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    void Initialize();
    Task SetLanguageAsync(string languageCode);
    string GetString(string key);
}

public sealed class TransactionService
{
    private readonly TransactionFormTestStore _store;

    public TransactionService(TransactionFormTestStore store)
    {
        _store = store;
    }

    public Task<List<AccountingApp.Models.Transaction>> GetAllAsync() =>
        Task.FromResult(_store.Transactions.OrderByDescending(transaction => transaction.Date).ToList());

    public Task AddAsync(AccountingApp.Models.Transaction transaction)
    {
        if (transaction.Id == 0)
        {
            transaction.Id = _store.Transactions.Count == 0 ? 1 : _store.Transactions.Max(item => item.Id) + 1;
        }

        _store.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AccountingApp.Models.Transaction transaction)
    {
        if (_store.UpdateException is not null)
        {
            throw _store.UpdateException;
        }

        var index = _store.Transactions.FindIndex(item => item.Id == transaction.Id);
        if (index >= 0)
        {
            _store.Transactions[index] = transaction;
        }
        else
        {
            _store.Transactions.Add(transaction);
        }

        return Task.CompletedTask;
    }
}

public sealed class TransactionImageService
{
    private readonly TransactionFormTestStore _store;

    public TransactionImageService(TransactionFormTestStore store)
    {
        _store = store;
    }

    public Task DeleteAsync(string? relativePath)
    {
        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            _store.DeletedImagePaths.Add(relativePath);
        }

        return Task.CompletedTask;
    }
}

public sealed class CategoryService
{
    private readonly TransactionFormTestStore _store;

    public CategoryService(TransactionFormTestStore store)
    {
        _store = store;
    }

    public Task<List<AccountingApp.Models.Category>> GetByTypeAsync(string type)
    {
        var normalized = NormalizeType(type);
        var categories = _store.Categories
            .Where(category => NormalizeType(category.Type) == normalized)
            .ToList();

        return Task.FromResult(categories);
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
}

public sealed class BudgetService
{
    public BudgetService(TransactionFormTestStore store)
    {
    }

    public Task CheckAndNotifyAsync(int categoryId, string month) => Task.CompletedTask;
}

public sealed class TestLocalizationService : ILocalizationService
{
    public string CurrentLanguage { get; private set; } = "zh-Hant";

    public void Initialize()
    {
    }

    public Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguage = languageCode;
        return Task.CompletedTask;
    }

    public string GetString(string key) => key;
}
