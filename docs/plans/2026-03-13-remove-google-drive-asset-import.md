# Remove Google Drive Asset Import Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove the Google Drive asset snapshot import path from the asset trend feature while preserving local CSV import and existing Google Drive backup/restore.

**Architecture:** Limit the rollback to asset trend UI and view model code. Leave `GoogleDriveService` available for backup/restore, but remove asset-trend-specific command wiring, debug output, and related contract assertions.

**Tech Stack:** .NET MAUI, xUnit, existing asset trend MVVM structure.

---

### Task 1: Remove Google Drive Import From Asset Trend ViewModel

**Files:**
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Test: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`

**Step 1: Write the failing test**

Update the contract test so it asserts the rollback target:

```csharp
Assert.DoesNotContain("ImportFromGoogleDriveCommand", vmCode);
Assert.DoesNotContain("Drive debug", vmCode);
Assert.DoesNotContain("ListBackupFolderFilesAsync", vmCode);
Assert.Contains("ImportCsvCommand", vmCode);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests"`
Expected: FAIL because the Google Drive import command and debug flow still exist.

**Step 3: Write minimal implementation**

- Remove `GoogleDriveService` dependency injection from `AssetTrendViewModel`
- Remove `ImportFromGoogleDriveCommand`
- Remove `ImportFromGoogleDriveAsync()`
- Remove temporary Google Drive debug alert logic
- Keep local `ImportCsvCommand`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp.Tests/AssetTrendViewModelContractTests.cs
git commit -m "refactor: remove google drive asset import viewmodel flow"
```

### Task 2: Remove Google Drive Import Button From Asset Trend Page

**Files:**
- Modify: `AccountingApp/Views/AssetTrendPage.xaml`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`

**Step 1: Write the failing test**

Update the page layout test:

```csharp
Assert.DoesNotContain("ImportFromGoogleDriveCommand", pageXaml);
Assert.Contains("ImportCsvCommand", pageXaml);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPageLayoutTests"`
Expected: FAIL because the page still includes the Google Drive import button.

**Step 3: Write minimal implementation**

- Remove the Google Drive import button binding from `AssetTrendPage.xaml`
- Keep the local CSV import button and summary UI unchanged

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/AssetTrendPage.xaml AccountingApp.Tests/AssetTrendPageLayoutTests.cs
git commit -m "refactor: remove google drive asset import button"
```

### Task 3: Remove Asset-Import-Specific Google Drive Test Expectations

**Files:**
- Modify: `AccountingApp.Tests/GoogleDriveServiceTests.cs`

**Step 1: Write the failing test**

Replace the current asset-import-specific expectation with a minimal check that still matches the remaining backup/restore-facing Google Drive surface, for example:

```csharp
Assert.Contains("GoogleDriveFileItem", source);
Assert.Contains("ListBackupFolderFilesAsync", source);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~GoogleDriveServiceTests"`
Expected: FAIL if the test still asserts removed asset-import-specific details.

**Step 3: Write minimal implementation**

- Update `GoogleDriveServiceTests.cs` so it no longer encodes the removed asset-import behavior as a requirement
- Do not remove backup/restore support from `GoogleDriveService`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Tests/GoogleDriveServiceTests.cs
git commit -m "test: align google drive service contract with backup-only usage"
```

### Task 4: Final Verification

**Files:**
- Optional: `openspec/changes/add-personal-asset-trend/tasks.md`

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendPageLayoutTests|FullyQualifiedName~GoogleDriveServiceTests"
```

Expected: PASS.

**Step 2: Run full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 3: Manual QA checklist**

- asset trend page shows only local CSV import
- local CSV import still works
- no Google Drive asset import prompt appears
- settings page Google Drive backup/restore still reachable

**Step 4: Commit**

```bash
git commit --allow-empty -m "chore: verify local-only asset import flow"
```
