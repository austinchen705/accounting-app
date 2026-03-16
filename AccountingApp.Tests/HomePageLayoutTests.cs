namespace AccountingApp.Tests;

public class HomePageLayoutTests
{
    [Fact]
    public void HomePage_has_month_navigation_controls()
    {
        var xaml = ReadHomeXaml();

        Assert.Contains("Command=\"{Binding PreviousMonthCommand}\"", xaml);
        Assert.Contains("Text=\"{Binding CurrentMonthLabel}\"", xaml);
        Assert.Contains("Command=\"{Binding NextMonthCommand}\"", xaml);
    }

    [Fact]
    public void HomePage_uses_selected_month_copy_instead_of_fixed_this_month()
    {
        var xaml = ReadHomeXaml();

        Assert.DoesNotContain("Text=\"本月總覽\"", xaml);
        Assert.Contains("Text=\"{Binding SummaryTitle}\"", xaml);
        Assert.Contains("Text=\"該月份尚無收支資料，點右下角新增第一筆。\"", xaml);
    }

    [Fact]
    public void HomePage_shows_original_currency_and_exchange_info_for_recent_transactions()
    {
        var xaml = ReadHomeXaml();

        Assert.Contains("Text=\"{Binding AmountDisplayText}\"", xaml);
        Assert.Contains("Text=\"{Binding ExchangeInfoText}\"", xaml);
        Assert.Contains("IsVisible=\"{Binding HasExchangeInfo}\"", xaml);
    }

    private static string ReadHomeXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/HomePage.xaml"));

        return File.ReadAllText(path);
    }
}
