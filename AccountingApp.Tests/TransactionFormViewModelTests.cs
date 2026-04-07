using AccountingApp.Core.Models;
using AccountingApp.Services;
using AccountingApp.Tests.Helpers;
using AccountingApp.ViewModels;
using AppCategory = AccountingApp.Models.Category;
using AppTransaction = AccountingApp.Models.Transaction;
using CoreCategory = AccountingApp.Core.Models.Category;
using CoreTransaction = AccountingApp.Core.Models.Transaction;
using FrequentCategoryRanking = AccountingApp.Core.Services.FrequentCategoryRanking;

namespace AccountingApp.Tests;

public class TransactionFormViewModelTests
{
    [Fact]
    public void Frequent_categories_are_limited_to_six_and_ranked_by_usage_then_name_for_the_selected_type()
    {
        List<CoreCategory> categories =
        [
            Category(1, "Alpha", "expense"),
            Category(2, "Bravo", "expense"),
            Category(3, "Charlie", "expense"),
            Category(4, "Delta", "expense"),
            Category(5, "Echo", "expense"),
            Category(6, "Foxtrot", "expense"),
            Category(7, "Golf", "expense"),
            Category(8, "Salary", "income")
        ];

        List<CoreTransaction> transactions =
        [
            Transaction(1, "expense"), Transaction(1, "expense"),
            Transaction(2, "expense"), Transaction(2, "expense"),
            Transaction(3, "expense"),
            Transaction(4, "expense"),
            Transaction(5, "expense"), Transaction(5, "expense"), Transaction(5, "expense"),
            Transaction(6, "expense"), Transaction(6, "expense"), Transaction(6, "expense"),
            Transaction(7, "expense"),
            Transaction(8, "income"), Transaction(8, "income")
        ];

        var frequentCategories = FrequentCategoryRanking.BuildFrequentCategories(categories, transactions, "expense");

        Assert.Equal(6, frequentCategories.Count);
        Assert.Equal(
            ["Echo", "Foxtrot", "Alpha", "Bravo", "Charlie", "Delta"],
            frequentCategories.Select(category => category.Name).ToArray());
        Assert.All(frequentCategories, category => Assert.Equal("expense", category.Type));
    }

    [Fact]
    public void Frequent_categories_only_include_the_selected_type()
    {
        List<CoreCategory> categories =
        [
            Category(1, "Salary", "income"),
            Category(2, "Bonus", "income"),
            Category(3, "Rent", "expense"),
            Category(4, "Food", "expense")
        ];

        List<CoreTransaction> transactions =
        [
            Transaction(1, "income"), Transaction(1, "income"),
            Transaction(2, "income"),
            Transaction(3, "expense"), Transaction(3, "expense"),
            Transaction(4, "expense")
        ];

        var frequentCategories = FrequentCategoryRanking.BuildFrequentCategories(categories, transactions, "income");

        Assert.Equal(["Salary", "Bonus"], frequentCategories.Select(category => category.Name).ToArray());
        Assert.All(frequentCategories, category => Assert.Equal("income", category.Type));
    }

    [Fact]
    public void ResolveSelectedCategory_prefers_the_requested_category_even_when_it_is_not_frequent()
    {
        List<CoreCategory> categories =
        [
            Category(1, "Archive", "expense"),
            Category(2, "Alpha", "expense"),
            Category(3, "Bravo", "expense"),
            Category(4, "Charlie", "expense"),
            Category(5, "Delta", "expense"),
            Category(6, "Echo", "expense"),
            Category(7, "Foxtrot", "expense"),
            Category(8, "Golf", "expense")
        ];

        List<CoreTransaction> transactions =
        [
            Transaction(2, "expense"), Transaction(2, "expense"),
            Transaction(3, "expense"), Transaction(3, "expense"),
            Transaction(4, "expense"), Transaction(4, "expense"),
            Transaction(5, "expense"), Transaction(5, "expense"),
            Transaction(6, "expense"), Transaction(6, "expense"),
            Transaction(7, "expense"), Transaction(7, "expense"),
            Transaction(8, "expense")
        ];

        var frequentCategories = FrequentCategoryRanking.BuildFrequentCategories(categories, transactions, "expense");
        var selected = FrequentCategoryRanking.ResolveSelectedCategory(categories, 1, frequentCategories.FirstOrDefault());

        Assert.NotNull(selected);
        Assert.Equal(1, selected!.Id);
        Assert.Equal("Archive", selected.Name);
    }

    [Fact]
    public async Task Changing_type_refreshes_frequent_categories_for_the_selected_type()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(store, "expense", ("Alpha", 2), ("Bravo", 2), ("Charlie", 2), ("Delta", 2), ("Echo", 2), ("Foxtrot", 2), ("Golf", 1));
        SeedCategoryUsage(store, "income", ("Salary", 3), ("Bonus", 2), ("Gift", 1));
        var vm = BuildViewModel(store);

        await vm.InitializeAsync();
        vm.Type = "income";

        await WaitForConditionAsync(() =>
            vm.FrequentCategories.Count == 3 &&
            vm.FrequentCategories.All(category => category.Type == "income"));

        Assert.Equal(["Salary", "Bonus", "Gift"], vm.FrequentCategories.Select(category => category.Name).ToArray());
    }

    [Fact]
    public async Task SelectFrequentCategoryCommand_updates_the_selected_category()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(store, "expense", ("Alpha", 2), ("Bravo", 1));
        var vm = BuildViewModel(store);

        await vm.InitializeAsync();

        var chip = vm.FrequentCategories.First(category => category.Name == "Alpha");
        vm.SelectFrequentCategoryCommand.Execute(chip);

        Assert.Same(chip, vm.SelectedCategory);
    }

    [Fact]
    public async Task Editing_existing_transaction_preserves_the_saved_category_even_when_it_is_not_frequent()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(
            store,
            "expense",
            ("Archive", 1),
            ("Alpha", 3),
            ("Bravo", 3),
            ("Charlie", 3),
            ("Delta", 3),
            ("Echo", 3),
            ("Foxtrot", 3),
            ("Golf", 3),
            ("Hotel", 3),
            ("India", 3));
        var archiveCategoryId = store.Categories.Single(category => category.Name == "Archive" && category.Type == "expense").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = archiveCategoryId,
            Date = new DateTime(2026, 3, 1),
            Note = "archive-edit",
            Type = "expense"
        });

        var vm = BuildViewModel(store);

        await vm.InitializeAsync();
        vm.TransactionId = 1;

        await WaitForConditionAsync(() => vm.SelectedCategory?.Id == archiveCategoryId);

        Assert.Equal(archiveCategoryId, vm.SelectedCategory?.Id);
        Assert.Equal("Archive", vm.SelectedCategory?.Name);
        Assert.DoesNotContain(vm.FrequentCategories, category => category.Id == archiveCategoryId);
    }

    [Fact]
    public async Task Editing_existing_transaction_raises_selected_category_change_notification_for_the_saved_category()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(
            store,
            "expense",
            ("Archive", 1),
            ("Alpha", 3),
            ("Bravo", 3),
            ("Charlie", 3),
            ("Delta", 3),
            ("Echo", 3),
            ("Foxtrot", 3),
            ("Golf", 3));
        var archiveCategoryId = store.Categories.Single(category => category.Name == "Archive" && category.Type == "expense").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = archiveCategoryId,
            Date = new DateTime(2026, 3, 1),
            Note = "archive-edit",
            Type = "expense"
        });

        var vm = BuildViewModel(store);
        var selectedCategoryNotifications = 0;
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(TransactionFormViewModel.SelectedCategory))
            {
                selectedCategoryNotifications++;
            }
        };

        await vm.InitializeAsync();
        selectedCategoryNotifications = 0;

        vm.TransactionId = 1;

        await WaitForConditionAsync(() => vm.SelectedCategory?.Id == archiveCategoryId);

        Assert.True(
            selectedCategoryNotifications > 0,
            "Editing should notify the UI when the saved category is restored.");
    }

    [Fact]
    public async Task Editing_existing_transaction_loads_the_saved_attachment_image()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(store, "expense", ("Food", 1));
        var categoryId = store.Categories.Single(category => category.Name == "Food").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = categoryId,
            Date = new DateTime(2026, 4, 7),
            Note = "receipt",
            Type = "expense",
            ImageRelativePath = "receipts/2026/04/original.jpg"
        });

        var vm = BuildViewModel(store);
        await vm.InitializeAsync();

        vm.TransactionId = 99;

        await WaitForConditionAsync(() => vm.AttachmentImageRelativePath == "receipts/2026/04/original.jpg");

        Assert.True(vm.HasAttachmentImage);
        Assert.Equal("receipts/2026/04/original.jpg", vm.AttachmentImageRelativePath);
    }

    [Fact]
    public async Task StageAttachmentImage_replaces_the_visible_attachment_without_changing_the_saved_transaction()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(store, "expense", ("Food", 1));
        var categoryId = store.Categories.Single(category => category.Name == "Food").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = categoryId,
            Date = new DateTime(2026, 4, 7),
            Note = "receipt",
            Type = "expense",
            ImageRelativePath = "receipts/2026/04/original.jpg"
        });

        var vm = BuildViewModel(store);
        await vm.InitializeAsync();
        vm.TransactionId = 99;
        await WaitForConditionAsync(() => vm.HasAttachmentImage);

        vm.StageAttachmentImage("receipts/2026/04/replacement.jpg");

        Assert.True(vm.HasAttachmentImage);
        Assert.Equal("receipts/2026/04/replacement.jpg", vm.AttachmentImageRelativePath);
        Assert.Equal("receipts/2026/04/original.jpg", store.Transactions.Single(transaction => transaction.Id == 99).ImageRelativePath);
    }

    [Fact]
    public async Task RemoveAttachmentImage_clears_the_visible_attachment_for_an_existing_transaction()
    {
        var store = new TransactionFormTestStore();
        SeedCategoryUsage(store, "expense", ("Food", 1));
        var categoryId = store.Categories.Single(category => category.Name == "Food").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = categoryId,
            Date = new DateTime(2026, 4, 7),
            Note = "receipt",
            Type = "expense",
            ImageRelativePath = "receipts/2026/04/original.jpg"
        });

        var vm = BuildViewModel(store);
        await vm.InitializeAsync();
        vm.TransactionId = 99;
        await WaitForConditionAsync(() => vm.HasAttachmentImage);

        vm.RemoveAttachmentImage();

        Assert.False(vm.HasAttachmentImage);
        Assert.Null(vm.AttachmentImageRelativePath);
    }

    [Fact]
    public async Task Save_failure_preserves_the_original_persisted_attachment_and_does_not_delete_it()
    {
        var store = new TransactionFormTestStore
        {
            UpdateException = new InvalidOperationException("save failed")
        };
        SeedCategoryUsage(store, "expense", ("Food", 1));
        var categoryId = store.Categories.Single(category => category.Name == "Food").Id;
        store.Transactions.Add(new AppTransaction
        {
            Id = 99,
            Amount = 10,
            Currency = "TWD",
            CategoryId = categoryId,
            Date = new DateTime(2026, 4, 7),
            Note = "receipt",
            Type = "expense",
            ImageRelativePath = "receipts/2026/04/original.jpg"
        });

        var vm = BuildViewModel(store);
        await vm.InitializeAsync();
        vm.TransactionId = 99;
        await WaitForConditionAsync(() => vm.HasAttachmentImage);
        vm.AmountText = "10";
        vm.SelectedCategory = vm.Categories.Single(category => category.Id == categoryId);
        vm.StageAttachmentImage("receipts/2026/04/replacement.jpg");

        await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeSaveAsync(vm));

        Assert.Equal("receipts/2026/04/original.jpg", store.Transactions.Single(transaction => transaction.Id == 99).ImageRelativePath);
        Assert.Empty(store.DeletedImagePaths);
    }

    private static TransactionFormViewModel BuildViewModel(TransactionFormTestStore store)
    {
        var transactionService = new TransactionService(store);
        var categoryService = new CategoryService(store);
        var budgetService = new BudgetService(store);
        var transactionImageService = new TransactionImageService(store);

        return new TransactionFormViewModel(
            transactionService,
            transactionImageService,
            categoryService,
            budgetService,
            new TestLocalizationService());
    }

    private static Task InvokeSaveAsync(TransactionFormViewModel viewModel)
    {
        var method = typeof(TransactionFormViewModel).GetMethod("SaveAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        var result = method!.Invoke(viewModel, null);
        Assert.NotNull(result);
        return (Task)result!;
    }

    private static void SeedCategoryUsage(
        TransactionFormTestStore store,
        string type,
        params (string Name, int Count)[] categories)
    {
        var nextCategoryId = store.Categories.Count == 0 ? 1 : store.Categories.Max(category => category.Id) + 1;
        var nextTransactionId = store.Transactions.Count == 0 ? 1 : store.Transactions.Max(transaction => transaction.Id) + 1;

        foreach (var (name, count) in categories)
        {
            var category = new AppCategory
            {
                Id = nextCategoryId++,
                Name = name,
                Type = type
            };
            store.Categories.Add(category);

            for (var i = 0; i < count; i++)
            {
                store.Transactions.Add(new AppTransaction
                {
                    Id = nextTransactionId++,
                    Amount = 10,
                    Currency = "TWD",
                    CategoryId = category.Id,
                    Date = new DateTime(2026, 3, 1),
                    Note = $"{name}-{i}",
                    Type = type
                });
            }
        }
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, int timeoutMs = 2_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(25);
        }

        Assert.True(condition(), "Timed out waiting for the view model to finish loading.");
    }

    private static CoreCategory Category(int id, string name, string type) => new()
    {
        Id = id,
        Name = name,
        Type = type
    };

    private static CoreTransaction Transaction(int categoryId, string type) => new()
    {
        CategoryId = categoryId,
        Type = type,
        Amount = 10,
        Currency = "TWD",
        Date = new DateTime(2026, 3, 1)
    };
}
