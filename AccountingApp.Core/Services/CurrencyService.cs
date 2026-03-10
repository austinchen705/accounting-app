using System.Text.Json;
using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Models;

namespace AccountingApp.Core.Services;

public class CurrencyService
{
    private readonly DatabaseService _db;
    private readonly HttpClient _http;
    private const string ApiBase = "https://api.exchangerate-api.com/v4/latest/";
    private const int CacheHours = 24;

    public CurrencyService(DatabaseService db, HttpClient? http = null)
    {
        _db = db;
        _http = http ?? new HttpClient();
    }

    public async Task<double> GetRateAsync(string from, string to)
    {
        if (from == to) return 1.0;
        var rates = await GetRatesAsync(from);
        return rates.TryGetValue(to, out var rate) ? rate : 1.0;
    }

    public async Task<Dictionary<string, double>> GetRatesAsync(string baseCurrency)
    {
        var cache = await _db.Db.Table<ExchangeRateCache>()
            .Where(c => c.BaseCurrency == baseCurrency).FirstOrDefaultAsync();

        bool cacheValid = cache is not null &&
            (DateTime.UtcNow - cache.UpdatedAt).TotalHours < CacheHours;

        if (!cacheValid)
        {
            try
            {
                var json = await _http.GetStringAsync($"{ApiBase}{baseCurrency}");
                var doc = JsonDocument.Parse(json);
                var ratesJson = doc.RootElement.GetProperty("rates").GetRawText();

                if (cache is null)
                {
                    cache = new ExchangeRateCache
                    {
                        BaseCurrency = baseCurrency,
                        RatesJson = ratesJson,
                        UpdatedAt = DateTime.UtcNow
                    };
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
