using AccountingApp.Core.Abstractions;

namespace AccountingApp.Core.Services;

public enum BackupWriteMode
{
    Created,
    Updated
}

public class GoogleDriveBackupCoordinator
{
    public const string BackupFileName = "accounting_backup.db";

    private readonly IGoogleDriveFileApi _driveApi;

    public GoogleDriveBackupCoordinator(IGoogleDriveFileApi driveApi)
    {
        _driveApi = driveApi;
    }

    public async Task<BackupWriteMode> BackupAsync(
        string? folderId,
        Func<Stream> localDbStreamFactory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folderId))
            throw new InvalidOperationException("尚未設定 Google Drive 備份資料夾。");

        var fileId = await _driveApi.FindFileIdAsync(folderId, BackupFileName, cancellationToken);
        await using var stream = localDbStreamFactory();

        if (string.IsNullOrWhiteSpace(fileId))
        {
            await _driveApi.CreateFileAsync(folderId, BackupFileName, stream, cancellationToken);
            return BackupWriteMode.Created;
        }

        await _driveApi.UpdateFileAsync(fileId, stream, cancellationToken);
        return BackupWriteMode.Updated;
    }

    public async Task RestoreAsync(
        string? folderId,
        Func<Stream, Task> writeLocalDbAsync,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folderId))
            throw new InvalidOperationException("尚未設定 Google Drive 備份資料夾。");

        var fileId = await _driveApi.FindFileIdAsync(folderId, BackupFileName, cancellationToken);
        if (string.IsNullOrWhiteSpace(fileId))
            throw new FileNotFoundException("找不到 Google Drive 備份檔 accounting_backup.db。");

        await using var stream = await _driveApi.DownloadFileAsync(fileId, cancellationToken);
        await writeLocalDbAsync(stream);
    }
}
