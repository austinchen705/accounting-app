using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public record AssetTrendSeriesResult(
    string[] Labels,
    decimal[] StockValues,
    decimal[] CashValues,
    decimal[] FirstTradeValues,
    decimal[] Fund3Values,
    decimal[] Totals);

public static class AssetTrendSeries
{
    public static AssetTrendSeriesResult Build(IEnumerable<AssetSnapshot> snapshots)
    {
        var ordered = snapshots.OrderBy(x => x.Date).ToList();

        return new AssetTrendSeriesResult(
            ordered.Select(x => x.Date.ToString("yyyy/MM/dd")).ToArray(),
            ordered.Select(x => x.Stock).ToArray(),
            ordered.Select(x => x.Cash).ToArray(),
            ordered.Select(x => x.FirstTrade).ToArray(),
            ordered.Select(x => x.Fund3).ToArray(),
            ordered.Select(x => x.Stock + x.Cash + x.FirstTrade + x.Fund3).ToArray());
    }
}
