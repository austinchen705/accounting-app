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
        Assert.Contains("SettingsPageTitle", xaml);
        Assert.Contains("SettingsCurrencySectionTitle", xaml);
        Assert.Contains("SetTraditionalChineseCommand", xaml);
        Assert.Contains("SetEnglishCommand", xaml);
    }

    [Fact]
    public void SettingsPage_includes_app_info_section_with_version_and_expiration_bindings()
    {
        var xaml = ReadSettingsXaml();

        Assert.Contains("SettingsAppInfoSectionTitle", xaml);
        Assert.Contains("SettingsAppVersionLabel", xaml);
        Assert.Contains("SettingsAppExpirationLabel", xaml);
        Assert.Contains("Text=\"{Binding AppVersionText}\"", xaml);
        Assert.Contains("Text=\"{Binding AppExpirationDateText}\"", xaml);
    }

    [Fact]
    public void SettingsPage_shows_auto_backup_status_with_failure_highlight()
    {
        var xaml = ReadSettingsXaml();

        Assert.Contains("Text=\"{Binding AutoBackupStatusText}\"", xaml);
        Assert.Contains("Binding=\"{Binding HasAutoBackupFailure}\"", xaml);
    }

    [Theory]
    [InlineData("AppResources.resx")]
    [InlineData("AppResources.zh-Hant.resx")]
    public void AppResources_define_auto_backup_strings(string resourceFile)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Strings",
            resourceFile));
        var resources = File.ReadAllText(path);

        Assert.Contains("name=\"SettingsAutoBackupNever\"", resources);
        Assert.Contains("name=\"SettingsAutoBackupLastSuccess\"", resources);
        Assert.Contains("name=\"SettingsAutoBackupFailed\"", resources);
        Assert.Contains("name=\"AutoBackupFailureNotificationTitle\"", resources);
    }

    private static string ReadSettingsXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/SettingsPage.xaml"));

        return File.ReadAllText(path);
    }
}
