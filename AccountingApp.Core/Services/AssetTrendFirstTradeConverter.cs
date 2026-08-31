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
}
