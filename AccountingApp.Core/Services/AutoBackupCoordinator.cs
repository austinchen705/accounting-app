using System.Globalization;
using AccountingApp.Core.Abstractions;

namespace AccountingApp.Core.Services;

public sealed record AutoBackupStatus(DateTimeOffset? LastSuccess, string? FailureMessage);

public class AutoBackupCoordinator
{
    private const string LastSuccessKey = "auto_backup_last_success_utc";
    private const string FailureMessageKey = "auto_backup_failure_message";
    private const string LastNotifiedKey = "auto_backup_last_notified_utc";

    private readonly IPreferenceStore _preferences;
    private readonly IAutoBackupTarget _target;
    private readonly Func<DateTimeOffset?> _getExpiration;
    private readonly Func<string, Task> _notifyFailureAsync;
    private readonly Func<DateTimeOffset> _clock;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AutoBackupCoordinator(
        IPreferenceStore preferences,
        IAutoBackupTarget target,
        Func<DateTimeOffset?> getExpiration,
        Func<string, Task> notifyFailureAsync,
        Func<DateTimeOffset>? clock = null)
    {
        _preferences = preferences;
        _target = target;
        _getExpiration = getExpiration;
        _notifyFailureAsync = notifyFailureAsync;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public AutoBackupStatus GetStatus()
    {
        var failure = _preferences.Get(FailureMessageKey, string.Empty);
        return new AutoBackupStatus(
            ReadTimestamp(LastSuccessKey),
            string.IsNullOrEmpty(failure) ? null : failure);
    }

    public void RecordSuccess()
    {
        _preferences.Set(LastSuccessKey, _clock().ToString("O", CultureInfo.InvariantCulture));
        _preferences.Set(FailureMessageKey, string.Empty);
    }

    public async Task RunIfDueAsync()
    {
        if (!await _gate.WaitAsync(0))
        {
            return;
        }

        try
        {
            var now = _clock();
            var reason = AutoBackupPolicy.Evaluate(now, ReadTimestamp(LastSuccessKey), _getExpiration());
            if (reason == AutoBackupReason.None)
            {
                return;
            }

            try
            {
                var issue = await _target.GetBlockingIssueAsync();
                if (issue is not null)
                {
                    throw new InvalidOperationException(issue);
                }

                await _target.UploadAsync();
                RecordSuccess();
            }
            catch (Exception ex)
            {
                await RecordFailureAsync(ex.Message, now);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task RecordFailureAsync(string message, DateTimeOffset now)
    {
        _preferences.Set(FailureMessageKey, message);

        var lastNotified = ReadTimestamp(LastNotifiedKey);
        if (lastNotified is not null && now - lastNotified < AutoBackupPolicy.DailyInterval)
        {
            return;
        }

        _preferences.Set(LastNotifiedKey, now.ToString("O", CultureInfo.InvariantCulture));
        try
        {
            await _notifyFailureAsync(message);
        }
        catch
        {
        }
    }

    private DateTimeOffset? ReadTimestamp(string key)
    {
        var raw = _preferences.Get(key, string.Empty);
        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value)
            ? value
            : null;
    }
}
