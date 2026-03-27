using System.Collections.ObjectModel;
using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Services;

namespace AccountingApp.ViewModels;

public sealed class TransactionFormFrequentCategoryState<TCategory, TTransaction>
    where TCategory : IFrequentCategorySourceCategory
    where TTransaction : IFrequentCategorySourceTransaction
{
    public ObservableCollection<TCategory> Categories { get; } = new();
    public ObservableCollection<TCategory> FrequentCategories { get; } = new();

    public TCategory? SelectedCategory { get; set; }

    public void SelectFrequentCategory(TCategory? category)
    {
        SelectedCategory = category;
    }

    public void Apply(
        IEnumerable<TCategory> categories,
        IEnumerable<TTransaction> transactions,
        string type,
        int? preferredCategoryId = null)
    {
        var categoryList = categories.ToList();

        Categories.Clear();
        foreach (var category in categoryList)
        {
            Categories.Add(category);
        }

        FrequentCategories.Clear();
        foreach (var frequentCategory in FrequentCategoryRanking.BuildFrequentCategories(categoryList, transactions, type))
        {
            FrequentCategories.Add(frequentCategory);
        }

        SelectedCategory = FrequentCategoryRanking.ResolveSelectedCategory(
            categoryList,
            preferredCategoryId,
            SelectedCategory);
    }
}
