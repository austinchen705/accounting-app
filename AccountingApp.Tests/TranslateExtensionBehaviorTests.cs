namespace AccountingApp.Tests;

public class TranslateExtensionBehaviorTests
{
    [Fact]
    public void TranslateExtension_uses_binding_to_localization_resource_manager()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Markup/TranslateExtension.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("IMarkupExtension<BindingBase>", code);
        Assert.Contains("new Binding", code);
        Assert.Contains("LocalizationResourceManager.Instance", code);
    }

    [Fact]
    public void LocalizationService_refreshes_localization_resource_manager_on_language_change()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Services/LocalizationService.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("LocalizationResourceManager.Instance.Refresh()", code);
    }
}
