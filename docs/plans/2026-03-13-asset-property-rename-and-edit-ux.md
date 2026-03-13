# Asset Property Rename And Edit UX Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rename the asset bucket `Fund3` to `Property` end-to-end and make asset snapshot editing obvious by auto-focusing the top edit form.

**Architecture:** Keep the existing fixed-field `AssetSnapshot` design, but rename the fourth bucket consistently across model, CSV parser, trend transformation, ViewModel, and UI. Preserve the shared top-form edit flow and improve it with explicit edit-mode labels plus auto-scroll to the form.

**Tech Stack:** .NET MAUI, SQLite, existing MVVM asset trend flow, xUnit.

---

### Task 1: Rename Core Asset Field And Trend Output

**Files:**
- Modify: `AccountingApp.Core/Models/AssetSnapshot.cs`
- Modify: `AccountingApp.Core/Services/AssetTrendSeries.cs`
- Test: `AccountingApp.Tests/AssetTrendSeriesTests.cs`

**Step 1: Write the failing test**

Update or add tests asserting the renamed property is used:

```csharp
[Fact]
public void AssetSnapshot_exposes_property_and_total_helper()
{
    var snapshot = new AssetSnapshot
    {
        Stock = 10,
        Cash = 20,
        FirstTrade = 30,
        Property = 40
    };

    Assert.Equal(40m, snapshot.Property);
    Assert.Equal(100m, snapshot.Total);
}
```

Also update the trend test to expect `PropertyValues`.

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendSeriesTests"`
Expected: FAIL because `Property` / `PropertyValues` do not exist yet.

**Step 3: Write minimal implementation**

- Rename `Fund3` to `Property` in `AssetSnapshot`
- Rename `Fund3Values` to `PropertyValues` in trend output
- Update total calculation to use `Property`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Core/Models/AssetSnapshot.cs AccountingApp.Core/Services/AssetTrendSeries.cs AccountingApp.Tests/AssetTrendSeriesTests.cs
git commit -m "refactor: rename fund3 asset field to property"
```

### Task 2: Rename CSV Import Schema To Property

**Files:**
- Modify: `AccountingApp.Core/Services/AssetSnapshotCsvImporter.cs`
- Test: `AccountingApp.Tests/AssetSnapshotCsvImporterTests.cs`

**Step 1: Write the failing test**

Update the importer test to use the new header:

```csharp
[Fact]
public void Import_parses_property_column_name()
{
    var csv = "Date,Stock,Cash,FirstTrade,Property\n2026-01-01,1,2,3,4";

    var result = AssetSnapshotCsvImporter.Parse(csv);

    Assert.Single(result.Snapshots);
    Assert.Equal(4m, result.Snapshots[0].Property);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetSnapshotCsvImporterTests"`
Expected: FAIL because the parser still assumes `Fund3`.

**Step 3: Write minimal implementation**

- Update parsing and model assignment to use `Property`
- Keep row-level error reporting intact

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Core/Services/AssetSnapshotCsvImporter.cs AccountingApp.Tests/AssetSnapshotCsvImporterTests.cs
git commit -m "refactor: rename asset csv property column"
```

### Task 3: Rename Asset Trend UI Labels To Property(房產)

**Files:**
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Modify: `AccountingApp/Views/AssetTrendPage.xaml`
- Test: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`

**Step 1: Write the failing tests**

Update contract/layout tests to require the new text:

```csharp
Assert.Contains("Property(房產)", vmCode);
Assert.Contains("Property(房產)", pageXaml);
Assert.DoesNotContain("Fund3", pageXaml);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendPageLayoutTests"`
Expected: FAIL because UI and chart names still use `Fund3`.

**Step 3: Write minimal implementation**

- Update chart series name to `Property(房產)`
- Update form/history labels to `Property(房產)`
- Remove remaining `Fund3` display strings from asset trend UI

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp/Views/AssetTrendPage.xaml AccountingApp.Tests/AssetTrendViewModelContractTests.cs AccountingApp.Tests/AssetTrendPageLayoutTests.cs
git commit -m "feat: rename asset trend ui field to property"
```

### Task 4: Improve Edit UX By Scrolling To The Form

**Files:**
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Modify: `AccountingApp/Views/AssetTrendPage.xaml`
- Modify: `AccountingApp/Views/AssetTrendPage.xaml.cs`
- Test: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`

**Step 1: Write the failing tests**

Add contract/layout checks for explicit edit UX:

```csharp
Assert.Contains("EditingSnapshotDisplayText", vmCode);
Assert.Contains("編輯資產快照", pageXaml);
Assert.Contains("ScrollToAsync", codeBehind);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendPageLayoutTests"`
Expected: FAIL because the page does not yet expose the clearer edit UX.

**Step 3: Write minimal implementation**

- Add an edit-mode display text to the ViewModel
- Raise a lightweight signal/event when edit begins
- In `AssetTrendPage.xaml.cs`, respond by scrolling the page to the top form
- Update the form header to switch between add/edit wording

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp/Views/AssetTrendPage.xaml AccountingApp/Views/AssetTrendPage.xaml.cs AccountingApp.Tests/AssetTrendViewModelContractTests.cs AccountingApp.Tests/AssetTrendPageLayoutTests.cs
git commit -m "feat: improve asset snapshot edit ux"
```

### Task 5: Final Verification

**Files:**
- Modify: `openspec/changes/add-personal-asset-trend/tasks.md`

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetSnapshot|FullyQualifiedName~AssetTrend"
```

Expected: PASS.

**Step 2: Run full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 3: Manual QA checklist**

- local CSV with `Property` header imports successfully
- edit loads selected snapshot and scrolls to the top form
- edit mode clearly shows which snapshot is being edited
- updated snapshot persists correctly
- asset trend page shows `Property(房產)` consistently

**Step 4: Update OpenSpec tasks if needed**

Record any status changes in:

- `openspec/changes/add-personal-asset-trend/tasks.md`

**Step 5: Commit**

```bash
git add openspec/changes/add-personal-asset-trend/tasks.md
git commit -m "chore: verify property rename and edit ux update"
```
