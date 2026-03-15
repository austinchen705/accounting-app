## 1. Home ViewModel month switching

- [ ] 1.1 Add `PreviousMonthCommand` and `NextMonthCommand` to `HomeViewModel` and update `CurrentMonth` by one calendar month per tap.
- [ ] 1.2 Add a display-friendly month label property for UI binding (e.g. `yyyy/MM`) and ensure property notifications fire when month changes.
- [ ] 1.3 Update `LoadAsync` to query monthly totals and monthly recent records based on selected month.

## 2. Home page UI updates

- [ ] 2.1 Add month navigation controls in `HomePage.xaml` above summary totals (previous button, month label, next button).
- [ ] 2.2 Bind the summary title and empty state copy to selected-month semantics (no fixed "本月" wording).
- [ ] 2.3 Keep existing quick-add button behavior unchanged.

## 3. Validation and regression checks

- [ ] 3.1 Add or update layout tests to verify month navigation controls and bindings exist on Home page.
- [ ] 3.2 Add or update ViewModel tests (or equivalent focused tests) to verify month changes trigger data reload for selected month.
- [ ] 3.3 Run focused tests and `dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64`.
