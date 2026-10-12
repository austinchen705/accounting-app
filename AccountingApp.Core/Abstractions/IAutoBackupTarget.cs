namespace AccountingApp.Core.Abstractions;

public interface IAutoBackupTarget
{
    /// <summary>回傳 null 表示可以備份；否則回傳無法備份的原因（不得觸發互動式登入）。</summary>
    Task<string?> GetBlockingIssueAsync();

    Task UploadAsync();
}
