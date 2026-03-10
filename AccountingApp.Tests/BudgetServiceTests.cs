using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class BudgetServiceTests
{
    private static (BudgetService Budget, TransactionService Txn, FakePreferenceStore Prefs)
        BuildSvcs(DatabaseService db)
    {
        var prefs = new FakePreferenceStore();
        prefs.Set("base_currency", "TWD");
        var currency = new CurrencyService(db);
        var txn = new TransactionService(db, currency, prefs);
        var budget = new BudgetService(db, txn, currency, prefs);
        return (budget, txn, prefs);
    }

    // ── SetBudget ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SetBudget_creates_new_budget_for_category_and_month()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, _, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(categoryId: 1, amount: 5000, month);
        var budgets = await svc.GetByMonthAsync(month);

        Assert.Single(budgets);
        Assert.Equal(5000, budgets[0].Amount);
    }

    [Fact]
    public async Task SetBudget_updates_existing_budget_for_same_category_and_month()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, _, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 3000, month);
        await svc.SetBudgetAsync(1, 5000, month);
        var budgets = await svc.GetByMonthAsync(month);

        Assert.Single(budgets);
        Assert.Equal(5000, budgets[0].Amount);
    }

    // ── Usage ratio ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsageRatio_returns_zero_when_no_budget_set()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, _, _) = BuildSvcs(db.Service);

        var ratio = await svc.GetUsageRatioAsync(categoryId: 99, "2026-03");

        Assert.Equal(0, ratio);
    }

    [Fact]
    public async Task GetUsageRatio_returns_50_percent_when_half_spent()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(categoryId: 1, amount: 1000, month);
        await txn.AddAsync(new Transaction
        {
            Amount = 500, Currency = "TWD", Type = "expense",
            CategoryId = 1, Date = new DateTime(2026, 3, 15)
        });

        var ratio = await svc.GetUsageRatioAsync(1, month);

        Assert.Equal(0.5m, ratio);
    }

    [Fact]
    public async Task GetUsageRatio_exceeds_1_when_over_budget()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 500, month);
        await txn.AddAsync(new Transaction
        {
            Amount = 800, Currency = "TWD", Type = "expense",
            CategoryId = 1, Date = new DateTime(2026, 3, 15)
        });

        var ratio = await svc.GetUsageRatioAsync(1, month);

        Assert.True(ratio > 1.0m);
    }

    [Fact]
    public async Task GetUsageRatio_ignores_income_transactions()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 1000, month);
        // Income should not count towards budget
        await txn.AddAsync(new Transaction
        {
            Amount = 999, Currency = "TWD", Type = "income",
            CategoryId = 1, Date = new DateTime(2026, 3, 1)
        });

        var ratio = await svc.GetUsageRatioAsync(1, month);

        Assert.Equal(0, ratio);
    }

    // ── Notification logic ─────────────────────────────────────────────────

    [Fact]
    public async Task ShouldNotify_returns_true_when_over_80_percent_and_not_yet_notified()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 1000, month);
        await txn.AddAsync(new Transaction
        {
            Amount = 850, Currency = "TWD", Type = "expense",
            CategoryId = 1, Date = new DateTime(2026, 3, 1)
        });

        var should = await svc.ShouldNotifyAsync(1, month);

        Assert.True(should);
    }

    [Fact]
    public async Task ShouldNotify_returns_false_after_already_notified()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 1000, month);
        await txn.AddAsync(new Transaction
        {
            Amount = 850, Currency = "TWD", Type = "expense",
            CategoryId = 1, Date = new DateTime(2026, 3, 1)
        });

        svc.MarkNotified(1, month);
        var should = await svc.ShouldNotifyAsync(1, month);

        Assert.False(should);
    }

    [Fact]
    public async Task ShouldNotify_returns_false_when_under_80_percent()
    {
        await using var db = await TestDb.CreateAsync();
        var (svc, txn, _) = BuildSvcs(db.Service);
        var month = "2026-03";

        await svc.SetBudgetAsync(1, 1000, month);
        await txn.AddAsync(new Transaction
        {
            Amount = 500, Currency = "TWD", Type = "expense",
            CategoryId = 1, Date = new DateTime(2026, 3, 1)
        });

        var should = await svc.ShouldNotifyAsync(1, month);

        Assert.False(should);
    }
}
