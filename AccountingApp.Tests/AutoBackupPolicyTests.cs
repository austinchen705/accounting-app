using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AutoBackupPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Evaluate_returns_daily_when_never_backed_up()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, lastSuccess: null, expiration: null);

        Assert.Equal(AutoBackupReason.Daily, reason);
    }

    [Fact]
    public void Evaluate_returns_none_when_backed_up_less_than_24_hours_ago()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, Now.AddHours(-23), expiration: null);

        Assert.Equal(AutoBackupReason.None, reason);
    }

    [Fact]
    public void Evaluate_returns_daily_when_backed_up_24_hours_ago()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, Now.AddHours(-24), expiration: null);

        Assert.Equal(AutoBackupReason.Daily, reason);
    }

    [Fact]
    public void Evaluate_returns_pre_expiry_when_inside_window_and_no_backup_since_window_start()
    {
        var expiration = Now.AddDays(2);
        var lastSuccess = Now.AddHours(-1).AddDays(-2);

        var reason = AutoBackupPolicy.Evaluate(Now, lastSuccess, expiration);

        Assert.Equal(AutoBackupReason.PreExpiry, reason);
    }

    [Fact]
    public void Evaluate_returns_pre_expiry_when_inside_window_and_never_backed_up()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, lastSuccess: null, expiration: Now.AddDays(1));

        Assert.Equal(AutoBackupReason.PreExpiry, reason);
    }

    [Fact]
    public void Evaluate_returns_none_when_inside_window_and_already_backed_up_since_window_start()
    {
        var expiration = Now.AddDays(2);
        var lastSuccess = Now.AddHours(-1);

        var reason = AutoBackupPolicy.Evaluate(Now, lastSuccess, expiration);

        Assert.Equal(AutoBackupReason.None, reason);
    }

    [Fact]
    public void Evaluate_ignores_expiration_outside_window()
    {
        var expiration = Now.AddDays(AutoBackupPolicy.PreExpiryDays).AddHours(1);

        var reason = AutoBackupPolicy.Evaluate(Now, Now.AddHours(-1), expiration);

        Assert.Equal(AutoBackupReason.None, reason);
    }

    [Fact]
    public void Evaluate_returns_daily_when_expiration_unknown_and_last_backup_is_old()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, Now.AddDays(-5), expiration: null);

        Assert.Equal(AutoBackupReason.Daily, reason);
    }

    [Fact]
    public void Evaluate_falls_back_to_daily_when_already_expired()
    {
        var reason = AutoBackupPolicy.Evaluate(Now, Now.AddDays(-5), Now.AddHours(-1));

        Assert.Equal(AutoBackupReason.Daily, reason);
    }
}
