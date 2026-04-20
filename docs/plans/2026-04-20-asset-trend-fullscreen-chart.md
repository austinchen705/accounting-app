# Asset Trend Fullscreen Chart Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a dedicated full-screen asset trend chart page with denser Y-axis ticks and full-value labels while keeping the existing asset trend page as a compact summary view.

**Architecture:** Reuse the existing asset snapshot-backed trend data and split chart presentation into two modes: summary on `AssetTrendPage` and detail on a new full-screen chart page. Keep the full-screen page chart-only, and centralize axis/label configuration in `AssetTrendViewModel` so both views stay consistent without duplicating chart data logic.

**Tech Stack:** .NET MAUI, C#, XAML, LiveChartsCore, xUnit, OpenSpec

---

### Task 1: Lock the new navigation surface with failing layout tests

**Files:**
- Modify: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`
- Create: `AccountingApp.Tests/AssetTrendChartPageLayoutTests.cs`
- Test: `AccountingApp/Views/AssetTrendPage.xaml`
- Test: `AccountingApp/AppShell.xaml.cs`

**Step 1: Write the failing tests**

Add layout assertions that:
- `AssetTrendPage.xaml` contains a full-screen / expand affordance near the chart section
- `AppShell.xaml.cs` registers a new `AssetTrendChartPage` route
- the new `AssetTrendChartPage.xaml` exists and contains a `CartesianChart`

Use file-content contract tests like the existing page layout tests.

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPageLayoutTests|FullyQualifiedName~AssetTrendChartPageLayoutTests"
```

Expected: FAIL because the route, page, and expand affordance do not exist yet.

**Step 3: Write minimal implementation**

Create:
- `AccountingApp/Views/AssetTrendChartPage.xaml`
- `AccountingApp/Views/AssetTrendChartPage.xaml.cs`

Modify:
- `AccountingApp/Views/AssetTrendPage.xaml`
- `AccountingApp/AppShell.xaml.cs`
- `AccountingApp/MauiProgram.cs`

Add:
- a chart expansion affordance in the summary chart card
- a chart-only full-screen page
- route registration and DI registration for the new page

**Step 4: Run test to verify it passes**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPageLayoutTests|FullyQualifiedName~AssetTrendChartPageLayoutTests"
```

Expected: PASS

**Step 5: Commit**

```bash
git add AccountingApp/Views/AssetTrendPage.xaml AccountingApp/Views/AssetTrendChartPage.xaml AccountingApp/Views/AssetTrendChartPage.xaml.cs AccountingApp/AppShell.xaml.cs AccountingApp/MauiProgram.cs AccountingApp.Tests/AssetTrendPageLayoutTests.cs AccountingApp.Tests/AssetTrendChartPageLayoutTests.cs
git commit -m "feat: add fullscreen asset trend chart page"
```

### Task 2: Define chart mode contracts before changing chart logic

**Files:**
- Modify: `AccountingApp.Tests/AssetTrendViewModelContractTests.cs`
- Create: `AccountingApp.Tests/AssetTrendChartAxisFormattingTests.cs`
- Test: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Test: `AccountingApp.Core/Services/StatisticsAxisScaleHelper.cs`

**Step 1: Write the failing tests**

Add tests that assert the source now exposes:
- a command or method for opening the full-screen chart
- separate summary/detail chart configuration surface
- detail-mode full-value label formatting
- denser tick calculation path for detail mode than summary mode

Prefer one pure behavior test file for axis formatting if you can place any reusable helper in `AccountingApp.Core`; otherwise use source contract tests for the view model plus helper-level tests for numeric step behavior.

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendChartAxisFormattingTests|FullyQualifiedName~StatisticsAxisScaleHelperTests"
```

Expected: FAIL because the view model currently has one hard-coded chart mode with `MinStep = 2_500_000` and compact label formatting only.

**Step 3: Write minimal implementation**

Refactor `AccountingApp/ViewModels/AssetTrendViewModel.cs` to:
- centralize chart-axis creation
- support summary and detail chart modes
- keep one dataset / series generation path
- use compact labels on the summary chart
- use full amount labels and denser Y-axis ticks on the detail chart

If needed, add a small reusable helper in `AccountingApp.Core/Services/` for asset-trend axis formatting so focused tests can validate behavior without MAUI page dependencies.

**Step 4: Run test to verify it passes**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendViewModelContractTests|FullyQualifiedName~AssetTrendChartAxisFormattingTests|FullyQualifiedName~StatisticsAxisScaleHelperTests"
```

Expected: PASS

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp.Core/Services/*.cs AccountingApp.Tests/AssetTrendViewModelContractTests.cs AccountingApp.Tests/AssetTrendChartAxisFormattingTests.cs AccountingApp.Tests/StatisticsAxisScaleHelperTests.cs
git commit -m "feat: add detailed asset trend chart axis mode"
```

### Task 3: Wire page interaction to the detailed chart mode

**Files:**
- Modify: `AccountingApp/Views/AssetTrendPage.xaml.cs`
- Modify: `AccountingApp/Views/AssetTrendChartPage.xaml`
- Modify: `AccountingApp/Views/AssetTrendChartPage.xaml.cs`
- Modify: `AccountingApp/ViewModels/AssetTrendViewModel.cs`
- Test: `AccountingApp.Tests/AssetTrendPageLayoutTests.cs`

**Step 1: Write the failing test**

Extend the page/source contract tests so they assert:
- the summary page affordance is wired to navigation
- the detailed page binds to the detail chart axes / series surface instead of reusing the summary bindings directly by accident

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPageLayoutTests|FullyQualifiedName~AssetTrendChartPageLayoutTests|FullyQualifiedName~AssetTrendViewModelContractTests"
```

Expected: FAIL until the page bindings and navigation flow are fully wired.

**Step 3: Write minimal implementation**

Implement the interaction so:
- tapping the summary chart affordance navigates to `AssetTrendChartPage`
- the chart page triggers a load/refresh path if needed
- the detailed chart binds to the detail mode axis configuration
- no snapshot form or history content is duplicated into the full-screen page

**Step 4: Run test to verify it passes**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrendPageLayoutTests|FullyQualifiedName~AssetTrendChartPageLayoutTests|FullyQualifiedName~AssetTrendViewModelContractTests"
```

Expected: PASS

**Step 5: Commit**

```bash
git add AccountingApp/Views/AssetTrendPage.xaml.cs AccountingApp/Views/AssetTrendChartPage.xaml AccountingApp/Views/AssetTrendChartPage.xaml.cs AccountingApp/ViewModels/AssetTrendViewModel.cs AccountingApp.Tests/AssetTrendPageLayoutTests.cs AccountingApp.Tests/AssetTrendChartPageLayoutTests.cs AccountingApp.Tests/AssetTrendViewModelContractTests.cs
git commit -m "feat: wire asset trend fullscreen navigation"
```

### Task 4: Verify the complete asset trend flow

**Files:**
- Modify: `openspec/changes/asset-trend-fullscreen-chart/tasks.md`

**Step 1: Run focused tests**

Run:
```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AssetTrend|FullyQualifiedName~StatisticsAxisScaleHelperTests"
```

Expected: PASS

**Step 2: Run targeted build**

Run:
```bash
dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64 -p:_DeviceName="iPhone 15 Pro"
```

Expected: PASS

**Step 3: Mark OpenSpec tasks complete**

Update:
- `openspec/changes/asset-trend-fullscreen-chart/tasks.md`

Mark completed checkboxes after verification.

**Step 4: Commit**

```bash
git add openspec/changes/asset-trend-fullscreen-chart/tasks.md
git commit -m "test: verify fullscreen asset trend chart flow"
```
