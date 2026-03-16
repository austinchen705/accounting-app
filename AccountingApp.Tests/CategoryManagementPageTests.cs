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

        Assert.Contains("Title=\"分類管理\"", xaml);
        Assert.Contains("CategoryListPage", xaml);
    }

    [Fact]
    public void CategoryListPage_includes_add_edit_delete_actions()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryListPage.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("FormTitleText", xaml);
        Assert.Contains("Text=\"編輯\"", xaml);
        Assert.Contains("Text=\"刪除\"", xaml);
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
        Assert.Contains("Text=\"{Binding SubmitButtonText}\"", xaml);
    }

    [Fact]
    public void CategoryFormViewModel_exposes_dynamic_form_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryFormViewModel.cs"));
        var code = File.ReadAllText(path);

        Assert.Contains("public string FormTitle => _isEdit ? \"編輯分類\" : \"新增分類\";", code);
        Assert.Contains("public string SubmitButtonText => _isEdit ? \"更新分類\" : \"新增分類\";", code);
    }
}
