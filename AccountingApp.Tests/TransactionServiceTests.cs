using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class TransactionServiceTests
{
    private static TransactionService BuildSvc(DatabaseService db, FakePreferenceStore? prefs = null)
    {
        prefs ??= new FakePreferenceStore();
        var currency = new CurrencyService(db);
        return new TransactionService(db, currency, prefs);
    }

    // ── Add / GetAll ───────────────────────────────────────────────────────

    [Fact]
    public async Task Add_transaction_and_retrieve_it()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        await svc.AddAsync(new Transaction
        {
            Amount = 500, Currency = "TWD", Type = "expense",
            Date = DateTime.Today, Note = "lunch"
        });

        var all = await svc.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(500, all[0].Amount);
        Assert.Equal("lunch", all[0].Note);
    }

    // ── Filter by month ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByMonth_returns_only_transactions_in_that_month()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        await svc.AddAsync(new Transaction { Amount = 100, Currency = "TWD", Type = "expense", Date = new DateTime(2026, 3, 1) });
        await svc.AddAsync(new Transaction { Amount = 200, Currency = "TWD", Type = "expense", Date = new DateTime(2026, 2, 15) });

        var march = await svc.GetByMonthAsync("2026-03");
        var feb = await svc.GetByMonthAsync("2026-02");

        Assert.Single(march);
        Assert.Single(feb);
        Assert.Equal(100, march[0].Amount);
        Assert.Equal(200, feb[0].Amount);
    }

    [Fact]
    public async Task GetByMonth_returns_empty_for_month_with_no_transactions()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        var result = await svc.GetByMonthAsync("2025-01");

        Assert.Empty(result);
    }

    // ── Filter ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFiltered_by_category_returns_only_matching()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        await svc.AddAsync(new Transaction { Amount = 100, Currency = "TWD", Type = "expense", Date = DateTime.Today, CategoryId = 1 });
        await svc.AddAsync(new Transaction { Amount = 200, Currency = "TWD", Type = "expense", Date = DateTime.Today, CategoryId = 2 });

        var result = await svc.GetFilteredAsync(null, categoryId: 1, null);

        Assert.Single(result);
        Assert.Equal(1, result[0].CategoryId);
    }

    // ── Update / Delete ────────────────────────────────────────────────────

    [Fact]
    public async Task Update_changes_the_amount()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        await svc.AddAsync(new Transaction { Amount = 100, Currency = "TWD", Type = "expense", Date = DateTime.Today });
        var txn = (await svc.GetAllAsync())[0];
        txn.Amount = 999;
        await svc.UpdateAsync(txn);

        var updated = (await svc.GetAllAsync())[0];
        Assert.Equal(999, updated.Amount);
    }

    [Fact]
    public async Task Delete_removes_the_transaction()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = BuildSvc(db.Service);

        await svc.AddAsync(new Transaction { Amount = 100, Currency = "TWD", Type = "expense", Date = DateTime.Today });
        var txn = (await svc.GetAllAsync())[0];
        await svc.DeleteAsync(txn.Id);

        Assert.Empty(await svc.GetAllAsync());
    }

    // ── Monthly summary ────────────────────────────────────────────────────

    [Fact]
    public async Task GetMonthSummary_returns_correct_income_and_expense_in_same_currency()
    {
        await using var db = await TestDb.CreateAsync();
        var prefs = new FakePreferenceStore();
        prefs.Set("base_currency", "TWD");
        var svc = BuildSvc(db.Service, prefs);

        var month = DateTime.Today.ToString("yyyy-MM");
        await svc.AddAsync(new Transaction { Amount = 1000, Currency = "TWD", Type = "income", Date = DateTime.Today });
        await svc.AddAsync(new Transaction { Amount = 300, Currency = "TWD", Type = "expense", Date = DateTime.Today });
        await svc.AddAsync(new Transaction { Amount = 200, Currency = "TWD", Type = "expense", Date = DateTime.Today });

        var (income, expense) = await svc.GetMonthSummaryAsync(month);

        Assert.Equal(1000, income);
        Assert.Equal(500, expense);
    }

    [Fact]
    public async Task GetMonthSummary_excludes_other_months()
    {
        await using var db = await TestDb.CreateAsync();
        var prefs = new FakePreferenceStore();
        prefs.Set("base_currency", "TWD");
        var svc = BuildSvc(db.Service, prefs);

        await svc.AddAsync(new Transaction { Amount = 100, Currency = "TWD", Type = "expense", Date = new DateTime(2026, 3, 1) });
        await svc.AddAsync(new Transaction { Amount = 500, Currency = "TWD", Type = "expense", Date = new DateTime(2026, 2, 1) });

        var (_, expense) = await svc.GetMonthSummaryAsync("2026-03");

        Assert.Equal(100, expense);
    }
}
