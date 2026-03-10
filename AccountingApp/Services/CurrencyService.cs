using System.Text.Json;
using AccountingApp.Models;

namespace AccountingApp.Services;

public class CurrencyService
{
    private readonly DatabaseService _db;
    private const string ApiBase = "https://api.exchangerate-api.com/v4/latest/";
    private const int CacheHours = 24;

    public CurrencyService(DatabaseService db) => _db = db;

    /// <summary>Returns the exchange rate from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public async Task<double> GetRateAsync(string from, string to)
    {
        if (from == to) return 1.0;

        var rates = await GetRatesAsync(from);
        return rates.TryGetValue(to, out var rate) ? rate : 1.0;
    }

    private async Task<Dictionary<string, double>> GetRatesAsync(string baseCurrency)
    {
        var cache = await _db.Db.Table<ExchangeRateCache>()
            .Where(c => c.BaseCurrency == baseCurrency).FirstOrDefaultAsync();

        bool cacheValid = cache is not null &&
            (DateTime.UtcNow - cache.UpdatedAt).TotalHours < CacheHours;

        if (!cacheValid)
        {
            try
            {
                using var http = new HttpClient();
                var json = await http.GetStringAsync($"{ApiBase}{baseCurrency}");
                var doc = JsonDocument.Parse(json);
                var ratesEl = doc.RootElement.GetProperty("rates");
                var ratesJson = ratesEl.GetRawText();

                if (cache is null)
                {
                    cache = new ExchangeRateCache { BaseCurrency = baseCurrency };
                    cache.RatesJson = ratesJson;
                    cache.UpdatedAt = DateTime.UtcNow;
                    await _db.Db.InsertAsync(cache);
                }
                else
                {
                    cache.RatesJson = ratesJson;
                    cache.UpdatedAt = DateTime.UtcNow;
                    await _db.Db.UpdateAsync(cache);
                }
            }
            catch
            {
                // Network unavailable — fall back to cached data
                if (cache is null)
                    return new Dictionary<string, double> { [baseCurrency] = 1.0 };
            }
        }

        return JsonSerializer.Deserialize<Dictionary<string, double>>(cache!.RatesJson)
               ?? new Dictionary<string, double>();
    }

    public bool IsCacheStale(ExchangeRateCache? cache) =>
        cache is null || (DateTime.UtcNow - cache.UpdatedAt).TotalHours >= CacheHours;
}
