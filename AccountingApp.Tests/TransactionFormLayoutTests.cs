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
        Assert.Contains("ColumnDefinitions=\"*,Auto\"", xaml);
        Assert.Contains("Text=\"⌄\"", xaml);
    }

    [Fact]
    public void TransactionForm_amount_entry_binds_to_amounttext_and_uses_numeric_keyboard()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.Contains("Text=\"{Binding AmountText}\"", xaml);
        Assert.Contains("Keyboard=\"Numeric\"", xaml);
        Assert.Contains("Text=\"{Binding PageTitle}\"", xaml);
        Assert.Contains("Text=\"{Binding SaveButtonText}\"", xaml);
    }

    [Fact]
    public void TransactionForm_includes_frequent_category_chips_above_the_full_picker()
    {
        var xaml = ReadTransactionFormXaml();

        Assert.Contains("FrequentCategoriesLabel", xaml);
        Assert.Contains("FrequentCategories", xaml);
        Assert.Contains("SelectFrequentCategoryCommand", xaml);
        Assert.Contains("BindableLayout.ItemsSource=\"{Binding FrequentCategories}\"", xaml);
        Assert.Contains("FlexLayout", xaml);
        Assert.Contains("CategorySelectionEqualsConverter", xaml);
        Assert.DoesNotContain("SelectionMode=\"Single\"", xaml);
        Assert.DoesNotContain("SelectedItem=\"{Binding SelectedCategory, Mode=TwoWay}\"", xaml);
        Assert.Contains("<Picker", xaml);
        Assert.Contains("ItemsSource=\"{Binding Categories}\"", xaml);
    }

    [Fact]
    public void TransactionForm_viewmodel_exposes_edit_specific_copy_properties()
    {
        var code = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/TransactionFormViewModel.cs")));

        Assert.Contains("TransactionFormEditTitle", code);
        Assert.Contains("TransactionFormUpdateButton", code);
        Assert.Contains("PageTitle", code);
        Assert.Contains("SaveButtonText", code);
    }

    [Fact]
    public void TransactionForm_amount_entry_moves_focus_to_note_entry_on_enter()
    {
        var xaml = ReadTransactionFormXaml();
        var code = ReadTransactionFormCodeBehind();
        var amountHandlerStart = code.IndexOf("private void OnAmountEntryCompleted", StringComparison.Ordinal);
        Assert.True(amountHandlerStart >= 0, "Expected OnAmountEntryCompleted handler in code-behind.");
        var amountHandlerEnd = code.IndexOf("private async void OnCalendarOpened", amountHandlerStart, StringComparison.Ordinal);
        Assert.True(amountHandlerEnd > amountHandlerStart, "Expected calendar opened handler after amount handler.");
        var amountHandlerSnippet = code[amountHandlerStart..amountHandlerEnd];

        Assert.Contains("x:Name=\"AmountEntry\"", xaml);
        Assert.Contains("ios:IosEntryAccessory.Next=\"True\"", xaml);
        Assert.Contains("x:Name=\"CategoryPicker\"", xaml);
        Assert.Contains("x:Name=\"FormCalendarDatePicker\"", xaml);
        Assert.Contains("x:Name=\"NoteEntry\"", xaml);
        Assert.Contains("ReturnType=\"Next\"", xaml);
        Assert.Contains("Completed=\"OnAmountEntryCompleted\"", xaml);
        Assert.Contains("ReturnType=\"Done\"", xaml);
        Assert.Contains("CategoryPicker.Focus();", amountHandlerSnippet);
        Assert.DoesNotContain("NoteEntry.Focus();", amountHandlerSnippet);
    }

    [Fact]
    public void TransactionForm_scrolls_calendar_into_view_and_continues_to_note()
    {
        var xaml = ReadTransactionFormXaml();
        var code = ReadTransactionFormCodeBehind();

        Assert.Contains("x:Name=\"FormScrollView\"", xaml);
        Assert.Contains("FormCalendarDatePicker.CalendarOpened += OnCalendarOpened;", code);
        Assert.Contains("FormCalendarDatePicker.CalendarCompleted += OnCalendarCompleted;", code);
        Assert.Contains("await FormScrollView.ScrollToAsync(0,", code);
        Assert.DoesNotContain("ScrollToAsync(FormCalendarDatePicker", code);
        Assert.Contains("NoteEntry.Focus();", code);
    }

    [Fact]
    public void TransactionForm_includes_attachment_section_and_media_actions()
    {
        var xaml = ReadTransactionFormXaml();
        var code = ReadTransactionFormCodeBehind();

        Assert.Contains("TransactionFormAttachmentLabel", xaml);
        Assert.Contains("TransactionFormAttachmentCameraButton", xaml);
        Assert.Contains("TransactionFormAttachmentPhotoLibraryButton", xaml);
        Assert.Contains("TransactionFormAttachmentViewButton", xaml);
        Assert.Contains("TransactionFormAttachmentReplaceButton", xaml);
        Assert.Contains("TransactionFormAttachmentRemoveButton", xaml);
        Assert.Contains("HasAttachmentImage", xaml);
        Assert.Contains("OnCaptureAttachmentClicked", xaml);
        Assert.Contains("OnPickAttachmentFromLibraryClicked", xaml);
        Assert.Contains("OnViewAttachmentClicked", xaml);
        Assert.Contains("OnReplaceAttachmentClicked", xaml);
        Assert.Contains("OnRemoveAttachmentClicked", xaml);
        Assert.Contains("private async void OnCaptureAttachmentClicked", code);
        Assert.Contains("private async void OnPickAttachmentFromLibraryClicked", code);
        Assert.Contains("private async void OnViewAttachmentClicked", code);
        Assert.Contains("private void OnRemoveAttachmentClicked", code);
    }

    [Fact]
    public void TransactionForm_registers_image_viewer_route_and_uses_media_picker_flow()
    {
        var shellCode = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/AppShell.xaml.cs")));
        var mauiProgramCode = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/MauiProgram.cs")));
        var formCode = ReadTransactionFormCodeBehind();
        var viewerXaml = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionImageViewerPage.xaml")));
        var viewerCode = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionImageViewerPage.xaml.cs")));

        Assert.Contains("TransactionImageViewerPage", shellCode);
        Assert.Contains("AddTransient<TransactionImageViewerPage>();", mauiProgramCode);
        Assert.Contains("MediaPicker.Default.CapturePhotoAsync", formCode);
        Assert.Contains("MediaPicker.Default.PickPhotoAsync", formCode);
        Assert.Contains("StageAttachmentImage", formCode);
        Assert.Contains("private async Task PickAttachmentFromLibraryAsync()", formCode);
        Assert.Contains("await PickAttachmentFromLibraryAsync();", formCode);
        Assert.Contains("await photo.OpenReadAsync()", formCode);
        Assert.Contains("ImportAsync(sourceStream, photo.FileName)", formCode);
        Assert.Contains("OnViewAttachmentClicked", formCode);
        Assert.Contains("Shell.Current.GoToAsync(nameof(TransactionImageViewerPage)", formCode);
        Assert.Contains("x:Class=\"AccountingApp.Views.TransactionImageViewerPage\"", viewerXaml);
        Assert.Contains("Image", viewerXaml);
        Assert.Contains("QueryProperty", viewerCode);
        Assert.Contains("ImageSource.FromFile", viewerCode);
    }

    private static string ReadTransactionFormXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionFormPage.xaml"));

        return File.ReadAllText(path);
    }

    private static string ReadTransactionFormCodeBehind()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/TransactionFormPage.xaml.cs"));

        return File.ReadAllText(path);
    }

    private static int IndexOf(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Expected to find '{value}' in TransactionFormPage.xaml");
        return index;
    }
}
