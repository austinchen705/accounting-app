namespace AccountingApp.Tests;

public class HomePageLayoutTests
{
    [Fact]
    public void HomePage_has_month_navigation_controls()
    {
        var xaml = ReadHomeXaml();

        Assert.Contains("Command=\"{Binding SetRangeCommand}\"", xaml);
        Assert.Contains("CommandParameter=\"Day\"", xaml);
        Assert.Contains("CommandParameter=\"Week\"", xaml);
        Assert.Contains("CommandParameter=\"Month\"", xaml);
        Assert.Contains("CommandParameter=\"Year\"", xaml);
        Assert.Contains("CommandParameter=\"All\"", xaml);
        Assert.Contains("Command=\"{Binding PreviousMonthCommand}\"", xaml);
        Assert.Contains("Text=\"{Binding PeriodLabel}\"", xaml);
        Assert.Contains("Command=\"{Binding NextMonthCommand}\"", xaml);
    }

    [Fact]
    public void HomePage_uses_selected_period_copy_instead_of_fixed_month_copy()
    {
        var xaml = ReadHomeXaml();

        Assert.DoesNotContain("Text=\"本月總覽\"", xaml);
        Assert.Contains("Text=\"{Binding SummaryTitle}\"", xaml);
        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("HomeEmptyStateText", xaml);
        Assert.Contains("HomeRecentTransactionsTitle", xaml);
    }

    [Fact]
    public void HomePage_shows_original_currency_and_exchange_info_for_recent_transactions()
    {
        var xaml = ReadHomeXaml();

        Assert.Contains("Text=\"{Binding AmountDisplayText}\"", xaml);
        Assert.Contains("Text=\"{Binding ExchangeInfoText}\"", xaml);
        Assert.Contains("IsVisible=\"{Binding HasExchangeInfo}\"", xaml);
        Assert.Contains("HomeIncomeLabel", xaml);
        Assert.Contains("HomeExpenseLabel", xaml);
        Assert.Contains("HomeBalanceLabel", xaml);
    }

    private static string ReadHomeXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/HomePage.xaml"));

        return File.ReadAllText(path);
    }
}
