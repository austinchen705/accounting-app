namespace AccountingApp.Tests;

public class TransactionListLayoutTests
{
    [Fact]
    public void TransactionList_uses_calendar_surface_instead_of_native_datepicker_for_filter()
    {
        var xaml = ReadTransactionListXaml();

        Assert.DoesNotContain("<DatePicker", xaml);
        Assert.Contains("<controls:CalendarDatePicker", xaml);
        Assert.Contains("Date=\"{Binding FilterDate}\"", xaml);
        Assert.Contains("HorizontalOptions=\"End\"", xaml);
        Assert.DoesNotContain("<Grid ColumnDefinitions=\"*,Auto\" ColumnSpacing=\"10\">", xaml);
    }

    [Fact]
    public void TransactionList_shows_daily_income_expense_and_balance_summary()
    {
        var xaml = ReadTransactionListXaml();

        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("TransactionListDailySummaryTitle", xaml);
        Assert.Contains("SummaryCurrencyText", xaml);
        Assert.Contains("Text=\"{Binding DailyIncomeText}\"", xaml);
        Assert.Contains("Text=\"{Binding DailyExpenseText}\"", xaml);
        Assert.Contains("Text=\"{Binding DailyBalanceText}\"", xaml);
    }

    [Fact]
    public void TransactionList_shows_amount_with_original_currency_per_row()
    {
        var xaml = ReadTransactionListXaml();

        Assert.Contains("Text=\"{Binding AmountDisplayText}\"", xaml);
        Assert.Contains("Text=\"{Binding ExchangeInfoText}\"", xaml);
        Assert.Contains("IsVisible=\"{Binding HasExchangeInfo}\"", xaml);
    }

    [Fact]
    public void TransactionList_shows_category_name_per_row_between_date_and_note()
    {
        var xaml = ReadTransactionListXaml();

        Assert.Contains("Text=\"{Binding CategoryName}\"", xaml);
    }

    private static string ReadTransactionListXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionListPage.xaml"));

        return File.ReadAllText(path);
    }
}
