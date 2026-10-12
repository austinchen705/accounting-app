namespace AccountingApp.Core.Services;

public enum AutoBackupReason
{
    None,
    Daily,
    PreExpiry
}

public static class AutoBackupPolicy
{
    public const int PreExpiryDays = 3;

    public static readonly TimeSpan DailyInterval = TimeSpan.FromHours(24);

    public static AutoBackupReason Evaluate(
        DateTimeOffset now,
        DateTimeOffset? lastSuccess,
        DateTimeOffset? expiration)
    {
        if (expiration is { } expirationDate)
        {
            var windowStart = expirationDate.AddDays(-PreExpiryDays);
            if (now >= windowStart &&
                now < expirationDate &&
                (lastSuccess is null || lastSuccess < windowStart))
            {
                return AutoBackupReason.PreExpiry;
            }
        }

        if (lastSuccess is null || now - lastSuccess >= DailyInterval)
        {
            return AutoBackupReason.Daily;
        }

        return AutoBackupReason.None;
    }
}
