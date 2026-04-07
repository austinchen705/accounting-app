## 1. Frequent category ranking

- [x] 1.1 Add transaction-form logic that computes per-type category usage counts from existing transaction history.
- [x] 1.2 Sort categories by usage count descending and category name ascending for ties.
- [x] 1.3 Limit the frequent category result set to the top 6 categories for the current transaction type.

## 2. Transaction form UI integration

- [ ] 2.1 Add a frequent-category section above the existing full category picker in `TransactionFormPage`.
- [ ] 2.2 Render frequent categories as directly tappable chips that share the same selected category state as the picker.
- [ ] 2.3 Preserve correct selection when editing an existing transaction, including cases where the saved category is not in the top 6.

## 3. Validation

- [x] 3.1 Add or update view model tests for type-specific ranking, chip selection, and edit-mode category preservation.
- [ ] 3.2 Add or update layout tests so the transaction form requires both a frequent-category section and the fallback picker.
- [ ] 3.3 Run `dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal`.
