namespace AccountingApp.Tests;

public class AppShellLocalizationTests
{
    [Fact]
    public void AppShell_uses_translation_markup_for_tab_titles()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/AppShell.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.DoesNotContain("Title=\"首頁\"", xaml);
        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("HomeTabTitle", xaml);
        Assert.Contains("SettingsTabTitle", xaml);
    }
}
