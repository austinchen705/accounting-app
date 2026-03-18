namespace AccountingApp.Tests;

public class BudgetPageLayoutTests
{
    [Fact]
    public void BudgetPage_uses_localized_empty_state_and_action_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/BudgetPage.xaml"));
        var xaml = File.ReadAllText(path);

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("BudgetPageTitle", xaml);
        Assert.Contains("BudgetEmptyStateText", xaml);
        Assert.Contains("BudgetSetButton", xaml);
    }
}
