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
    }

    [Fact]
    public void TransactionList_shows_daily_income_expense_and_balance_summary()
    {
        var xaml = ReadTransactionListXaml();

        Assert.Contains("Text=\"當日總覽\"", xaml);
        Assert.Contains("Text=\"{Binding DailyIncome, StringFormat='收入 {0:N0}'}\"", xaml);
        Assert.Contains("Text=\"{Binding DailyExpense, StringFormat='支出 {0:N0}'}\"", xaml);
        Assert.Contains("Text=\"{Binding DailyBalance, StringFormat='結餘 {0:N0}'}\"", xaml);
    }

    private static string ReadTransactionListXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionListPage.xaml"));

        return File.ReadAllText(path);
    }
}
