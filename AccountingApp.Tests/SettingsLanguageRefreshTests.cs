namespace AccountingApp.Tests;

public class SettingsLanguageRefreshTests
{
    [Fact]
    public void SettingsViewModel_delegates_language_refresh_to_app_shell_reset()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/SettingsViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("ResetShellForLanguageChangeAsync", code);
        Assert.DoesNotContain("Application.Current.Windows[0].Page = new AppShell();", code);
        Assert.DoesNotContain("Shell.Current.GoToAsync(\"//HomePage\")", code);
    }

    [Fact]
    public void App_exposes_language_shell_reset_helper()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/App.xaml.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("ResetShellForLanguageChangeAsync", code);
        Assert.Contains("new AppShell(_localizationService)", code);
        Assert.Contains("window.Page = shell", code);
        Assert.Contains("shell.GoToAsync(\"//HomePage\")", code);
        Assert.DoesNotContain("shell.GoToAsync(\"//SettingsPage\")", code);
    }
}
