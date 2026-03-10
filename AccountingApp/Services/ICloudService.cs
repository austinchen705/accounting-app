using AccountingApp.Services;

namespace AccountingApp.Services;

public class ICloudService
{
    private readonly DatabaseService _databaseService;
    private const string BackupFileName = "accounting_backup.db";
    private bool _pendingSync;

    public ICloudService(DatabaseService databaseService) => _databaseService = databaseService;

    private string? GetICloudDocumentsPath()
    {
#if IOS
        var url = Foundation.NSFileManager.DefaultManager
            .GetUrl(Foundation.NSSearchPathDirectory.DocumentDirectory,
                    Foundation.NSSearchPathDomain.User,
                    null, true, out var error);
        if (error is not null) return null;

        // Use iCloud Documents container
        var icloudUrl = Foundation.NSFileManager.DefaultManager
            .GetUrlForUbiquityContainer(null);
        if (icloudUrl is null) return null;

        return Path.Combine(icloudUrl.Path!, "Documents");
#else
        return null;
#endif
    }

    public async Task SyncOnStartupAsync()
    {
        var icloudDir = GetICloudDocumentsPath();
        if (icloudDir is null)
        {
            // iCloud not available
            return;
        }

        var icloudPath = Path.Combine(icloudDir, BackupFileName);
        var localPath = _databaseService.DatabasePath;

        if (!File.Exists(icloudPath)) return;

        var icloudModified = File.GetLastWriteTimeUtc(icloudPath);
        var localModified = File.Exists(localPath) ? File.GetLastWriteTimeUtc(localPath) : DateTime.MinValue;

        if (icloudModified > localModified)
        {
            File.Copy(icloudPath, localPath, overwrite: true);
            // Re-initialize database connection after overwrite
            await _databaseService.InitializeAsync();
        }

        await Task.CompletedTask;
    }

    public async Task UploadAsync()
    {
        var icloudDir = GetICloudDocumentsPath();
        if (icloudDir is null)
        {
            _pendingSync = true;
            return;
        }

        Directory.CreateDirectory(icloudDir);
        var dest = Path.Combine(icloudDir, BackupFileName);
        File.Copy(_databaseService.DatabasePath, dest, overwrite: true);
        _pendingSync = false;
        await Task.CompletedTask;
    }

    public async Task RestoreAsync()
    {
        var icloudDir = GetICloudDocumentsPath();
        if (icloudDir is null)
            throw new InvalidOperationException("iCloud 不可用");

        var icloudPath = Path.Combine(icloudDir, BackupFileName);
        if (!File.Exists(icloudPath))
            throw new FileNotFoundException("iCloud 上無備份檔案");

        File.Copy(icloudPath, _databaseService.DatabasePath, overwrite: true);
        await _databaseService.InitializeAsync();
    }

    public bool IsPendingSync => _pendingSync;

    public async Task FlushPendingSyncAsync()
    {
        if (_pendingSync)
            await UploadAsync();
    }
}
