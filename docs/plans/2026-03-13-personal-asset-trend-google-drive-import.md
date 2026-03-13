# Personal Asset Trend Google Drive Import Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a Google Drive based asset snapshot import flow that lists `.csv` files from the configured backup folder, lets the user pick one, and replaces existing `AssetSnapshot` data with the selected file.

**Architecture:** Reuse `GoogleDriveService` for authorization, folder access, listing, and download. Keep replace-import behavior in `AssetSnapshotService`, then let `AssetTrendViewModel` coordinate selection, confirmation, import, and UI refresh.

**Tech Stack:** .NET MAUI, SQLite (`sqlite-net-pcl`), existing Google Drive API integration, xUnit.

---

### Task 1: Add Google Drive CSV Listing Support

**Files:**
- Modify: `AccountingApp/Services/GoogleDriveService.cs`
- Test: `AccountingApp.Tests/GoogleDriveServiceTests.cs`

**Step 1: Write the failing test**

Add a test proving the service can filter backup-folder files down to `.csv` candidates, for example with a small extracted helper or service-level method result assertion.

```csharp
[Fact]
public void AssetImport_candidates_include_only_csv_files()
{
    var files = new[]
    {
        new GoogleDriveFileItem("1", "asset-a.csv"),
        new GoogleDriveFileItem("2", "notes.txt"),
        new GoogleDriveFileItem("3", "asset-b.CSV")
    };

    var result = GoogleDriveService.FilterCsvFiles(files);

    Assert.Equal(new[] { "asset-a.csv", "asset-b.CSV" }, result.Select(x => x.Name));
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~GoogleDriveServiceTests"`
Expected: FAIL because the CSV file model/helper does not exist.

**Step 3: Write minimal implementation**

- Add a small file item model, for example:

```csharp
public record GoogleDriveFileItem(string Id, string Name);
```

- Add a helper or public method that returns only `.csv` files:

```csharp
public static IReadOnlyList<GoogleDriveFileItem> FilterCsvFiles(IEnumerable<GoogleDriveFileItem> files) =>
    files.Where(file => file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).ToList();
```

- Add a Drive API method that lists files for the configured backup folder and maps them into `GoogleDriveFileItem`.

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Services/GoogleDriveService.cs AccountingApp.Tests/GoogleDriveServiceTests.cs
git commit -m "feat: add google drive csv listing support for asset import"
```

### Task 2: Add Replace-Import Flow for Asset Snapshots

**Files:**
- Modify: `AccountingApp/Services/AssetSnapshotService.cs`
- Test: `AccountingApp.Tests/AssetSnapshotServiceTests.cs`

**Step 1: Write the failing test**

Add a test proving the replace-import flow clears previous snapshots before inserting imported rows.

```csharp
[Fact]
public async Task ReplaceImportCsvAsync_clears_existing_snapshots_before_import()
{
    await using var db = await TestDb.CreateAsync();
    var svc = new AssetSnapshotService(db.Service);

    await svc.AddAsync(new AssetSnapshot { Date = new DateTime(2026, 1, 1), Stock = 1, Cash = 1, FirstTrade = 1, Fund3 = 1 });

    await svc.ReplaceImportCsvAsync("Date,Stock,Cash,FirstTrade,Fund3\n2026-02-01,10,20,30,40");

    var all = await svc.GetAllAsync();
    Assert.Single(all);
    Assert.Equal(new DateTime(2026, 2, 1), all[0].Date);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetSnapshotServiceTests"`
Expected: FAIL because replace-import does not exist.

**Step 3: Write minimal implementation**

- Add delete-all support for asset snapshots if needed.
- Add a replace-import method:

```csharp
public async Task<AssetSnapshotImportResult> ReplaceImportCsvAsync(string csvContent)
{
    await EnsureInitializedAsync();
    await _coreDatabaseService.Db.DeleteAllAsync<AssetSnapshot>();
    var result = AssetSnapshotCsvImporter.Parse(csvContent);
    foreach (var snapshot in result.Snapshots)
        await _coreService.AddAsync(snapshot);
    return result;
}
```

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Services/AssetSnapshotService.cs AccountingApp.Tests/AssetSnapshotServiceTests.cs
git commit -m "feat: add replace import flow for asset snapshots"
```

### Task 3: Wire Google Drive Import Into Asset Trend UI

**Files:**
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Modify: `AccountingApp/Views/AssetTrendPage.xaml`
- Modify: `AccountingApp/MauiProgram.cs`
- Test: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`

**Step 1: Write the failing tests**

Add contract/layout checks for the new command and binding:

```csharp
Assert.Contains("ImportFromGoogleDriveCommand", vmCode);
Assert.Contains("ImportFromGoogleDriveCommand", pageXaml);
Assert.Contains("DisplayActionSheet", vmCode);
Assert.Contains("DisplayAlert", vmCode);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendPageLayoutTests"`
Expected: FAIL because the Google Drive import command is not present.

**Step 3: Write minimal implementation**

- Inject `GoogleDriveService` into `AssetTrendViewModel`
- Add `ImportFromGoogleDriveCommand`
- Implement the flow:

```csharp
private async Task ImportFromGoogleDriveAsync()
{
    var files = await _googleDriveService.ListBackupFolderFilesAsync();
    var csvFiles = GoogleDriveService.FilterCsvFiles(files);
    if (csvFiles.Count == 0)
    {
        await Application.Current!.Windows[0].Page!.DisplayAlert("提示", "Google Drive 備份資料夾中沒有 CSV 檔案。", "確定");
        return;
    }

    var fileName = await Application.Current!.Windows[0].Page!
        .DisplayActionSheet("選擇要匯入的 CSV", "取消", null, csvFiles.Select(x => x.Name).ToArray());
    if (string.IsNullOrWhiteSpace(fileName) || fileName == "取消") return;

    var confirm = await Application.Current!.Windows[0].Page!
        .DisplayAlert("確認匯入", "這會清空目前所有資產快照並改用所選 CSV 重新建立。", "匯入", "取消");
    if (!confirm) return;

    var selected = csvFiles.Single(x => x.Name == fileName);
    await using var stream = await _googleDriveService.DownloadFileAsync(selected.Id);
    using var reader = new StreamReader(stream);
    var csv = await reader.ReadToEndAsync();
    var result = await _assetSnapshotService.ReplaceImportCsvAsync(csv);
    // update ImportedCount, SkippedCount, ImportErrorDetails and reload
}
```

- Add a new page button bound to `ImportFromGoogleDriveCommand`

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp/Views/AssetTrendPage.xaml AccountingApp/MauiProgram.cs AccountingApp.Tests/AssetTrendViewModelContractTests.cs AccountingApp.Tests/AssetTrendPageLayoutTests.cs
git commit -m "feat: add google drive asset snapshot import flow"
```

### Task 4: Final Verification

**Files:**
- Modify: `openspec/changes/add-personal-asset-trend/tasks.md`

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetSnapshot|FullyQualifiedName~AssetTrend|FullyQualifiedName~GoogleDriveServiceTests"
```

Expected: PASS.

**Step 2: Run full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 3: Manual QA checklist**

- Google Drive folder already configured -> CSV list appears
- No CSV in folder -> friendly alert appears
- Select CSV and cancel confirmation -> no data changes
- Confirm import -> old asset snapshots are cleared and replaced
- Invalid rows -> skipped count and error details shown

**Step 4: Update OpenSpec tasks**

Mark the related checkbox items in:

- `openspec/changes/add-personal-asset-trend/tasks.md`

**Step 5: Commit**

```bash
git add openspec/changes/add-personal-asset-trend/tasks.md
git commit -m "chore: verify google drive asset import flow"
```
