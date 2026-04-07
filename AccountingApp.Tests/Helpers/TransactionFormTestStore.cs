namespace AccountingApp.Tests.Helpers;

public sealed class TransactionFormTestStore
{
    public List<AccountingApp.Models.Category> Categories { get; } = new();
    public List<AccountingApp.Models.Transaction> Transactions { get; } = new();
    public List<string> DeletedImagePaths { get; } = new();
    public Exception? UpdateException { get; set; }
}
