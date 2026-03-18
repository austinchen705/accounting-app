namespace AccountingApp.Tests;

public class LocalizationResourceContractTests
{
    [Fact]
    public void Localization_resources_exist_with_core_shell_keys()
    {
        var englishPath = GetProjectPath("AccountingApp/Resources/Strings/AppResources.resx");
        var zhHantPath = GetProjectPath("AccountingApp/Resources/Strings/AppResources.zh-Hant.resx");

        Assert.True(File.Exists(englishPath));
        Assert.True(File.Exists(zhHantPath));

        var englishResx = File.ReadAllText(englishPath);
        var zhHantResx = File.ReadAllText(zhHantPath);

        Assert.Contains("HomeTabTitle", englishResx);
        Assert.Contains("HomeTabTitle", zhHantResx);
        Assert.Contains("SettingsTabTitle", englishResx);
        Assert.Contains("SettingsTabTitle", zhHantResx);
    }

    private static string GetProjectPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../", relativePath));
    }
}
