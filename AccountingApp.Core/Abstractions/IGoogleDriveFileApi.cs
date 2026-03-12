namespace AccountingApp.Core.Abstractions;

public interface IGoogleDriveFileApi
{
    Task<string?> FindFileIdAsync(string folderId, string fileName, CancellationToken cancellationToken = default);
    Task CreateFileAsync(string folderId, string fileName, Stream content, CancellationToken cancellationToken = default);
    Task UpdateFileAsync(string fileId, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);
}
