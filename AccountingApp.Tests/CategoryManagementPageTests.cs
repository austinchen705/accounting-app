namespace AccountingApp.Tests;

public class CategoryManagementPageTests
{
    [Fact]
    public void AppShell_includes_category_management_tab()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/AppShell.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("CategoryListPage", xaml);
        Assert.Contains("CategoryManagementTabTitle", xaml);
    }

    [Fact]
    public void CategoryListPage_includes_add_edit_delete_actions()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryListPage.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("FormTitleText", xaml);
        Assert.Contains("CategoryNameLabel", xaml);
        Assert.Contains("EditButtonText", xaml);
        Assert.Contains("DeleteButtonText", xaml);
        Assert.Contains("HasCategories", xaml);
        Assert.Contains("SubmitButtonText", xaml);
        Assert.Contains("CancelEditCommand", xaml);
    }

    [Fact]
    public void CategoryFormPage_uses_dynamic_title_and_submit_text()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryFormPage.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("Title=\"{Binding FormTitle}\"", xaml);
        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("CategoryNameLabel", xaml);
        Assert.Contains("Text=\"{Binding SubmitButtonText}\"", xaml);
    }

    [Fact]
    public void CategoryFormViewModel_exposes_dynamic_form_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryFormViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("CategoryFormEditTitle", code);
        Assert.Contains("CategoryFormCreateTitle", code);
        Assert.Contains("CategoryFormUpdateButton", code);
        Assert.Contains("CategoryFormCreateButton", code);
    }
}
