namespace AccountingApp.Tests;

public class IosEntryAccessoryLayoutTests
{
    [Fact]
    public void IosEntryAccessory_exposes_opt_in_property_and_toolbar_mapping()
    {
        var code = ReadIosEntryAccessoryCode();
        var mauiProgram = ReadMauiProgramCode();
        var resources = ReadAppResources();
        var zhResources = ReadAppResourcesZhHant();

        Assert.Contains("BindableProperty.CreateAttached", code);
        Assert.Contains("\"Next\"", code);
        Assert.Contains("EntryHandler.Mapper.AppendToMapping", code);
        Assert.Contains("InputAccessoryView", code);
        Assert.Contains("new UIToolbar()", code);
        Assert.Contains("new UIBarButtonItem(", code);
        Assert.Contains("\"Next\"", code);
        Assert.Contains("UIBarButtonSystemItem.FlexibleSpace", code);
        Assert.Contains("SendActionForControlEvents(UIControlEvent.EditingDidEndOnExit)", code);
        Assert.Contains("ResignFirstResponder()", code);
        Assert.Contains("LocalizationResourceManager.Instance.GetString(\"TransactionFormInputNextButton\")", code);
        Assert.Contains("IosEntryAccessory.Configure();", mauiProgram);
        Assert.Contains("TransactionFormInputNextButton", resources);
        Assert.Contains("TransactionFormInputNextButton", zhResources);
    }

    private static string ReadIosEntryAccessoryCode()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Platforms/iOS/IosEntryAccessory.cs"));

        return File.ReadAllText(path);
    }

    private static string ReadMauiProgramCode()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/MauiProgram.cs"));

        return File.ReadAllText(path);
    }

    private static string ReadAppResources()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Strings/AppResources.resx"));

        return File.ReadAllText(path);
    }

    private static string ReadAppResourcesZhHant()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Strings/AppResources.zh-Hant.resx"));

        return File.ReadAllText(path);
    }
}
