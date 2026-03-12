using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class GoogleDriveBackupCoordinatorTests
{
    [Fact]
    public async Task Backup_creates_file_when_missing()
    {
        var api = new FakeGoogleDriveFileApi { ExistingFileId = null };
        var sut = new GoogleDriveBackupCoordinator(api);

        var mode = await sut.BackupAsync("folder-1", OpenLocal);

        Assert.Equal(BackupWriteMode.Created, mode);
        Assert.Equal(1, api.CreateCalls);
        Assert.Equal(0, api.UpdateCalls);
        Assert.Equal(GoogleDriveBackupCoordinator.BackupFileName, api.LastCreateFileName);
    }

    [Fact]
    public async Task Backup_updates_file_when_exists()
    {
        var api = new FakeGoogleDriveFileApi { ExistingFileId = "file-123" };
        var sut = new GoogleDriveBackupCoordinator(api);

        var mode = await sut.BackupAsync("folder-1", OpenLocal);

        Assert.Equal(BackupWriteMode.Updated, mode);
        Assert.Equal(0, api.CreateCalls);
        Assert.Equal(1, api.UpdateCalls);
        Assert.Equal("file-123", api.LastUpdateFileId);
    }

    [Fact]
    public async Task Restore_throws_when_folder_missing()
    {
        var api = new FakeGoogleDriveFileApi();
        var sut = new GoogleDriveBackupCoordinator(api);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RestoreAsync(null, _ => Task.CompletedTask));

        Assert.Contains("資料夾", ex.Message);
    }

    [Fact]
    public async Task Restore_throws_when_backup_file_missing()
    {
        var api = new FakeGoogleDriveFileApi { ExistingFileId = null };
        var sut = new GoogleDriveBackupCoordinator(api);

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            sut.RestoreAsync("folder-1", _ => Task.CompletedTask));

        Assert.Contains("accounting_backup.db", ex.Message);
    }

    [Fact]
    public async Task Restore_throws_when_folder_is_invalid_on_lookup()
    {
        var api = new FakeGoogleDriveFileApi
        {
            LookupException = new InvalidOperationException("folderId 無效")
        };
        var sut = new GoogleDriveBackupCoordinator(api);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RestoreAsync("invalid-folder", _ => Task.CompletedTask));

        Assert.Contains("folderId", ex.Message);
    }

    [Fact]
    public async Task Restore_downloads_and_writes_local_data()
    {
        var api = new FakeGoogleDriveFileApi
        {
            ExistingFileId = "file-123",
            DownloadBytes = [1, 2, 3]
        };
        var sut = new GoogleDriveBackupCoordinator(api);
        byte[]? received = null;

        await sut.RestoreAsync("folder-1", async stream =>
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            received = ms.ToArray();
        });

        Assert.Equal([1, 2, 3], received);
        Assert.Equal("file-123", api.LastDownloadFileId);
    }

    private static Stream OpenLocal() => new MemoryStream([9, 9, 9]);

    private sealed class FakeGoogleDriveFileApi : IGoogleDriveFileApi
    {
        public string? ExistingFileId { get; set; }
        public int CreateCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public string? LastCreateFileName { get; private set; }
        public string? LastUpdateFileId { get; private set; }
        public string? LastDownloadFileId { get; private set; }
        public byte[] DownloadBytes { get; set; } = [];
        public Exception? LookupException { get; set; }

        public Task<string?> FindFileIdAsync(string folderId, string fileName, CancellationToken cancellationToken = default)
            => LookupException is null
                ? Task.FromResult(ExistingFileId)
                : Task.FromException<string?>(LookupException);

        public async Task CreateFileAsync(string folderId, string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            LastCreateFileName = fileName;
            CreateCalls++;
            await DrainAsync(content, cancellationToken);
        }

        public async Task UpdateFileAsync(string fileId, Stream content, CancellationToken cancellationToken = default)
        {
            LastUpdateFileId = fileId;
            UpdateCalls++;
            await DrainAsync(content, cancellationToken);
        }

        public Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            LastDownloadFileId = fileId;
            return Task.FromResult<Stream>(new MemoryStream(DownloadBytes));
        }

        private static async Task DrainAsync(Stream content, CancellationToken cancellationToken)
        {
            using var sink = new MemoryStream();
            await content.CopyToAsync(sink, cancellationToken);
        }
    }
}
