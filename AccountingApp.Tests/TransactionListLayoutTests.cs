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

    private static string ReadTransactionListXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionListPage.xaml"));

        return File.ReadAllText(path);
    }
}
