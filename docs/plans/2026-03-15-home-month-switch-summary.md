# Home Month Switch Summary Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Let users switch month on Home page and have both monthly totals and recent transactions update to the selected month.

**Architecture:** Keep `HomeViewModel.CurrentMonth` as `yyyy-MM` (existing query contract), add previous/next month commands, and reload Home data from selected month. Update `HomePage.xaml` with month navigation controls and selected-month copy. Because `AccountingApp.Tests` does not reference MAUI UI assembly, use focused layout/source contract tests (file-content tests) plus iOS build verification.

**Tech Stack:** .NET MAUI (XAML + C#), xUnit, existing `TransactionService` month queries

---

### Task 1: Add failing Home layout tests for month navigation UI

**Files:**
- Create: `AccountingApp.Tests/HomePageLayoutTests.cs`
- Test: `AccountingApp/Views/HomePage.xaml`

**Step 1: Write the failing test**

```csharp
namespace AccountingApp.Tests;

public class HomePageLayoutTests
{
    [Fact]
    public void HomePage_has_month_navigation_controls()
    {
        var xaml = ReadHomeXaml();

        Assert.Contains("Command=\"{Binding PreviousMonthCommand}\"", xaml);
        Assert.Contains("Text=\"{Binding CurrentMonthLabel}\"", xaml);
        Assert.Contains("Command=\"{Binding NextMonthCommand}\"", xaml);
    }

    [Fact]
    public void HomePage_uses_selected_month_copy_instead_of_fixed_this_month()
    {
        var xaml = ReadHomeXaml();

        Assert.DoesNotContain("Text=\"本月總覽\"", xaml);
        Assert.Contains("Text=\"{Binding SummaryTitle}\"", xaml);
        Assert.Contains("Text=\"該月份尚無收支資料，點右下角新增第一筆。\"", xaml);
    }

    private static string ReadHomeXaml()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/Views/HomePage.xaml"));

        return File.ReadAllText(path);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~HomePageLayoutTests"`
Expected: FAIL because Home page does not yet contain month nav bindings/copy.

**Step 3: Commit**

```bash
git add AccountingApp.Tests/HomePageLayoutTests.cs
git commit -m "test: add failing home month navigation layout tests"
```

### Task 2: Add failing Home ViewModel source contract tests

**Files:**
- Create: `AccountingApp.Tests/HomeViewModelMonthContractTests.cs`
- Test: `AccountingApp/ViewModels/HomeViewModel.cs`

**Step 1: Write the failing test**

```csharp
namespace AccountingApp.Tests;

public class HomeViewModelMonthContractTests
{
    [Fact]
    public void HomeViewModel_defines_month_navigation_commands_and_label()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("public ICommand PreviousMonthCommand", source);
        Assert.Contains("public ICommand NextMonthCommand", source);
        Assert.Contains("public string CurrentMonthLabel", source);
    }

    [Fact]
    public void HomeViewModel_loads_recent_transactions_from_selected_month()
    {
        var source = ReadHomeViewModel();

        Assert.Contains("GetMonthSummaryAsync(_currentMonth)", source);
        Assert.Contains("GetByMonthAsync(_currentMonth)", source);
    }

    private static string ReadHomeViewModel()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../AccountingApp/ViewModels/HomeViewModel.cs"));

        return File.ReadAllText(path);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~HomeViewModelMonthContractTests"`
Expected: FAIL because command/label members and month-scoped recent query are missing.

**Step 3: Commit**

```bash
git add AccountingApp.Tests/HomeViewModelMonthContractTests.cs
git commit -m "test: add failing home month contract tests"
```

### Task 3: Implement Home month navigation in ViewModel (minimal)

**Files:**
- Modify: `AccountingApp/ViewModels/HomeViewModel.cs`
- Test: `AccountingApp.Tests/HomeViewModelMonthContractTests.cs`

**Step 1: Write minimal implementation**

```csharp
public ICommand PreviousMonthCommand { get; }
public ICommand NextMonthCommand { get; }

public string CurrentMonthLabel => DateTime.ParseExact(_currentMonth + "-01", "yyyy-MM-dd", null)
    .ToString("yyyy/MM");

public string SummaryTitle => $"{CurrentMonthLabel} 總覽";
```

```csharp
PreviousMonthCommand = new Command(async () =>
{
    var dt = DateTime.ParseExact(_currentMonth + "-01", "yyyy-MM-dd", null).AddMonths(-1);
    CurrentMonth = dt.ToString("yyyy-MM");
    await LoadAsync();
});

NextMonthCommand = new Command(async () =>
{
    var dt = DateTime.ParseExact(_currentMonth + "-01", "yyyy-MM-dd", null).AddMonths(1);
    CurrentMonth = dt.ToString("yyyy-MM");
    await LoadAsync();
});
```

```csharp
var recent = await _transactionService.GetByMonthAsync(_currentMonth);
RecentTransactions.Clear();
foreach (var t in recent.Take(10)) RecentTransactions.Add(t);
```

Also ensure `CurrentMonth` setter calls:
```csharp
OnPropertyChanged();
OnPropertyChanged(nameof(CurrentMonthLabel));
OnPropertyChanged(nameof(SummaryTitle));
```

**Step 2: Run test to verify it passes**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~HomeViewModelMonthContractTests"`
Expected: PASS.

**Step 3: Commit**

```bash
git add AccountingApp/ViewModels/HomeViewModel.cs AccountingApp.Tests/HomeViewModelMonthContractTests.cs
git commit -m "feat: add home month navigation commands and month-scoped loading"
```

### Task 4: Implement Home page month switching UI

**Files:**
- Modify: `AccountingApp/Views/HomePage.xaml`
- Test: `AccountingApp.Tests/HomePageLayoutTests.cs`

**Step 1: Minimal XAML update**

Add a month navigation row in summary card:

```xml
<Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="8">
    <Button Text="‹" Command="{Binding PreviousMonthCommand}" />
    <Label Grid.Column="1" Text="{Binding CurrentMonthLabel}" HorizontalTextAlignment="Center" />
    <Button Grid.Column="2" Text="›" Command="{Binding NextMonthCommand}" />
</Grid>
<Label Text="{Binding SummaryTitle}" Style="{StaticResource SectionTitleStyle}" />
```

Replace empty copy:

```xml
<Label Text="該月份尚無收支資料，點右下角新增第一筆。" ... />
```

**Step 2: Run test to verify it passes**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~HomePageLayoutTests"`
Expected: PASS.

**Step 3: Commit**

```bash
git add AccountingApp/Views/HomePage.xaml AccountingApp.Tests/HomePageLayoutTests.cs
git commit -m "feat: add home month switch controls and selected-month copy"
```

### Task 5: Final verification and integration check

**Files:**
- Verify touched files only

**Step 1: Run focused regression tests**

Run:
- `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~HomePageLayoutTests|FullyQualifiedName~HomeViewModelMonthContractTests"`

Expected: PASS.

**Step 2: Run full test suite**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj`
Expected: PASS (no regressions).

**Step 3: Run iOS build check**

Run: `dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64`
Expected: Build succeeded.

**Step 4: Manual behavior check**

- Open Home page
- Tap previous month / next month
- Verify title, totals, and recent list all change with selected month
- Verify month with no data shows empty-state copy

**Step 5: Final commit**

```bash
git add AccountingApp/ViewModels/HomeViewModel.cs AccountingApp/Views/HomePage.xaml AccountingApp.Tests/HomePageLayoutTests.cs AccountingApp.Tests/HomeViewModelMonthContractTests.cs
git commit -m "feat: support month switching on home summary"
```
