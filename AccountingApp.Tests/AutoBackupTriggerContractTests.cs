namespace AccountingApp.Tests;

public class AutoBackupTriggerContractTests
{
    [Fact]
    public void App_runs_auto_backup_when_window_is_activated_after_startup()
    {
        var source = ReadAppSource();

        Assert.Contains("window.Activated +=", source);
        Assert.Contains("_autoBackup.RunIfDueAsync()", source);
        Assert.Contains("_startupCompleted", source);
    }

    private static string ReadAppSource()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/App.xaml.cs"));

        return File.ReadAllText(path);
    }
}
