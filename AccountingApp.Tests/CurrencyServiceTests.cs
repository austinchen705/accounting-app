using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class CurrencyServiceTests
{
    // ── Same currency ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetRate_same_currency_returns_1()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CurrencyService(db.Service);

        var rate = await svc.GetRateAsync("TWD", "TWD");

        Assert.Equal(1.0, rate);
    }

    // ── Cache freshness ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRates_uses_cached_data_when_fresh()
    {
        await using var db = await TestDb.CreateAsync();
        // Pre-seed a fresh cache entry
        await db.Service.Db.InsertAsync(new ExchangeRateCache
        {
            BaseCurrency = "TWD",
            RatesJson = "{\"USD\":0.031,\"TWD\":1.0}",
            UpdatedAt = DateTime.UtcNow  // fresh
        });

        var svc = new CurrencyService(db.Service);
        var rates = await svc.GetRatesAsync("TWD");

        Assert.True(rates.ContainsKey("USD"));
        Assert.Equal(0.031, rates["USD"]);
    }

    [Fact]
    public async Task IsCacheStale_returns_true_when_cache_is_null()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CurrencyService(db.Service);

        Assert.True(svc.IsCacheStale(null));
    }

    [Fact]
    public async Task IsCacheStale_returns_false_when_cache_is_recent()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CurrencyService(db.Service);
        var cache = new ExchangeRateCache { UpdatedAt = DateTime.UtcNow };

        Assert.False(svc.IsCacheStale(cache));
    }

    [Fact]
    public async Task IsCacheStale_returns_true_when_cache_is_over_24_hours_old()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CurrencyService(db.Service);
        var cache = new ExchangeRateCache { UpdatedAt = DateTime.UtcNow.AddHours(-25) };

        Assert.True(svc.IsCacheStale(cache));
    }

    // ── Offline fallback ───────────────────────────────────────────────────

    [Fact]
    public async Task GetRate_falls_back_to_stale_cache_when_api_unreachable()
    {
        await using var db = await TestDb.CreateAsync();
        // Seed stale cache
        await db.Service.Db.InsertAsync(new ExchangeRateCache
        {
            BaseCurrency = "TWD",
            RatesJson = "{\"USD\":0.030,\"TWD\":1.0}",
            UpdatedAt = DateTime.UtcNow.AddHours(-48)  // stale
        });

        // Use HttpClient that always fails
        var failHttp = new HttpClient(new FailingHttpHandler());
        var svc = new CurrencyService(db.Service, failHttp);

        var rate = await svc.GetRateAsync("TWD", "USD");

        // Should fall back to stale cache value
        Assert.Equal(0.030, rate, precision: 5);
    }

    [Fact]
    public async Task GetRate_returns_1_when_no_cache_and_api_unreachable()
    {
        await using var db = await TestDb.CreateAsync();
        var failHttp = new HttpClient(new FailingHttpHandler());
        var svc = new CurrencyService(db.Service, failHttp);

        // Different-currency request with no cache → fallback
        var rate = await svc.GetRateAsync("USD", "JPY");

        // No cache + no API → returns 1.0 (fallback)
        Assert.Equal(1.0, rate);
    }

    // ── Helper ─────────────────────────────────────────────────────────────

    private class FailingHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("Simulated network failure");
    }
}
