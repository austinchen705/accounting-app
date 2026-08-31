namespace AccountingApp.Core.Services;

public static class AssetTrendFirstTradeConverter
{
    // A real USD/TWD rate can never equal exactly 1.0, so a currency service falling back to
    // that sentinel value (no cache, API unreachable) doubles as our "conversion unavailable" signal.
    public static bool HasRateError(double exchangeRate) => exchangeRate == 1.0;

    public static bool ShowPreview(bool isEditing, decimal firstTradeAmount, double exchangeRate) =>
        !isEditing && firstTradeAmount != 0 && !HasRateError(exchangeRate);

    public static bool ShouldBlockSubmission(bool isEditing, decimal firstTradeAmount, double exchangeRate) =>
        !isEditing && firstTradeAmount != 0 && HasRateError(exchangeRate);

    public static decimal ConvertToBaseCurrency(bool isEditing, decimal firstTradeAmount, double exchangeRate) =>
        isEditing ? firstTradeAmount : Math.Round(firstTradeAmount * (decimal)exchangeRate, 2);

    // Inverse of ConvertToBaseCurrency's new-record path: used to prefill the (raw-currency)
    // input box from a previously stored (base-currency) amount without double-converting it.
    public static decimal ConvertBaseCurrencyToInputAmount(decimal baseCurrencyAmount, double exchangeRate) =>
        Math.Round(baseCurrencyAmount / (decimal)exchangeRate, 2);
}
