## 1. Dynamic axis scale helper

- [ ] 1.1 Add a shared helper that derives a nice Y-axis step from a chart's visible value range.
- [ ] 1.2 Cover small and large value ranges with tests for `1k / 2k / 5k / 10k / 20k / 50k` style step selection.

## 2. Statistics page integration

- [ ] 2.1 Replace the fixed `50_000` Y-axis step in the main income/expense trend chart with the dynamic helper.
- [ ] 2.2 Replace the fixed `50_000` Y-axis step in the category trend chart with the same dynamic helper.
- [ ] 2.3 Preserve existing axis label formatting while applying the new step logic.

## 3. Validation

- [ ] 3.1 Update statistics axis tests so they verify dynamic scale usage instead of hard-coded `50_000`.
- [ ] 3.2 Add focused tests for representative low-range and high-range datasets.
- [ ] 3.3 Run `dotnet test /Users/austin/repository/accounting-app/AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal`.
