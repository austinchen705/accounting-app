## 1. Drill-down data query

- [ ] 1.1 Add a statistics service query that returns expense transactions for one category within the current report range and anchor period.
- [ ] 1.2 Reuse the same range and period filtering rules as the category report summary so totals and detail rows stay consistent.
- [ ] 1.3 Add tests covering category-specific filtering, expense-only behavior, and date ordering inputs.

## 2. Navigation and detail page

- [ ] 2.1 Make category rows on the category report page selectable and trigger drill-down navigation.
- [ ] 2.2 Add a new category transaction detail page and view model.
- [ ] 2.3 Pass the selected category and current report filter context into the detail page without losing the parent page state.

## 3. Date-grouped transaction detail UI

- [ ] 3.1 Build a grouped detail view model shape for date sections and transaction rows.
- [ ] 3.2 Render the detail page with date groups ordered newest to oldest and transactions ordered newest to oldest within each group.
- [ ] 3.3 Add localized labels and empty-state copy for the detail page.

## 4. Validation

- [ ] 4.1 Add or update tests for category report row selection and detail-page grouping behavior.
- [ ] 4.2 Add or update layout tests for the new category transaction detail page.
- [ ] 4.3 Run `dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal`.
