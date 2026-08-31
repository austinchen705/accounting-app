namespace AccountingApp.Tests;

public class AssetTrendPrefillContractTests
{
    [Fact]
    public void AssetTrendViewModel_prefill_logic_copies_latest_amounts_without_overwriting_date()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/AssetTrendViewModel.cs"));
        var vmCode = File.ReadAllText(path);

        Assert.Contains("private void PrefillLatestSnapshot()", vmCode);
        Assert.Contains("var latestSnapshot = Snapshots.FirstOrDefault();", vmCode);
        Assert.Contains("Stock = latestSnapshot.Stock;", vmCode);
        Assert.Contains("Cash = latestSnapshot.Cash;", vmCode);
        Assert.Contains("AssetTrendFirstTradeConverter.ConvertBaseCurrencyToInputAmount(", vmCode);
        Assert.Contains("latestSnapshot.FirstTrade, _firstTradeExchangeRate);", vmCode);
        Assert.Contains("Property = latestSnapshot.Property;", vmCode);
        Assert.DoesNotContain("FirstTrade = latestSnapshot.FirstTrade;", vmCode);
        Assert.DoesNotContain("SnapshotDate = latestSnapshot.Date;", vmCode);
    }
}
