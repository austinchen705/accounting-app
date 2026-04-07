# Category Report Transaction Drilldown Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Let users tap a category row on the Category Report page and open a detail page that shows that category's matching expense transactions grouped by date from newest to oldest.

**Architecture:** Extend `StatisticsService` with a category-report drilldown query that reuses the existing range and anchor-date filtering rules, then add a dedicated detail page/view model for grouped transaction display. Keep `CategoryReportPage` as a summary screen and use navigation parameters to pass the selected category plus current report context into the new page.

**Tech Stack:** .NET MAUI, XAML, C#, xUnit, existing MVVM services, `.resx` localization resources.

---

### Task 1: Add failing tests for category-report drilldown query and grouping model

**Files:**
- Modify: `AccountingApp.Tests/ExpenseCategoryReportTests.cs`
- Create: `AccountingApp.Tests/CategoryReportTransactionDetailViewModelTests.cs`
- Test: `AccountingApp/Services/StatisticsService.cs`
- Test: `AccountingApp/ViewModels/` new detail view model

**Step 1: Write the failing test**

Add a focused test in `ExpenseCategoryReportTests.cs` that verifies a category drilldown query:

```csharp
[Fact]
public async Task GetExpenseCategoryTransactionsAsync_filters_to_selected_category_and_range()
{
    // seed income + expense rows across multiple categories and dates
    // query a single category in Month range
    // assert only matching expense rows are returned in descending date order
}
```

Create `CategoryReportTransactionDetailViewModelTests.cs` with assertions that the new detail view model groups transactions by date and orders groups newest first, for example:

```csharp
Assert.Equal("2025/06/25", groups[0].DateLabel);
Assert.Equal("2025/06/05", groups[1].DateLabel);
Assert.All(groups, group => Assert.NotEmpty(group.Items));
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~ExpenseCategoryReportTests|FullyQualifiedName~CategoryReportTransactionDetailViewModelTests"
```

Expected: FAIL because there is no drilldown query and no grouped detail view model yet.

**Step 3: Write minimal implementation**

- Add a drilldown record model in `StatisticsService.cs`, for example:

```csharp
public record ExpenseCategoryTransactionDetailStat(
    int TransactionId,
    string CategoryName,
    string Note,
    DateTime Date,
    decimal Amount,
    string Currency);
```

- Add a new service method that filters by:
  - selected category
  - current `ExpenseCategoryReportRange`
  - current anchor date
  - `expense` transactions only
- Sort the returned records newest to oldest before the view model groups them.

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Tests/ExpenseCategoryReportTests.cs AccountingApp.Tests/CategoryReportTransactionDetailViewModelTests.cs AccountingApp/Services/StatisticsService.cs
git commit -m "feat: add category report transaction drilldown query"
```

### Task 2: Make category rows selectable and navigate to a new detail page

**Files:**
- Modify: `AccountingApp/Views/CategoryReportPage.xaml`
- Modify: `AccountingApp/ViewModels/CategoryReportViewModel.cs`
- Create: `AccountingApp/Views/CategoryReportTransactionDetailPage.xaml`
- Create: `AccountingApp/Views/CategoryReportTransactionDetailPage.xaml.cs`
- Create: `AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs`
- Modify: `AccountingApp/AppShell.xaml.cs` or route registration location
- Modify: `AccountingApp.Tests/CategoryReportPageCopyTests.cs`

**Step 1: Write the failing test**

Update `CategoryReportPageCopyTests.cs` to verify that category rows are no longer passive labels only. Assert the XAML contains a command binding for row selection and references the new detail page or command, for example:

```csharp
Assert.Contains("OpenCategoryDetailCommand", xaml);
Assert.Contains("SelectionMode=\"Single\"", xaml);
```

Add assertions that the view model source contains the navigation command and current filter context passing:

```csharp
Assert.Contains("OpenCategoryDetailCommand", code);
Assert.Contains("_selectedRange", code);
Assert.Contains("_anchorDate", code);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~CategoryReportPageCopyTests"
```

Expected: FAIL because category rows are not selectable and no navigation command exists.

**Step 3: Write minimal implementation**

- Add a row-tap command to `CategoryReportViewModel`.
- Extend `CategoryReportItem` to carry category identity needed for drilldown.
- Make the category list selectable or wrap each row in a `TapGestureRecognizer`.
- Add a new detail page and register a route for it.
- Pass:
  - selected category id/name
  - current report range
  - current anchor date

Example command surface:

```csharp
public ICommand OpenCategoryDetailCommand { get; }
```

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/CategoryReportPage.xaml AccountingApp/ViewModels/CategoryReportViewModel.cs AccountingApp/Views/CategoryReportTransactionDetailPage.xaml AccountingApp/Views/CategoryReportTransactionDetailPage.xaml.cs AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs AccountingApp/AppShell.xaml.cs AccountingApp.Tests/CategoryReportPageCopyTests.cs
git commit -m "feat: add category report drilldown navigation"
```

### Task 3: Build the grouped transaction detail UI and localized copy

**Files:**
- Modify: `AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs`
- Modify: `AccountingApp/Views/CategoryReportTransactionDetailPage.xaml`
- Modify: `AccountingApp/Resources/Strings/AppResources.resx`
- Modify: `AccountingApp/Resources/Strings/AppResources.zh-Hant.resx`
- Create: `AccountingApp.Tests/CategoryReportTransactionDetailPageTests.cs`

**Step 1: Write the failing test**

Create `CategoryReportTransactionDetailPageTests.cs` to assert the new page includes:

```csharp
Assert.Contains("CollectionView", xaml);
Assert.Contains("IsGrouped=\"True\"", xaml);
Assert.Contains("DateLabel", xaml);
Assert.Contains("AmountText", xaml);
Assert.Contains("CategoryReportTransactionDetailEmptyStateText", xaml);
```

Add view model assertions for grouped output and localized header/period copy:

```csharp
Assert.Contains("ObservableCollection<DateGroup>", code);
Assert.Contains("PeriodLabel", code);
Assert.Contains("TotalAmountText", code);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~CategoryReportTransactionDetailPageTests|FullyQualifiedName~CategoryReportTransactionDetailViewModelTests"
```

Expected: FAIL because the detail page and grouped UI do not exist yet.

**Step 3: Write minimal implementation**

- Build a grouped detail model, e.g.:

```csharp
public class TransactionDateGroup : ObservableCollection<TransactionItem>
{
    public string DateLabel { get; }
}
```

- Show:
  - category name
  - current period label
  - total amount / count summary if useful
  - grouped transaction rows
- Order date groups newest to oldest.
- Order transactions inside each group newest to oldest.
- Add localized strings for page title, empty state, headers, and summary labels.

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs AccountingApp/Views/CategoryReportTransactionDetailPage.xaml AccountingApp/Resources/Strings/AppResources.resx AccountingApp/Resources/Strings/AppResources.zh-Hant.resx AccountingApp.Tests/CategoryReportTransactionDetailPageTests.cs AccountingApp.Tests/CategoryReportTransactionDetailViewModelTests.cs
git commit -m "feat: add grouped category transaction detail page"
```

### Task 4: Verify return flow and full regression coverage

**Files:**
- Modify: `AccountingApp.Tests/CategoryReportPageCopyTests.cs`
- Modify: `AccountingApp.Tests/CategoryReportTransactionDetailPageTests.cs`
- Modify any touched source files from Tasks 1-3 as needed

**Step 1: Add final regression checks**

Add assertions or behavior tests that verify:

- the selected range and anchor period are passed into the detail page
- returning from the detail page does not reset the category report range state
- empty-state behavior works when the selected category has no matching expense rows

**Step 2: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~CategoryReport|FullyQualifiedName~ExpenseCategoryReportTests"
```

Expected: PASS.

**Step 3: Run the full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 4: Commit**

```bash
git add AccountingApp.Tests/CategoryReportPageCopyTests.cs AccountingApp.Tests/CategoryReportTransactionDetailPageTests.cs AccountingApp.Tests/ExpenseCategoryReportTests.cs AccountingApp/Views/CategoryReportPage.xaml AccountingApp/ViewModels/CategoryReportViewModel.cs AccountingApp/Views/CategoryReportTransactionDetailPage.xaml AccountingApp/ViewModels/CategoryReportTransactionDetailViewModel.cs AccountingApp/Services/StatisticsService.cs AccountingApp/Resources/Strings/AppResources.resx AccountingApp/Resources/Strings/AppResources.zh-Hant.resx
git commit -m "test: verify category report drilldown flow"
```
