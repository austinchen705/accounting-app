namespace AccountingApp.Tests;

public class HomeViewModelMonthContractTests
{
    [Fact]
    public void HomeViewModel_defines_month_navigation_commands_and_label()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("public ICommand PreviousMonthCommand", source);
        Assert.Contains("public ICommand NextMonthCommand", source);
        Assert.Contains("public string CurrentMonthLabel", source);
        Assert.Contains("public string SummaryTitle", source);
    }

    [Fact]
    public void HomeViewModel_loads_recent_transactions_from_selected_month()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("GetMonthSummaryAsync(_currentMonth)", source);
        Assert.Contains("GetByMonthAsync(_currentMonth)", source);
    }

    private static string ReadHomeViewModel()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/HomeViewModel.cs"));

        return File.ReadAllText(path);
    }
}
