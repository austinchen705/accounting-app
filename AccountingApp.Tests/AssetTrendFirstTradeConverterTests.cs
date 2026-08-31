using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AssetTrendFirstTradeConverterTests
{
    [Theory]
    [InlineData(1.0, true)]
    [InlineData(31.5, false)]
    [InlineData(0.5, false)]
    public void HasRateError_only_true_for_the_1_0_sentinel(double exchangeRate, bool expected)
    {
        Assert.Equal(expected, AssetTrendFirstTradeConverter.HasRateError(exchangeRate));
    }

    [Fact]
    public void ConvertToBaseCurrency_new_record_multiplies_by_rate_and_rounds()
    {
        var result = AssetTrendFirstTradeConverter.ConvertToBaseCurrency(isEditing: false, firstTradeAmount: 1000m, exchangeRate: 31.5);

        Assert.Equal(31500.00m, result);
    }

    [Fact]
    public void ConvertToBaseCurrency_new_record_rounds_to_two_decimal_places()
    {
        var result = AssetTrendFirstTradeConverter.ConvertToBaseCurrency(isEditing: false, firstTradeAmount: 3m, exchangeRate: 33.333);

        Assert.Equal(100.00m, result);
    }

    [Fact]
    public void ConvertToBaseCurrency_editing_record_returns_stored_amount_unchanged()
    {
        var result = AssetTrendFirstTradeConverter.ConvertToBaseCurrency(isEditing: true, firstTradeAmount: 31250m, exchangeRate: 99.0);

        Assert.Equal(31250m, result);
    }

    [Fact]
    public void ShouldBlockSubmission_true_when_new_record_has_amount_and_rate_unavailable()
    {
        var result = AssetTrendFirstTradeConverter.ShouldBlockSubmission(isEditing: false, firstTradeAmount: 100m, exchangeRate: 1.0);

        Assert.True(result);
    }

    [Fact]
    public void ShouldBlockSubmission_false_when_amount_is_zero_even_if_rate_unavailable()
    {
        var result = AssetTrendFirstTradeConverter.ShouldBlockSubmission(isEditing: false, firstTradeAmount: 0m, exchangeRate: 1.0);

        Assert.False(result);
    }

    [Fact]
    public void ShouldBlockSubmission_false_when_editing_even_if_rate_unavailable()
    {
        var result = AssetTrendFirstTradeConverter.ShouldBlockSubmission(isEditing: true, firstTradeAmount: 100m, exchangeRate: 1.0);

        Assert.False(result);
    }

    [Fact]
    public void ShouldBlockSubmission_false_when_rate_available()
    {
        var result = AssetTrendFirstTradeConverter.ShouldBlockSubmission(isEditing: false, firstTradeAmount: 100m, exchangeRate: 31.5);

        Assert.False(result);
    }

    [Fact]
    public void ConvertBaseCurrencyToInputAmount_divides_by_rate_and_rounds()
    {
        var result = AssetTrendFirstTradeConverter.ConvertBaseCurrencyToInputAmount(baseCurrencyAmount: 31500m, exchangeRate: 31.5);

        Assert.Equal(1000.00m, result);
    }

    [Fact]
    public void ConvertBaseCurrencyToInputAmount_round_trips_with_ConvertToBaseCurrency()
    {
        var converted = AssetTrendFirstTradeConverter.ConvertToBaseCurrency(isEditing: false, firstTradeAmount: 1000m, exchangeRate: 31.5);
        var roundTripped = AssetTrendFirstTradeConverter.ConvertBaseCurrencyToInputAmount(converted, exchangeRate: 31.5);

        Assert.Equal(1000.00m, roundTripped);
    }

    [Fact]
    public void ShowPreview_true_only_for_new_record_with_amount_and_available_rate()
    {
        var result = AssetTrendFirstTradeConverter.ShowPreview(isEditing: false, firstTradeAmount: 100m, exchangeRate: 31.5);

        Assert.True(result);
    }

    [Theory]
    [InlineData(true, 100, 31.5)]
    [InlineData(false, 0, 31.5)]
    [InlineData(false, 100, 1.0)]
    public void ShowPreview_false_when_editing_or_no_amount_or_rate_unavailable(bool isEditing, decimal firstTradeAmount, double exchangeRate)
    {
        var result = AssetTrendFirstTradeConverter.ShowPreview(isEditing, firstTradeAmount, exchangeRate);

        Assert.False(result);
    }
}
