## Why

The current asset trend chart is useful for quick pattern recognition, but it is too compact for reading exact historical amounts. Users need a focused, full-screen view with denser Y-axis ticks so earlier data points can be read precisely without sacrificing the concise overview on the main page.

## What Changes

- Add a dedicated full-screen asset trend chart page reachable from the existing asset trend chart section.
- Keep the existing asset trend page chart as a summary view.
- Show denser Y-axis ticks and full amount labels on the full-screen chart page.
- Preserve the existing asset trend data source so both views reflect the same snapshot history.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `personal-asset-trend`: extend the chart viewing experience to support full-screen inspection and a detailed Y-axis labeling mode.

## Impact

- Affected code:
  - `AccountingApp/Views/AssetTrendPage.xaml`
  - `AccountingApp/Views/AssetTrendPage.xaml.cs`
  - `AccountingApp/ViewModels/AssetTrendViewModel.cs`
  - `AccountingApp/AppShell.xaml.cs`
  - `AccountingApp/MauiProgram.cs`
  - new full-screen chart page and view model files
- Affected tests:
  - asset trend page layout tests
  - asset trend view model contract tests
  - axis scale / label formatting tests
- No external API or package changes expected.
