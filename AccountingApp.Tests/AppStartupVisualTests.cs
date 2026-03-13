namespace AccountingApp.Tests;

public class AppStartupVisualTests
{
    [Fact]
    public void App_sets_window_background_to_splash_color_to_avoid_black_gap()
    {
        var appPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/App.xaml.cs"));
        var appCode = File.ReadAllText(appPath);

        Assert.Contains("shell.BackgroundColor = Color.FromArgb(\"#10B981\")", appCode);
    }
}
