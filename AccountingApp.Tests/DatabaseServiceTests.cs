using AccountingApp.Core.Models;
using AccountingApp.Core.Services;
using SQLite;

namespace AccountingApp.Tests;

public class DatabaseServiceTests
{
    [Fact]
    public async Task Initialize_does_not_repair_transaction_types_to_match_linked_categories()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        try
        {
            var seedDb = new SQLiteAsyncConnection(path);
            await seedDb.CreateTableAsync<Transaction>();
            await seedDb.CreateTableAsync<Category>();
            await seedDb.InsertAsync(new Category { Id = 1, Name = "收入", Type = "income" });
            await seedDb.InsertAsync(new Transaction
            {
                Amount = 300,
                Currency = "TWD",
                CategoryId = 1,
                Date = new DateTime(2026, 7, 1),
                Type = "expense"
            });
            await seedDb.CloseAsync();

            var svc = new DatabaseService(path);
            await svc.InitializeAsync();

            var txn = await svc.Db.Table<Transaction>().FirstAsync();
            Assert.Equal("expense", txn.Type);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
