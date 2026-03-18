namespace AccountingApp.Tests;

public class SettingsPageLayoutTests
{
    [Fact]
    public void SettingsPage_includes_localized_language_selector()
    {
        var xaml = ReadSettingsXaml();

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("LanguageLabel", xaml);
        Assert.Contains("LanguageTraditionalChinese", xaml);
        Assert.Contains("LanguageEnglish", xaml);
        Assert.Contains("SetTraditionalChineseCommand", xaml);
        Assert.Contains("SetEnglishCommand", xaml);
    }

    private static string ReadSettingsXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/SettingsPage.xaml"));

        return File.ReadAllText(path);
    }
}
