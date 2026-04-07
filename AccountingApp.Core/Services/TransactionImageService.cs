namespace AccountingApp.Core.Services;

public class TransactionImageService
{
    private readonly string _rootDirectory;
    private readonly Func<DateTime> _utcNow;
    private readonly Func<string, string> _fileNameFactory;

    public TransactionImageService(
        string rootDirectory,
        Func<DateTime>? utcNow = null,
        Func<string, string>? fileNameFactory = null)
    {
        _rootDirectory = rootDirectory;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
        _fileNameFactory = fileNameFactory ?? CreateFileName;
    }

    public async Task<string> ImportAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path is required.", nameof(sourcePath));
        }

        var now = _utcNow();
        var fileName = _fileNameFactory(sourcePath);
        var relativePath = Path.Combine("receipts", now.ToString("yyyy"), now.ToString("MM"), fileName)
            .Replace(Path.DirectorySeparatorChar, '/');
        var destinationPath = GetAbsolutePath(relativePath);
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        await using var sourceStream = File.OpenRead(sourcePath);
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        return relativePath;
    }

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string relativePath) =>
        Path.Combine(_rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string CreateFileName(string sourcePath)
    {
        var extension = Path.GetExtension(sourcePath);
        return $"{Guid.NewGuid():N}{extension}";
    }
}
