## Context

The app already has a statistics tab with a monthly category pie chart and multi-month trend charts, but it does not have a standalone category-first report experience. The requested change adds a new bottom tab that emphasizes expense composition for a selected range and period using a layout closer to the provided mobile mockup.

This change crosses shell navigation, statistics aggregation, and a new page/view-model pair. It also introduces a broader period model than the existing statistics page, because the report must support week, month, year, and all-time views with period navigation where applicable.

## Goals / Non-Goals

**Goals:**
- Add a sixth `分類報告` tab while preserving the existing `統計` tab.
- Build a dedicated page for expense-only category reporting with a donut chart and ranked list.
- Support fixed `week`, `month`, `year`, and `all` filters.
- Support previous/next period navigation for week, month, and year ranges.
- Reuse the app's base-currency conversion so report totals match other financial views.

**Non-Goals:**
- Replacing or redesigning the existing statistics tab.
- Adding account-level filtering.
- Including income categories in this report.
- Adding category drill-down, editing, or export behavior in the first version.

## Decisions

### Create a dedicated category report page and view-model
The report will use a new `CategoryReportPage` and `CategoryReportViewModel` instead of extending `StatisticsPage` and `StatisticsViewModel`. This keeps the existing statistics flow stable and prevents period-range logic for week/year/all from leaking into the current statistics page.

Alternative considered: add another section inside the existing statistics page. Rejected because the new screen has different interaction patterns, a different visual hierarchy, and needs its own tab anyway.

### Add report-specific aggregation through the statistics service layer
The report data will be loaded through a report-focused method on `StatisticsService`, returning one normalized summary object per expense category with amount and transaction count. The service will accept a range mode and anchor date, compute the effective date window, filter to `expense` transactions, convert each amount into the configured base currency, and aggregate the final results for the page.

Alternative considered: compute the report directly inside the view-model from raw transactions. Rejected because aggregation, filtering, and currency conversion belong in the service layer and should remain unit-testable without UI state.

### Use a simple period model with an anchor date
The view-model will keep two pieces of state: selected range mode and current anchor date. The service will derive the actual date window from those values:
- `week`: start of week through end of week around the anchor date
- `month`: first through last day of the anchor month
- `year`: first through last day of the anchor year
- `all`: no date bounds

The previous/next controls will adjust the anchor date by one week, one month, or one year depending on the active range. `all` will disable period stepping.

Alternative considered: store an explicit start/end range in the view-model. Rejected because it complicates navigation and duplicates range-calculation logic.

### Keep the visual structure close to the mockup while reusing existing MAUI chart patterns
The page will use a top filter section, a donut chart summary card, and a ranked list card. The donut chart will reuse the existing LiveCharts pie-series approach already present in the statistics page so colors, slice behavior, and chart setup remain consistent with the rest of the app.

Alternative considered: build a custom-drawn ring summary instead of LiveCharts. Rejected because it adds unnecessary rendering complexity for the first version.

## Risks / Trade-offs

- [Week boundary expectations can vary by locale] -> Define one consistent week start in the service and cover it with tests.
- [A large number of categories can crowd the list and chart colors] -> Show all non-zero categories for correctness, but keep list row density compact and reuse a stable color cycle.
- [Range switching adds more asynchronous reload states] -> Keep one load path in the view-model and gate stale responses with the existing version-token pattern used in other statistics flows.

## Migration Plan

No data migration is required. Deploy by adding the new tab, report page, service aggregation, and tests together. Rollback is a direct code revert that removes the new shell entry and report files.

## Open Questions

None.
