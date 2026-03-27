# Transaction Form Frequent Categories Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a top-6 frequent category chip section to the transaction form so users can pick common categories faster while keeping the full picker as a fallback.

**Architecture:** Extend `TransactionFormViewModel` to derive per-type category usage counts from existing transaction history, expose a frequent-category collection, and keep it synchronized with the existing `SelectedCategory`. Update `TransactionFormPage.xaml` to render the frequent categories as tappable chips above the current picker, without changing save semantics or edit-mode behavior.

**Tech Stack:** .NET MAUI, MVVM (`BindableObject`, `Command`), xUnit layout and view model tests

---

### Task 1: Add failing tests for frequent category behavior

**Files:**
- Modify: `AccountingApp.Tests/TransactionFormViewModelTests.cs`
- Modify: `AccountingApp.Tests/TransactionFormLayoutTests.cs`

**Step 1: Write the failing view model tests**

Add tests that verify:
- frequent categories are limited to 6 items
- ranking is based on usage count descending, then category name ascending for ties
- only categories matching the selected `Type` appear
- selecting a frequent category updates `SelectedCategory`
- editing an existing transaction preserves a category even when it is not in the top 6

**Step 2: Write the failing layout test**

Add assertions that `TransactionFormPage.xaml` contains:
- a frequent-category section label
- a bindable items surface for frequent categories
- chip tap binding to a view model command
- the existing picker fallback

**Step 3: Run tests to verify they fail**

Run:
```bash
dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormViewModelTests|FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: FAIL because the frequent-category collection, command, and layout do not exist yet.

**Step 4: Commit**

```bash
git add AccountingApp.Tests/TransactionFormViewModelTests.cs AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "test: cover transaction form frequent categories"
```

### Task 2: Implement frequent category ranking in the view model

**Files:**
- Modify: `AccountingApp/ViewModels/TransactionFormViewModel.cs`
- Check: `AccountingApp/Services/TransactionService.cs`
- Check: `AccountingApp/Services/CategoryService.cs`

**Step 1: Add frequent-category view model state**

Add:
- a small view model or record for frequent-category chip items if needed
- `ObservableCollection<...> FrequentCategories`
- `ICommand SelectFrequentCategoryCommand`

**Step 2: Add ranking logic**

Use `TransactionService.GetAllAsync()` and `CategoryService.GetByTypeAsync(Type)` to:
- count transactions by `CategoryId` for the current `Type`
- join counts with the current type’s categories
- sort by usage count descending, then category name ascending
- take the first 6

**Step 3: Keep frequent categories synchronized**

Update category-loading flow so that:
- changing `Type` refreshes both `Categories` and `FrequentCategories`
- selecting a chip sets `SelectedCategory`
- editing an existing transaction still loads the original category correctly

**Step 4: Run focused tests**

Run:
```bash
dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormViewModelTests"
```

Expected: PASS for new view model behavior tests.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/TransactionFormViewModel.cs AccountingApp.Tests/TransactionFormViewModelTests.cs
git commit -m "feat: rank frequent transaction categories"
```

### Task 3: Render frequent category chips in the transaction form

**Files:**
- Modify: `AccountingApp/Views/TransactionFormPage.xaml`
- Modify: `AccountingApp/Resources/Strings/AppResources.resx`
- Modify: `AccountingApp/Resources/Strings/AppResources.zh-Hant.resx`
- Test: `AccountingApp.Tests/TransactionFormLayoutTests.cs`

**Step 1: Add user-facing copy**

Add a localized string for the frequent-category section label, such as:
- English: `Frequent Categories`
- zh-Hant: `常用分類`

**Step 2: Add the chip section**

In the category area of `TransactionFormPage.xaml`:
- insert a label for frequent categories above the existing picker
- render frequent categories with a bindable layout or flex layout
- bind chip tap to `SelectFrequentCategoryCommand`
- visually distinguish selected vs unselected chips
- keep the existing picker below as fallback

**Step 3: Keep the picker behavior unchanged**

Ensure:
- `Picker` still binds to `Categories` and `SelectedCategory`
- save flow continues to use `SelectedCategory`
- no date, amount, or currency interactions are changed

**Step 4: Run focused layout tests**

Run:
```bash
dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: PASS with new frequent-category layout assertions.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Resources/Strings/AppResources.resx AccountingApp/Resources/Strings/AppResources.zh-Hant.resx AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "feat: add frequent category chips to transaction form"
```

### Task 4: Final verification

**Files:**
- Verify: `AccountingApp/ViewModels/TransactionFormViewModel.cs`
- Verify: `AccountingApp/Views/TransactionFormPage.xaml`
- Verify: `AccountingApp.Tests/TransactionFormViewModelTests.cs`
- Verify: `AccountingApp.Tests/TransactionFormLayoutTests.cs`

**Step 1: Run the focused transaction form tests**

Run:
```bash
dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormViewModelTests|FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: PASS

**Step 2: Run the full test suite**

Run:
```bash
dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS with no regressions.

**Step 3: Commit final cleanup if needed**

```bash
git add AccountingApp/ViewModels/TransactionFormViewModel.cs AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Resources/Strings/AppResources.resx AccountingApp/Resources/Strings/AppResources.zh-Hant.resx AccountingApp.Tests/TransactionFormViewModelTests.cs AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "test: finalize transaction form frequent category coverage"
```
