namespace AccountingApp.Core.Abstractions;

public interface IFrequentCategorySourceTransaction
{
    int CategoryId { get; }
    string Type { get; }
}
