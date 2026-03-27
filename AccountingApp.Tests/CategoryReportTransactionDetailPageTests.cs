namespace AccountingApp.Tests;

public class CategoryReportTransactionDetailPageTests
{
    [Fact]
    public void CategoryReportTransactionDetailPage_includes_grouped_collection_and_empty_state_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryReportTransactionDetailPage.xaml"));

        Assert.True(File.Exists(path), $"Expected file to exist: {path}");

        var xaml = File.ReadAllText(path);

        Assert.Contains("CollectionView", xaml);
        Assert.Contains("IsGrouped=\"True\"", xaml);
        Assert.Contains("DateLabel", xaml);
        Assert.Contains("AmountText", xaml);
        Assert.Contains("CategoryReportTransactionDetailEmptyStateText", xaml);
        Assert.Contains("TapGestureRecognizer", xaml);
        Assert.Contains("OpenTransactionEditCommand", xaml);
    }

    [Fact]
    public void CategoryReportTransactionDetailPage_separates_note_and_currency_from_amount_column()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/CategoryReportTransactionDetailPage.xaml"));

        var xaml = File.ReadAllText(path);

        Assert.Contains("<VerticalStackLayout Grid.Column=\"0\" Spacing=\"2\">", xaml);
        Assert.DoesNotContain("RowDefinitions=\"Auto,Auto\"", xaml);
    }

    [Fact]
    public void CategoryReportTransactionDetailViewModel_exposes_grouped_output_and_period_summary_copy()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs"));

        Assert.True(File.Exists(path), $"Expected file to exist: {path}");

        var code = File.ReadAllText(path);

        Assert.Contains("ObservableCollection<TransactionDateGroup>", code);
        Assert.Contains("PeriodLabel", code);
        Assert.Contains("TotalAmountText", code);
        Assert.Contains("AmountText", code);
        Assert.Contains("HasTransactions", code);
        Assert.Contains("OpenTransactionEditCommand", code);
        Assert.Contains("CategoryReportTransactionDetailEmptyStateText", File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Strings/AppResources.resx"))));
        Assert.Contains("CategoryReportTransactionDetailEmptyStateText", File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Resources/Strings/AppResources.zh-Hant.resx"))));
        Assert.Contains("[QueryProperty(nameof(Range), \"range\")]", code);
        Assert.Contains("[QueryProperty(nameof(AnchorDate), \"anchorDate\")]", code);
    }
}
