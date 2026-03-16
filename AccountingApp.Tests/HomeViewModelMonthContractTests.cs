namespace AccountingApp.Tests;

public class HomeViewModelMonthContractTests
{
    [Fact]
    public void HomeViewModel_defines_range_navigation_commands_and_period_label()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("public ICommand PreviousMonthCommand", source);
        Assert.Contains("public ICommand NextMonthCommand", source);
        Assert.Contains("public ICommand SetRangeCommand", source);
        Assert.Contains("public string PeriodLabel", source);
        Assert.Contains("public string SummaryTitle", source);
    }

    [Fact]
    public void HomeViewModel_loads_summary_and_recent_transactions_from_selected_range()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("GetSummaryAsync(_selectedRange, _anchorDate)", source);
        Assert.Contains("GetByDateRangeAsync(window.Start, window.EndExclusive)", source);
    }

    private static string ReadHomeViewModel()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/HomeViewModel.cs"));

        return File.ReadAllText(path);
    }
}
