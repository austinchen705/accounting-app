namespace AccountingApp.Services;

/// <summary>
/// Wraps iOS local notification dispatch.
/// </summary>
public class NotificationService
{
    public static async Task RequestPermissionAsync()
    {
#if IOS
        var center = UserNotifications.UNUserNotificationCenter.Current;
        await center.RequestAuthorizationAsync(
            UserNotifications.UNAuthorizationOptions.Alert |
            UserNotifications.UNAuthorizationOptions.Sound |
            UserNotifications.UNAuthorizationOptions.Badge);
#else
        await Task.CompletedTask;
#endif
    }

    public static async Task SendAsync(string title, string body)
    {
#if IOS
        var content = new UserNotifications.UNMutableNotificationContent();
        content.Title = title;
        content.Body = body;
        content.Sound = UserNotifications.UNNotificationSound.Default;

        var trigger = UserNotifications.UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
        var request = UserNotifications.UNNotificationRequest.FromIdentifier(
            Guid.NewGuid().ToString(), content, trigger);

        var center = UserNotifications.UNUserNotificationCenter.Current;
        await center.AddNotificationRequestAsync(request);
#else
        await Task.CompletedTask;
#endif
    }
}
