using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class StatisticsAxisScaleHelperTests
{
    [Theory]
    [InlineData(new double[] { 1200, 4300 }, 1000)]
    [InlineData(new double[] { 18000, 22000 }, 5000)]
    [InlineData(new double[] { 52000, 91000 }, 20000)]
    [InlineData(new double[] { 180000, 260000 }, 50000)]
    [InlineData(new double[] { 0, 0, 4300 }, 1000)]
    public void CalculateStep_returns_readable_nice_steps(double[] values, double expectedStep)
    {
        Assert.Equal(expectedStep, StatisticsAxisScaleHelper.CalculateStep(values));
    }

    [Fact]
    public void CalculateStep_returns_minimum_readable_step_when_all_values_are_zero()
    {
        Assert.Equal(1000, StatisticsAxisScaleHelper.CalculateStep([0, 0, 0]));
    }
}
