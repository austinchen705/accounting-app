using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using AccountingApp.Tests.Helpers;

namespace AccountingApp.Tests;

public class CategoryServiceTests
{
    // ── Seed data ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DatabaseInit_seeds_default_categories()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);

        var all = await svc.GetAllAsync();

        Assert.True(all.Count >= 6, $"Expected ≥6 default categories, got {all.Count}");
        Assert.Contains(all, c => c.Name == "餐飲" && c.Type == "expense");
        Assert.Contains(all, c => c.Name == "薪資" && c.Type == "income");
    }

    // ── Add ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_new_category_returns_true_and_persists()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);

        var result = await svc.AddAsync(new Category { Name = "旅遊", Type = "expense" });
        var all = await svc.GetAllAsync();

        Assert.True(result);
        Assert.Contains(all, c => c.Name == "旅遊");
    }

    [Fact]
    public async Task Add_duplicate_name_returns_false()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);

        await svc.AddAsync(new Category { Name = "旅遊", Type = "expense" });
        var result = await svc.AddAsync(new Category { Name = "旅遊", Type = "income" });

        Assert.False(result);
    }

    [Fact]
    public async Task Add_duplicate_does_not_create_extra_entry()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);

        await svc.AddAsync(new Category { Name = "旅遊", Type = "expense" });
        await svc.AddAsync(new Category { Name = "旅遊", Type = "income" });
        var all = await svc.GetAllAsync();

        Assert.Equal(1, all.Count(c => c.Name == "旅遊"));
    }

    // ── GetByType ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByType_returns_only_matching_type()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);

        var expenses = await svc.GetByTypeAsync("expense");
        var incomes = await svc.GetByTypeAsync("income");

        Assert.All(expenses, c => Assert.Equal("expense", c.Type));
        Assert.All(incomes, c => Assert.Equal("income", c.Type));
        Assert.NotEmpty(expenses);
        Assert.NotEmpty(incomes);
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_category_with_no_transactions_succeeds()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);
        await svc.AddAsync(new Category { Name = "旅遊", Type = "expense" });
        var cat = (await svc.GetByTypeAsync("expense")).First(c => c.Name == "旅遊");

        var result = await svc.DeleteAsync(cat.Id);
        var all = await svc.GetAllAsync();

        Assert.True(result);
        Assert.DoesNotContain(all, c => c.Name == "旅遊");
    }

    [Fact]
    public async Task Delete_category_with_transactions_returns_false()
    {
        await using var db = await TestDb.CreateAsync();
        var svc = new CategoryService(db.Service);
        var cat = (await svc.GetByTypeAsync("expense")).First();

        // Insert a transaction linked to that category
        await db.Service.Db.InsertAsync(new Transaction
        {
            Amount = 100, Currency = "TWD", CategoryId = cat.Id,
            Date = DateTime.Today, Type = "expense"
        });

        var result = await svc.DeleteAsync(cat.Id);

        Assert.False(result);
        Assert.Contains(await svc.GetAllAsync(), c => c.Id == cat.Id);
    }
}
