namespace AccountingApp.Tests;

public class TransactionFormLayoutTests
{
    [Fact]
    public void TransactionForm_prioritizes_amount_category_and_date_before_note_and_currency()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.True(IndexOf(xaml, "Text=\"金額\"") < IndexOf(xaml, "Text=\"分類\""));
        Assert.True(IndexOf(xaml, "Text=\"分類\"") < IndexOf(xaml, "Text=\"日期\""));
        Assert.True(IndexOf(xaml, "Text=\"日期\"") < IndexOf(xaml, "Text=\"備註\""));
        Assert.True(IndexOf(xaml, "Text=\"備註\"") < IndexOf(xaml, "Text=\"幣別\""));
    }

    [Fact]
    public void TransactionForm_adds_extra_bottom_padding_for_keyboard_clearance()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.Contains("Padding=\"16,16,16,120\"", xaml);
    }

    [Fact]
    public void TransactionForm_uses_calendar_surface_instead_of_inline_datepicker()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.DoesNotContain("<DatePicker", xaml);
        Assert.Contains("Command=\"{Binding OpenCalendarCommand}\"", xaml);
        Assert.Contains("ItemsSource=\"{Binding CalendarDays}\"", xaml);
    }

    private static string ReadTransactionFormXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionFormPage.xaml"));

        return File.ReadAllText(path);
    }

    private static int IndexOf(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Expected to find '{value}' in TransactionFormPage.xaml");
        return index;
    }
}
