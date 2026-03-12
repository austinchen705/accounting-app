namespace AccountingApp.Tests;

public class StatisticsAxisStepTests
{
    [Fact]
    public void StatisticsViewModel_sets_finer_axis_step_for_both_trend_charts()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var source = File.ReadAllText(path);

        Assert.Equal(2, CountOccurrences(source, "MinStep = 50_000"));
        Assert.Equal(2, CountOccurrences(source, "ForceStepToMin = true"));
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
