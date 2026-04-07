using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class TransactionImageServiceTests : IDisposable
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), $"txn-images-{Guid.NewGuid():N}");

    [Fact]
    public async Task ImportAsync_copies_file_into_app_storage_and_DeleteAsync_removes_it()
    {
        Directory.CreateDirectory(_rootPath);
        var service = new TransactionImageService(
            _rootPath,
            () => new DateTime(2026, 4, 7, 10, 30, 0, DateTimeKind.Utc),
            _ => "example.jpg");
        var sourcePath = Path.Combine(_rootPath, "source.jpg");
        await File.WriteAllTextAsync(sourcePath, "image-bytes");

        var relativePath = await service.ImportAsync(sourcePath);
        var storedPath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.Equal("receipts/2026/04/example.jpg", relativePath);
        Assert.True(File.Exists(storedPath));
        Assert.Equal("image-bytes", await File.ReadAllTextAsync(storedPath));

        await service.DeleteAsync(relativePath);

        Assert.False(File.Exists(storedPath));
    }

    [Fact]
    public async Task ImportAsync_from_stream_copies_file_into_app_storage()
    {
        Directory.CreateDirectory(_rootPath);
        var service = new TransactionImageService(
            _rootPath,
            () => new DateTime(2026, 4, 7, 10, 30, 0, DateTimeKind.Utc),
            _ => "picked.png");
        await using var sourceStream = new MemoryStream("picked-image"u8.ToArray());

        var relativePath = await service.ImportAsync(sourceStream, "picked.png");
        var storedPath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.Equal("receipts/2026/04/picked.png", relativePath);
        Assert.True(File.Exists(storedPath));
        Assert.Equal("picked-image", await File.ReadAllTextAsync(storedPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }
}
