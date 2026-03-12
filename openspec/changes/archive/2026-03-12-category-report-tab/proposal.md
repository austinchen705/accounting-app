## Why

The app currently offers a general statistics tab, but it does not provide a dedicated category-focused expense report experience. Users need a separate report screen that makes category composition and ranking easier to understand across week, month, year, and all-time ranges.

## What Changes

- Add a new sixth bottom tab named `分類報告` without changing the existing `統計` tab.
- Add a dedicated expense category report page with a layout centered on a donut chart summary and a ranked category list.
- Add fixed range filters for `week`, `month`, `year`, and `all`, with period navigation for week/month/year and a full-history mode for all.
- Aggregate only expense transactions, convert them into the configured base currency, and show totals, counts, and percentage share per category.
- Show a consistent empty state when the selected range contains no expense transactions.

## Capabilities

### New Capabilities
- `expense-category-report`: A dedicated tab presents an expense-only category report with range filters, period navigation, donut visualization, and ranked category summaries.

### Modified Capabilities

## Impact

- Affected code: `AccountingApp/AppShell.xaml`, new category report view and view-model files, `AccountingApp/Services/StatisticsService.cs`, and supporting tests
- Affected UX: bottom navigation gains a sixth tab for category reporting
- No new external dependencies or database schema changes
