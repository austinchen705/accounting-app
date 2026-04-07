# Statistics Dynamic Axis Scale Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the fixed `50,000` Y-axis step on the Statistics page trend charts with a shared dynamic scaling rule that chooses readable steps for both small and large value ranges.

**Architecture:** Extract a shared Y-axis scale helper that computes a nice step from the chart's visible values, then use it when building both `TrendYAxes` and `CategoryTrendYAxes` in `StatisticsViewModel`. Keep the existing label formatter, but stop hard-coding `MinStep = 50_000`.

**Tech Stack:** .NET MAUI, C#, LiveChartsCore, xUnit.

---

### Task 1: Add failing tests for dynamic axis step selection

**Files:**
- Modify: `AccountingApp.Tests/StatisticsAxisStepTests.cs`
- Create: `AccountingApp.Tests/StatisticsAxisScaleHelperTests.cs`
- Test: `AccountingApp/ViewModels/StatisticsViewModel.cs`
- Test: new shared helper file under `AccountingApp/Services/` or `AccountingApp/Core/`

**Step 1: Write the failing test**

Replace the current hard-coded `50_000` assertion with tests that require a shared dynamic helper. Add assertions such as:

```csharp
Assert.Contains("StatisticsAxisScaleHelper", source);
Assert.Equal(2, CountOccurrences(source, "CreateYAxis("));
Assert.DoesNotContain("MinStep = 50_000", source);
```

Create `StatisticsAxisScaleHelperTests.cs` with representative low/high-range tests:

```csharp
[Theory]
[InlineData(new double[] { 1200, 4300 }, 1000)]
[InlineData(new double[] { 18000, 22000 }, 5000)]
[InlineData(new double[] { 52000, 91000 }, 20000)]
[InlineData(new double[] { 180000, 260000 }, 50000)]
public void CalculateStep_returns_readable_nice_steps(double[] values, double expectedStep)
{
    Assert.Equal(expectedStep, StatisticsAxisScaleHelper.CalculateStep(values));
}
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~StatisticsAxisStepTests|FullyQualifiedName~StatisticsAxisScaleHelperTests"
```

Expected: FAIL because the helper does not exist and `StatisticsViewModel` still hard-codes `50_000`.

**Step 3: Write minimal implementation**

Add a helper, for example:

```csharp
public static class StatisticsAxisScaleHelper
{
    public static double CalculateStep(IEnumerable<double> values) { ... }
}
```

Use a nice-number series such as:

```csharp
1_000, 2_000, 5_000, 10_000, 20_000, 50_000, 100_000, 200_000
```

with a target of about 5 visible Y-axis intervals.

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Tests/StatisticsAxisStepTests.cs AccountingApp.Tests/StatisticsAxisScaleHelperTests.cs AccountingApp/Services/StatisticsAxisScaleHelper.cs
git commit -m "feat: add dynamic statistics axis scale helper"
```

### Task 2: Apply the shared dynamic step to both Statistics trend charts

**Files:**
- Modify: `AccountingApp/ViewModels/StatisticsViewModel.cs`
- Test: `AccountingApp.Tests/StatisticsAxisStepTests.cs`

**Step 1: Write the failing integration assertion**

Update `StatisticsAxisStepTests.cs` so it verifies both Y-axis builders now use the helper instead of hard-coded steps, for example:

```csharp
Assert.Equal(2, CountOccurrences(source, "StatisticsAxisScaleHelper.CalculateStep("));
Assert.Contains("var trendAxisStep", source);
Assert.Contains("var categoryTrendAxisStep", source);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~StatisticsAxisStepTests"
```

Expected: FAIL because the view model still builds both axes with fixed `50_000`.

**Step 3: Write minimal implementation**

In `StatisticsViewModel.cs`:

- Gather visible numeric values before building `TrendYAxes`
- Gather visible numeric values before building `CategoryTrendYAxes`
- Compute each chart's step with the shared helper
- Keep:
  - `MinLimit = 0`
  - `ForceStepToMin = true`
  - `Labeler = value => FormatAxisValue(value)`

Example shape:

```csharp
var trendAxisStep = StatisticsAxisScaleHelper.CalculateStep(
    incomeValues.Concat(expenseValues));
```

and:

```csharp
MinStep = trendAxisStep,
```

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/StatisticsViewModel.cs AccountingApp.Tests/StatisticsAxisStepTests.cs
git commit -m "feat: apply dynamic y-axis scaling to statistics charts"
```

### Task 3: Validate low-range and high-range chart behavior

**Files:**
- Modify: `AccountingApp.Tests/StatisticsAxisScaleHelperTests.cs`
- Modify: `AccountingApp.Tests/StatisticsPageTrendCopyTests.cs` if needed
- Modify any touched source files from Tasks 1-2 as needed

**Step 1: Add final regression checks**

Add focused tests covering:

- values below `50k` produce fine-grained steps
- values above `50k` still produce larger readable steps
- both Statistics charts continue using the same scaling strategy

If useful, add source-level assertions that both charts still keep:

```csharp
Assert.Contains("ForceStepToMin = true", source);
Assert.Contains("FormatAxisValue", source);
```

**Step 2: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~StatisticsAxis|FullyQualifiedName~StatisticsPageTrendCopyTests"
```

Expected: PASS.

**Step 3: Run full suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 4: Commit**

```bash
git add AccountingApp.Tests/StatisticsAxisScaleHelperTests.cs AccountingApp.Tests/StatisticsAxisStepTests.cs AccountingApp.Tests/StatisticsPageTrendCopyTests.cs AccountingApp/ViewModels/StatisticsViewModel.cs AccountingApp/Services/StatisticsAxisScaleHelper.cs
git commit -m "test: verify dynamic statistics axis scaling"
```
