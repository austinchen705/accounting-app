using AccountingApp.Core.Models;
using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AssetTrendSeriesTests
{
    [Fact]
    public void Build_returns_stacked_bucket_values_and_total_per_date()
    {
        var snapshots = new[]
        {
            new AssetSnapshot { Date = new DateTime(2026, 1, 1), Stock = 10, Cash = 20, FirstTrade = 30, Fund3 = 40 },
            new AssetSnapshot { Date = new DateTime(2026, 1, 2), Stock = 1, Cash = 2, FirstTrade = 3, Fund3 = 4 }
        };

        var result = AssetTrendSeries.Build(snapshots);

        Assert.Equal(new[] { "2026/01/01", "2026/01/02" }, result.Labels);
        Assert.Equal(new decimal[] { 100, 10 }, result.Totals);
        Assert.Equal(new decimal[] { 10, 1 }, result.StockValues);
        Assert.Equal(new decimal[] { 20, 2 }, result.CashValues);
        Assert.Equal(new decimal[] { 30, 3 }, result.FirstTradeValues);
        Assert.Equal(new decimal[] { 40, 4 }, result.Fund3Values);
    }
}
