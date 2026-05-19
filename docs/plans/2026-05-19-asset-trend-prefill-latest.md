# Asset Trend Prefill Latest Snapshot Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a create-form shortcut on the asset trend page that copies the latest snapshot values into a new draft while keeping the date set to today.

**Architecture:** Reuse the existing shared asset snapshot form and keep the prefill logic inside `AssetTrendViewModel`. The page only needs new button wiring and visibility bindings; persistence remains unchanged because the copied data is still saved through the existing add flow.

**Tech Stack:** .NET MAUI, existing MVVM asset trend flow, xUnit.

---

### Task 1: Expose Prefill Command Surface

**Files:**
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Test: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`

**Step 1: Write the failing test**

Add contract assertions for:

```csharp
Assert.Contains("PrefillLatestSnapshotCommand", vmCode);
Assert.Contains("CanPrefillLatestSnapshot", vmCode);
Assert.Contains("PrefillLatestSnapshot", vmCode);
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests"`
Expected: FAIL because the ViewModel does not yet expose the prefill surface.

**Step 3: Write minimal implementation**

- add a prefill command
- add a create-mode visibility/enabled property
- notify bindings when create/edit state or snapshot data changes

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

### Task 2: Add Behavior Test For Copying Latest Values

**Files:**
- Create: `AccountingApp.Tests/AssetTrendPrefillContractTests.cs`
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`

**Step 1: Write the failing test**

Add a contract test that asserts the ViewModel source:

- defines `PrefillLatestSnapshot`
- copies `Stock`, `Cash`, `FirstTrade`, and `Property` from the latest snapshot
- does not assign `SnapshotDate` from the copied snapshot

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPrefillContractTests"`
Expected: FAIL because the command behavior does not exist yet.

**Step 3: Write minimal implementation**

- find the latest snapshot from loaded data
- copy only amount fields
- keep the current date unchanged
- clear stale form errors

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

### Task 3: Wire The Button Into The Page

**Files:**
- Modify: `AccountingApp/Views/AssetTrendPage.xaml`
- Modify: `AccountingApp/Resources/Strings/AppResources.resx`
- Modify: `AccountingApp/Resources/Strings/AppResources.zh-Hant.resx`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs` if present, otherwise extend the existing contract-style coverage

**Step 1: Write the failing test**

Add assertions for:

- new localized resource key for the button text
- button binding to `PrefillLatestSnapshotCommand`
- visibility or enabled binding using `CanPrefillLatestSnapshot`

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrend"`
Expected: FAIL because the page does not yet include the new action.

**Step 3: Write minimal implementation**

- add the button near the create-form actions
- bind text through localization
- show it only when create mode has prior snapshots available

**Step 4: Run test to verify it passes**

Run the same command as Step 2.
Expected: PASS.

### Task 4: Final Verification

**Files:**
- Modify: none unless a small cleanup is needed

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrend"
```

Expected: PASS.

**Step 2: Run full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 3: Manual QA checklist**

- create mode with history shows the prefill button
- tapping the button copies latest amounts into the form
- tapping the button keeps the current date as today
- edit mode does not show the prefill action
- empty history does not show the prefill action
