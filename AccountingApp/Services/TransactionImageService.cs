using CoreTransactionImageService = AccountingApp.Core.Services.TransactionImageService;

namespace AccountingApp.Services;

public class TransactionImageService
{
    private readonly CoreTransactionImageService _inner;

    public TransactionImageService()
    {
        _inner = new CoreTransactionImageService(FileSystem.AppDataDirectory);
    }

    public Task<string> ImportAsync(string sourcePath, CancellationToken cancellationToken = default) =>
        _inner.ImportAsync(sourcePath, cancellationToken);

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default) =>
        _inner.DeleteAsync(relativePath, cancellationToken);
}
