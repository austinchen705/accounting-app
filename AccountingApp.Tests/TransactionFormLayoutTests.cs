namespace AccountingApp.Tests;

public class TransactionFormLayoutTests
{
    [Fact]
    public void TransactionForm_prioritizes_amount_category_and_date_before_note_and_currency()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.True(IndexOf(xaml, "TransactionFormAmountLabel") < IndexOf(xaml, "TransactionFormCategoryLabel"));
        Assert.True(IndexOf(xaml, "TransactionFormCategoryLabel") < IndexOf(xaml, "TransactionFormDateLabel"));
        Assert.True(IndexOf(xaml, "TransactionFormDateLabel") < IndexOf(xaml, "TransactionFormNoteLabel"));
        Assert.True(IndexOf(xaml, "TransactionFormNoteLabel") < IndexOf(xaml, "TransactionFormCurrencyLabel"));
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
        Assert.Contains("<controls:CalendarDatePicker", xaml);
        Assert.Contains("Date=\"{Binding Date}\"", xaml);
        Assert.Contains("ShowTrigger=\"False\"", xaml);
        Assert.Contains("x:Name=\"FormCalendarDatePicker\"", xaml);
        Assert.Contains("Source={x:Reference FormCalendarDatePicker}, Path=OpenCalendarCommand", xaml);
    }

    [Fact]
    public void TransactionForm_amount_entry_binds_to_amounttext_and_uses_numeric_keyboard()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.Contains("Text=\"{Binding AmountText}\"", xaml);
        Assert.Contains("Keyboard=\"Numeric\"", xaml);
        Assert.Contains("markup:Translate", xaml);
        Assert.Contains("TransactionFormSaveButton", xaml);
    }

    [Fact]
    public void TransactionForm_amount_entry_moves_focus_to_note_entry_on_enter()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.Contains("x:Name=\"AmountEntry\"", xaml);
        Assert.Contains("x:Name=\"NoteEntry\"", xaml);
        Assert.Contains("ReturnType=\"Next\"", xaml);
        Assert.Contains("Completed=\"OnAmountEntryCompleted\"", xaml);
        Assert.Contains("ReturnType=\"Done\"", xaml);
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
