using AccountingApp.Core.Abstractions;

namespace AccountingApp.Core.Services;

public static class FrequentCategoryRanking
{
    public static IReadOnlyList<TCategory> BuildFrequentCategories<TCategory, TTransaction>(
        IEnumerable<TCategory> categories,
        IEnumerable<TTransaction> transactions,
        string type,
        int limit = 6)
        where TCategory : IFrequentCategorySourceCategory
        where TTransaction : IFrequentCategorySourceTransaction
    {
        var normalizedType = NormalizeType(type);
        var usageCounts = transactions
            .Where(transaction => NormalizeType(transaction.Type) == normalizedType)
            .GroupBy(transaction => transaction.CategoryId)
            .ToDictionary(group => group.Key, group => group.Count());

        return categories
            .Where(category => NormalizeType(category.Type) == normalizedType)
            .Select(category => new
            {
                Category = category,
                UsageCount = usageCounts.TryGetValue(category.Id, out var count) ? count : 0
            })
            .Where(item => item.UsageCount > 0)
            .OrderByDescending(item => item.UsageCount)
            .ThenBy(item => item.Category.Name, StringComparer.Ordinal)
            .Take(limit)
            .Select(item => item.Category)
            .ToList();
    }

    public static TCategory? ResolveSelectedCategory<TCategory>(
        IReadOnlyList<TCategory> categories,
        int? preferredCategoryId,
        TCategory? currentSelection)
        where TCategory : IFrequentCategorySourceCategory
    {
        if (preferredCategoryId.HasValue)
        {
            return categories.FirstOrDefault(category => category.Id == preferredCategoryId.Value);
        }

        if (currentSelection is not null &&
            categories.Any(category => category.Id == currentSelection.Id))
        {
            return categories.FirstOrDefault(category => category.Id == currentSelection.Id);
        }

        return categories.FirstOrDefault();
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
