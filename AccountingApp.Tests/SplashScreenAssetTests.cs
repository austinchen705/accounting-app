namespace AccountingApp.Tests;

public class SplashScreenAssetTests
{
    [Fact]
    public void SplashScreen_uses_fullscreen_branding_copy_and_large_base_size()
    {
        var projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/AccountingApp.csproj"));
        var splashPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Splash/splash.svg"));

        var project = File.ReadAllText(projectPath);
        var splash = File.ReadAllText(splashPath);

        Assert.Contains("BaseSize=\"430,932\"", project);
        Assert.Contains("PERSONAL FINANCE", splash);
        Assert.Contains("記帳小助手", splash);
        Assert.Contains("Simple • Clean • Daily Money Tracker", splash);
        Assert.Contains("y=\"2550\"", splash);
        Assert.Contains("Avenir Next", splash);
        Assert.Contains("PingFang TC", splash);
    }
}
