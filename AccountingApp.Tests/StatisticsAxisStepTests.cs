namespace AccountingApp.Tests;

public class StatisticsAxisStepTests
{
    [Fact]
    public void StatisticsViewModel_uses_shared_axis_scale_helper_for_trend_charts()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var source = File.ReadAllText(path);

        Assert.Contains("StatisticsAxisScaleHelper", source);
        Assert.Equal(2, CountOccurrences(source, "CreateYAxis("));
        Assert.DoesNotContain("MinStep = 50_000", source);
        Assert.Equal(2, CountOccurrences(source, "ForceStepToMin = true"));
        Assert.Equal(2, CountOccurrences(source, "Labeler = value => FormatAxisValue(value)"));
    }

    [Fact]
    public void StatisticsViewModel_calculates_dynamic_axis_steps_for_both_trend_charts()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/StatisticsViewModel.cs"));
        var source = File.ReadAllText(path);

        Assert.Equal(2, CountOccurrences(source, "StatisticsAxisScaleHelper.CalculateStep("));
        Assert.Contains("var trendAxisStep", source);
        Assert.Contains("var categoryTrendAxisStep", source);
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
