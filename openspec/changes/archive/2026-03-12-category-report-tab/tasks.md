## 1. Navigation and report scaffolding

- [x] 1.1 Add a sixth `分類報告` shell tab that routes to a new `CategoryReportPage`
- [x] 1.2 Create `CategoryReportPage` and `CategoryReportViewModel` with range selection, period label, and previous/next commands

## 2. Expense category report aggregation

- [x] 2.1 Add a statistics service method that builds expense category summaries for week, month, year, and all-time ranges
- [x] 2.2 Add automated tests for date-window selection, expense-only filtering, base-currency aggregation, and stable category ordering

## 3. Report UI composition

- [x] 3.1 Bind the donut chart summary to the new report view-model and show the total expense in the chart center
- [x] 3.2 Add the ranked category list with transaction count, amount, percentage, and empty-state handling

## 4. Verification

- [x] 4.1 Add or update tests that verify the new report page wiring and key copy in XAML
- [x] 4.2 Run the relevant automated tests and build, then manually verify week, month, year, and all filters in the app
