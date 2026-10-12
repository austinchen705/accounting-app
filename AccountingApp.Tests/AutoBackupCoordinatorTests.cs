using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class AutoBackupCoordinatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RunIfDue_uploads_and_records_success_when_due()
    {
        var h = new Harness();

        await h.Sut.RunIfDueAsync();

        Assert.Equal(1, h.Target.UploadCalls);
        var status = h.Sut.GetStatus();
        Assert.Equal(Now, status.LastSuccess);
        Assert.Null(status.FailureMessage);
        Assert.Empty(h.Notifications);
    }

    [Fact]
    public async Task RunIfDue_skips_upload_when_backed_up_recently()
    {
        var h = new Harness();
        h.Sut.RecordSuccess();
        h.Now = Now.AddHours(1);

        await h.Sut.RunIfDueAsync();

        Assert.Equal(0, h.Target.UploadCalls);
    }

    [Fact]
    public async Task RunIfDue_uploads_again_after_24_hours()
    {
        var h = new Harness();
        await h.Sut.RunIfDueAsync();
        h.Now = Now.AddHours(24);

        await h.Sut.RunIfDueAsync();

        Assert.Equal(2, h.Target.UploadCalls);
        Assert.Equal(Now.AddHours(24), h.Sut.GetStatus().LastSuccess);
    }

    [Fact]
    public async Task RunIfDue_uploads_for_pre_expiry_even_when_daily_interval_has_not_elapsed()
    {
        var h = new Harness();
        h.Sut.RecordSuccess();
        h.Now = Now.AddHours(12);
        h.Expiration = Now.AddDays(3).AddHours(6);

        await h.Sut.RunIfDueAsync();

        Assert.Equal(1, h.Target.UploadCalls);
    }

    [Fact]
    public async Task RunIfDue_records_failure_and_notifies_when_authorization_is_invalid()
    {
        var h = new Harness { Target = { BlockingIssue = "Google Drive 授權已失效，請到設定頁重新授權。" } };

        await h.Sut.RunIfDueAsync();

        Assert.Equal(0, h.Target.UploadCalls);
        var status = h.Sut.GetStatus();
        Assert.Null(status.LastSuccess);
        Assert.Equal("Google Drive 授權已失效，請到設定頁重新授權。", status.FailureMessage);
        var notification = Assert.Single(h.Notifications);
        Assert.Contains("授權已失效", notification);
    }

    [Fact]
    public async Task RunIfDue_records_failure_and_notifies_when_upload_throws()
    {
        var h = new Harness { Target = { UploadException = new InvalidOperationException("Google Drive 授權已失效，請重新授權。") } };

        await h.Sut.RunIfDueAsync();

        Assert.Equal("Google Drive 授權已失效，請重新授權。", h.Sut.GetStatus().FailureMessage);
        Assert.Single(h.Notifications);
    }

    [Fact]
    public async Task RunIfDue_notifies_at_most_once_per_day()
    {
        var h = new Harness { Target = { BlockingIssue = "授權失效" } };

        await h.Sut.RunIfDueAsync();
        h.Now = Now.AddHours(3);
        await h.Sut.RunIfDueAsync();
        h.Now = Now.AddDays(1);
        await h.Sut.RunIfDueAsync();

        Assert.Equal(2, h.Notifications.Count);
    }

    [Fact]
    public async Task RunIfDue_clears_failure_after_later_success()
    {
        var h = new Harness { Target = { BlockingIssue = "授權失效" } };
        await h.Sut.RunIfDueAsync();
        h.Target.BlockingIssue = null;
        h.Now = Now.AddMinutes(5);

        await h.Sut.RunIfDueAsync();

        var status = h.Sut.GetStatus();
        Assert.Equal(Now.AddMinutes(5), status.LastSuccess);
        Assert.Null(status.FailureMessage);
    }

    [Fact]
    public async Task RunIfDue_does_not_throw_when_notifier_fails()
    {
        var h = new Harness { Target = { BlockingIssue = "授權失效" } };
        h.NotifierException = new InvalidOperationException("notify failed");

        await h.Sut.RunIfDueAsync();

        Assert.Equal("授權失效", h.Sut.GetStatus().FailureMessage);
    }

    [Fact]
    public async Task RunIfDue_runs_only_once_when_called_concurrently()
    {
        var h = new Harness();
        var gate = new TaskCompletionSource();
        h.Target.UploadGate = gate.Task;

        var first = h.Sut.RunIfDueAsync();
        var second = h.Sut.RunIfDueAsync();
        gate.SetResult();
        await Task.WhenAll(first, second);

        Assert.Equal(1, h.Target.UploadCalls);
    }

    [Fact]
    public void RecordSuccess_updates_status_and_clears_failure()
    {
        var h = new Harness();
        h.Prefs.Set("auto_backup_failure_message", "舊的失敗");

        h.Sut.RecordSuccess();

        var status = h.Sut.GetStatus();
        Assert.Equal(Now, status.LastSuccess);
        Assert.Null(status.FailureMessage);
    }

    [Fact]
    public void GetStatus_returns_empty_when_nothing_recorded()
    {
        var h = new Harness();

        var status = h.Sut.GetStatus();

        Assert.Null(status.LastSuccess);
        Assert.Null(status.FailureMessage);
    }

    private sealed class Harness
    {
        public FakePreferenceStore Prefs { get; } = new();
        public FakeAutoBackupTarget Target { get; } = new();
        public List<string> Notifications { get; } = [];
        public DateTimeOffset Now { get; set; } = AutoBackupCoordinatorTests.Now;
        public DateTimeOffset? Expiration { get; set; }
        public Exception? NotifierException { get; set; }
        public AutoBackupCoordinator Sut { get; }

        public Harness()
        {
            Sut = new AutoBackupCoordinator(
                Prefs,
                Target,
                () => Expiration,
                message =>
                {
                    if (NotifierException is not null) throw NotifierException;
                    Notifications.Add(message);
                    return Task.CompletedTask;
                },
                () => Now);
        }
    }

    private sealed class FakeAutoBackupTarget : IAutoBackupTarget
    {
        public string? BlockingIssue { get; set; }
        public Exception? UploadException { get; set; }
        public Task? UploadGate { get; set; }
        public int UploadCalls { get; private set; }

        public Task<string?> GetBlockingIssueAsync() => Task.FromResult(BlockingIssue);

        public async Task UploadAsync()
        {
            UploadCalls++;
            if (UploadGate is not null) await UploadGate;
            if (UploadException is not null) throw UploadException;
        }
    }
}
